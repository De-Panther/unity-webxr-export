using UnityEngine;

namespace WebXR
{
  public class WebXRCamera : MonoBehaviour
  {
    public enum CameraID
    {
      Main,
      LeftVR,
      RightVR,
      LeftAR,
      RightAR
    }

    [SerializeField]
    private Camera cameraMain = null, cameraL = null, cameraR = null, cameraARL = null, cameraARR = null;

    private WebXRState xrState = WebXRState.NORMAL;
    private Rect leftRect, rightRect;

    private int viewsCount = 1;

    private bool switched = false;

    void OnEnable()
    {
      WebXRManager.OnXRChange += OnXRChange;
      WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;
    }
    
    void OnDisable()
    {
      WebXRManager.OnXRChange -= OnXRChange;
      WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
    }

    void Update()
    {
      if (switched)
      {
        return;
      }
      switched = true;
      switch (xrState)
      {
        case WebXRState.AR:
          cameraMain.enabled = false;
          cameraL.enabled = false;
          cameraR.enabled = false;
          cameraARL.enabled = viewsCount > 0;
          cameraARL.rect = leftRect;
          cameraARR.enabled = viewsCount > 1;
          cameraARR.rect = rightRect;
          break;
        case WebXRState.VR:
          cameraMain.enabled = false;
          cameraL.enabled = viewsCount > 0;
          cameraL.rect = leftRect;
          cameraR.enabled = viewsCount > 1;
          cameraR.rect = rightRect;
          cameraARL.enabled = false;
          cameraARR.enabled = false;
          break;
        case WebXRState.NORMAL:
          cameraMain.enabled = true;
          cameraL.enabled = false;
          cameraR.enabled = false;
          cameraARL.enabled = false;
          cameraARR.enabled = false;
          break;
      }
    }

    public Quaternion GetLocalRotation()
    {
      switch (xrState)
      {
        case WebXRState.AR:
          return cameraARL.transform.localRotation;
        case WebXRState.VR:
          return cameraL.transform.localRotation;
      }
      return cameraMain.transform.localRotation;
    }

    public Vector3 GetLocalPosition()
    {
      switch (xrState)
      {
        case WebXRState.AR:
          if (viewsCount > 1)
          {
            return (cameraARL.transform.localPosition + cameraARR.transform.localPosition) * 0.5f;
          }
          return cameraARL.transform.localPosition;
        case WebXRState.VR:
          return (cameraL.transform.localPosition + cameraR.transform.localPosition) * 0.5f;
      }
      return cameraMain.transform.localPosition;
    }

    public Camera GetCamera(CameraID cameraID)
    {
      switch (cameraID)
      {
        case CameraID.LeftVR:
          return cameraL;
        case CameraID.RightVR:
          return cameraR;
        case CameraID.LeftAR:
          return cameraARL;
        case CameraID.RightAR:
          return cameraARR;
      }
      return cameraMain;
    }

    private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      xrState = state;
      this.viewsCount = viewsCount;
      this.leftRect = leftRect;
      this.rightRect = rightRect;
      switched = false;
    }

    private void OnHeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Quaternion leftRotation,
        Quaternion rightRotation,
        Vector3 leftPosition,
        Vector3 rightPosition)
    {
      if (xrState == WebXRState.VR)
      {
        cameraL.transform.localPosition = leftPosition;
        cameraL.transform.localRotation = leftRotation;
        cameraL.projectionMatrix = leftProjectionMatrix;
        cameraR.transform.localPosition = rightPosition;
        cameraR.transform.localRotation = rightRotation;
        cameraR.projectionMatrix = rightProjectionMatrix;
      }
      else if (xrState == WebXRState.AR)
      {
        cameraARL.transform.localPosition = leftPosition;
        cameraARL.transform.localRotation = leftRotation;
        cameraARL.projectionMatrix = leftProjectionMatrix;
        cameraARR.transform.localPosition = rightPosition;
        cameraARR.transform.localRotation = rightRotation;
        cameraARR.projectionMatrix = rightProjectionMatrix;
      }
    }
  }
}
