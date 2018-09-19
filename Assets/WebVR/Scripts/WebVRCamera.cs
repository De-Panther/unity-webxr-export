using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
    private Camera cameraMain, cameraL, cameraR;
    private bool vrActive = false;

    [DllImport("__Internal")]
    private static extern void PostRender();

    private IEnumerator endOfFrame()
    {
        // Wait until end of frame to report back to WebVR browser to submit frame.
        yield return new WaitForEndOfFrame();
        PostRender ();
    }

    void OnEnable()
    {
        WebVRManager.Instance.OnVRChange += onVRChange;
        WebVRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;
        
        cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
        cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
        cameraR = GameObject.Find("CameraR").GetComponent<Camera>();

        cameraMain.transform.Translate(new Vector3(0, WebVRManager.Instance.DefaultHeight, 0));
    }

    void Update()
    {
        if (vrActive)
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

        #if !UNITY_EDITOR && UNITY_WEBGL
        // Calls Javascript to Submit Frame to the browser WebVR API.
        StartCoroutine(endOfFrame());
        #endif
    }

    private void onVRChange(WebVRState state)
    {
        vrActive = state == WebVRState.ENABLED;
    }

    private void onHeadsetUpdate (
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix)
    {
        if (vrActive)
        {
            WebVRMatrixUtil.SetTransformFromViewMatrix (cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);
            cameraL.projectionMatrix = leftProjectionMatrix;
            WebVRMatrixUtil.SetTransformFromViewMatrix (cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);
            cameraR.projectionMatrix = rightProjectionMatrix;
        }
    }
}