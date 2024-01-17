using UnityEngine;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using UnityEngine.InputSystem;
#endif

namespace WebXR.Interactions
{
  public class SceneHitTest : MonoBehaviour
  {
    public Transform originTransform;
    public GameObject visual;
    [SerializeField]
    private WebXRController leftController;
    [SerializeField]
    private WebXRController rightController;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
    [SerializeField]
#endif
    private bool useInputSystem = false;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
    [SerializeField]
    private InputActionProperty leftTrigger;
    [SerializeField]
    private InputActionProperty rightTrigger;
    [SerializeField]
    private InputActionProperty click;
#endif

    private bool isFollowing = false;

    private Vector3 originPosition;
    private Quaternion originRotation;

    private Transform arCameraTransform;

    void Start()
    {
      if (useInputSystem)
      {
        return;
      }
      if (leftController == null || rightController == null)
      {
#if UNITY_2023_1_OR_NEWER
        var controllers = FindObjectsByType<WebXRController>(FindObjectsSortMode.None);
#else
        var controllers = FindObjectsOfType<WebXRController>();
#endif
        for (int i = 0; i < controllers.Length; i++)
        {
          if (controllers[i].hand == WebXRControllerHand.LEFT)
          {
            leftController = leftController ?? controllers[i];
          }
          else if (controllers[i].hand == WebXRControllerHand.RIGHT)
          {
            rightController = rightController ?? controllers[i];
          }
          if (leftController != null && rightController != null)
          {
            return;
          }
        }
      }
    }

    void OnEnable()
    {
      isFollowing = false;
      visual.SetActive(false);
      originPosition = originTransform.localPosition;
      originRotation = originTransform.localRotation;
      WebXRManager.OnXRChange += HandleOnXRChange;
#if UNITY_2023_1_OR_NEWER
      arCameraTransform = FindFirstObjectByType<WebXRCamera>()?.GetCamera(WebXRCamera.CameraID.LeftAR).transform;
#else
      arCameraTransform = FindObjectOfType<WebXRCamera>()?.GetCamera(WebXRCamera.CameraID.LeftAR).transform;
#endif
      if (arCameraTransform == null)
      {
        arCameraTransform = Camera.main.transform;
      }
    }

    void OnDisable()
    {
      WebXRManager.OnXRChange -= HandleOnXRChange;
      WebXRManager.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
    }

    bool GetControllersButtonDown()
    {
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
      if (useInputSystem)
      {
        return leftTrigger.action.phase == InputActionPhase.Performed
          || rightTrigger.action.phase == InputActionPhase.Performed;
      }
#endif
      bool leftDown = (leftController.isHandActive || leftController.isControllerActive) && leftController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
      bool rightDown = (rightController.isHandActive || rightController.isControllerActive) && rightController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
      return leftDown || rightDown;
    }

    void Update()
    {
      if (isFollowing && (
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
        (useInputSystem && click.action?.IsPressed() == true) ||
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        Input.GetMouseButtonDown(0) ||
#endif
        GetControllersButtonDown()))
      {
        isFollowing = false;
        visual.SetActive(false);
        WebXRManager.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
        WebXRManager.Instance.StopViewerHitTest();
      }
    }

    private void HandleOnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
#if HAS_POSITION_AND_ROTATION
      originTransform.SetLocalPositionAndRotation(originPosition, originRotation);
#else
      originTransform.localPosition = originPosition;
      originTransform.localRotation = originRotation;
#endif
      isFollowing = false;
      visual.SetActive(false);
      if (state == WebXRState.AR)
      {
        WebXRManager.OnViewerHitTestUpdate += HandleOnViewerHitTestUpdate;
        WebXRManager.Instance.StartViewerHitTest();
      }
      else
      {
        WebXRManager.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
      }
    }

    void HandleOnViewerHitTestUpdate(WebXRHitPoseData hitPoseData)
    {
      visual.SetActive(hitPoseData.available);
      if (hitPoseData.available)
      {
        isFollowing = true;
#if HAS_POSITION_AND_ROTATION
        transform.SetLocalPositionAndRotation(hitPoseData.position, hitPoseData.rotation);
#else
        transform.localPosition = hitPoseData.position;
        transform.localRotation = hitPoseData.rotation;
#endif
        FollowByViewRotation(hitPoseData);
      }
    }

    void FollowByHitRotation(WebXRHitPoseData hitPoseData)
    {
      Quaternion rotationOffset = Quaternion.Inverse(hitPoseData.rotation);
#if HAS_POSITION_AND_ROTATION
      originTransform.SetLocalPositionAndRotation(rotationOffset * (originPosition - hitPoseData.position), rotationOffset);
#else
      originTransform.localPosition = rotationOffset * (originPosition - hitPoseData.position);
      originTransform.localRotation = rotationOffset;
#endif
    }

    void FollowByViewRotation(WebXRHitPoseData hitPoseData)
    {
      Vector2 diff = new Vector2(hitPoseData.position.x, hitPoseData.position.z) - new Vector2(arCameraTransform.localPosition.x, arCameraTransform.localPosition.z);
      float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90f;
      Quaternion rotationOffset = Quaternion.Euler(0, angle, 0);
#if HAS_POSITION_AND_ROTATION
      originTransform.SetLocalPositionAndRotation(rotationOffset * (originPosition - hitPoseData.position), rotationOffset);
#else
      originTransform.localPosition = rotationOffset * (originPosition - hitPoseData.position);
      originTransform.localRotation = rotationOffset;
#endif
    }
  }
}
