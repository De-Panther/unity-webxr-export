using UnityEngine;

public class WebXRCamera : MonoBehaviour
{
  private Camera cameraMain, cameraL, cameraR, cameraAR;
  private WebXRState xrState = WebXRState.NORMAL;

  void OnEnable()
  {
    WebXRManager.Instance.OnXRChange += onXRChange;
    WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;

    cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
    cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
    cameraR = GameObject.Find("CameraR").GetComponent<Camera>();
    cameraAR = GameObject.Find("CameraAR").GetComponent<Camera>();
  }

  void Update()
  {
    switch (xrState)
    {
      case WebXRState.AR:
        cameraMain.enabled = false;
        cameraL.enabled = false;
        cameraR.enabled = false;
        cameraAR.enabled = true;
        break;
      case WebXRState.VR:
        cameraMain.enabled = false;
        cameraL.enabled = true;
        cameraR.enabled = true;
        cameraAR.enabled = false;
        break;
      case WebXRState.NORMAL:
        cameraMain.enabled = true;
        cameraL.enabled = false;
        cameraR.enabled = false;
        cameraAR.enabled = false;
        break;
    }
  }

  private void onXRChange(WebXRState state)
  {
    xrState = state;
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
      WebXRMatrixUtil.SetTransformFromViewMatrix(cameraAR.transform, leftViewMatrix * sitStandMatrix.inverse);
      cameraAR.projectionMatrix = leftProjectionMatrix;
    }
  }
}