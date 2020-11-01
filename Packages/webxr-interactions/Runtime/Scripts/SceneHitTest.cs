using UnityEngine;

namespace WebXR.Interactions
{
  public class SceneHitTest : MonoBehaviour
  {
    public Transform originTransform;
    public GameObject visual;

    private bool isFollowing = false;

    private Vector3 originPosition;
    private Quaternion originRotation;

    private Transform arCameraTransform;

    void OnEnable()
    {
      isFollowing = false;
      visual.SetActive(false);
      originPosition = originTransform.localPosition;
      originRotation = originTransform.localRotation;
      WebXRManager.OnXRChange += HandleOnXRChange;
      arCameraTransform = FindObjectOfType<WebXRCamera>().GetCamera(WebXRCamera.CameraID.LeftAR).transform;
    }

    void OnDisable()
    {
      WebXRManager.OnXRChange -= HandleOnXRChange;
      WebXRManager.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
    }

    void Update()
    {
      if (isFollowing && Input.GetMouseButtonDown(0))
      {
        isFollowing = false;
        visual.SetActive(false);
        WebXRManager.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
        WebXRManager.Instance.StopViewerHitTest();
      }
    }

    private void HandleOnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      WebXRManager.Instance.transform.localPosition = originPosition;
      WebXRManager.Instance.transform.localRotation = originRotation;
      isFollowing = false;
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
        transform.localPosition = hitPoseData.position;
        transform.localRotation = hitPoseData.rotation;
        FollowByViewRotation(hitPoseData);
      }
    }

    void FollowByHitRotation(WebXRHitPoseData hitPoseData)
    {
      Quaternion rotationOffset = Quaternion.Inverse(hitPoseData.rotation);
      WebXRManager.Instance.transform.localPosition = rotationOffset * (originPosition - hitPoseData.position);
      WebXRManager.Instance.transform.localRotation = rotationOffset;
    }

    void FollowByViewRotation(WebXRHitPoseData hitPoseData)
    {
      Vector2 diff = new Vector2(hitPoseData.position.x, hitPoseData.position.z) - new Vector2(arCameraTransform.localPosition.x, arCameraTransform.localPosition.z);
      float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90f;
      Quaternion rotationOffset = Quaternion.Euler(0, angle, 0);
      WebXRManager.Instance.transform.localPosition = rotationOffset * (originPosition - hitPoseData.position);
      WebXRManager.Instance.transform.localRotation = rotationOffset;
    }
  }
}
