using UnityEngine;

namespace WebXR
{
  public class WebXRCamera : MonoBehaviour
  {
    private Camera cameraMain, cameraL, cameraR, cameraARL, cameraARR;
    private WebXRState xrState = WebXRState.NORMAL;
    private Rect leftRect, rightRect;

    private int viewsCount = 1;

    private bool switched = false;

    void OnEnable()
    {
      WebXRManager.Instance.OnXRChange += onXRChange;
      WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;

      cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
      cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
      cameraR = GameObject.Find("CameraR").GetComponent<Camera>();
      cameraARL = GameObject.Find("CameraARL").GetComponent<Camera>();
      cameraARR = GameObject.Find("CameraARR").GetComponent<Camera>();
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
          cameraR.enabled = viewsCount > 1;
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

    private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      xrState = state;
      this.viewsCount = viewsCount;
      this.leftRect = leftRect;
      this.rightRect = rightRect;
      switched = false;
    }

    private void onHeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix)
    {
      if (xrState == WebXRState.VR)
      {
        WebXRMatrixUtil.SetTransformFromViewMatrix(cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);
        cameraL.projectionMatrix = leftProjectionMatrix;
        WebXRMatrixUtil.SetTransformFromViewMatrix(cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);
        cameraR.projectionMatrix = rightProjectionMatrix;
      }
      else if (xrState == WebXRState.AR)
      {
        WebXRMatrixUtil.SetTransformFromViewMatrix(cameraARL.transform, leftViewMatrix * sitStandMatrix.inverse);
        cameraARL.projectionMatrix = leftProjectionMatrix;
        WebXRMatrixUtil.SetTransformFromViewMatrix(cameraARR.transform, rightViewMatrix * sitStandMatrix.inverse);
        cameraARR.projectionMatrix = rightProjectionMatrix;
      }
    }
  }
}
