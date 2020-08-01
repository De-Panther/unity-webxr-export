using UnityEngine;
using WebXR;

public class SceneHitTest : MonoBehaviour
{
  public GameObject visual;

  private bool isFollowing = false;

  private Vector3 originPosition;
  private Quaternion originRotation;

  private Transform arCameraTransform;

  void OnEnable()
  {
    isFollowing = false;
    visual.SetActive(false);
    originPosition = WebXRManager.Instance.transform.localPosition;
    originRotation = WebXRManager.Instance.transform.localRotation;
    WebXRManager.Instance.OnXRChange += HandleOnXRChange;
    arCameraTransform = FindObjectOfType<WebXRCamera>().GetCamera(WebXRCamera.CameraID.LeftAR).transform;
  }

  void OnDisable()
  {
    WebXRManager.Instance.OnXRChange -= HandleOnXRChange;
    WebXRManager.Instance.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
  }

  void Update()
  {
    if (isFollowing && Input.GetMouseButtonDown(0))
    {
      isFollowing = false;
      visual.SetActive(false);
      WebXRManager.Instance.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
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
      WebXRManager.Instance.OnViewerHitTestUpdate += HandleOnViewerHitTestUpdate;
      WebXRManager.Instance.StartViewerHitTest();
    }
    else
    {
      WebXRManager.Instance.OnViewerHitTestUpdate -= HandleOnViewerHitTestUpdate;
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
    WebXRManager.Instance.transform.localPosition = rotationOffset * (originPosition-hitPoseData.position);
    WebXRManager.Instance.transform.localRotation = rotationOffset;
  }

  void FollowByViewRotation(WebXRHitPoseData hitPoseData)
  {
    Vector2 diff = new Vector2(hitPoseData.position.x, hitPoseData.position.z) - new Vector2(arCameraTransform.localPosition.x, arCameraTransform.localPosition.z);
    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90f;
    Quaternion rotationOffset = Quaternion.Euler(0, angle, 0);
    WebXRManager.Instance.transform.localPosition = rotationOffset * (originPosition-hitPoseData.position);
    WebXRManager.Instance.transform.localRotation = rotationOffset;
  }
}
