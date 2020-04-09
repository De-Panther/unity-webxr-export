using UnityEngine;

public class WebXRCamera : MonoBehaviour
{
    private Camera cameraMain, cameraL, cameraR;
    private bool xrActive = false;

    void OnEnable()
    {
        WebXRManager.Instance.OnXRChange += onXRChange;
        WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;
        
        cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
        cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
        cameraR = GameObject.Find("CameraR").GetComponent<Camera>();
    }

    void Update()
    {
        if (xrActive)
        {
            cameraMain.enabled = false;
            cameraL.enabled = true;
            cameraR.enabled = true;
        }
        else
        {
            cameraMain.enabled = true;
            cameraL.enabled = false;
            cameraR.enabled = false;
        }
    }

    private void onXRChange(WebXRState state)
    {
        xrActive = state == WebXRState.ENABLED;
    }

    private void onHeadsetUpdate (
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix)
    {
        if (xrActive)
        {
            WebXRMatrixUtil.SetTransformFromViewMatrix (cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);
            cameraL.projectionMatrix = leftProjectionMatrix;
            WebXRMatrixUtil.SetTransformFromViewMatrix (cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);
            cameraR.projectionMatrix = rightProjectionMatrix;
        }
    }
}