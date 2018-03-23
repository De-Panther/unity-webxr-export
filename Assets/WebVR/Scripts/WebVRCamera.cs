using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
	[DllImport("__Internal")]
	private static extern void TestTimeReturn();

	[DllImport("__Internal")]
	private static extern void PostRender();

	Camera cameraMain, cameraL, cameraR;

	WebVRManager webVRManager;

	// [Tooltip("GameObject to be controlled by the left hand controller.")]
	// public GameObject leftHandObject;
    
	// [Tooltip("GameObject to be controlled by the right hand controller.")]
	// public GameObject rightHandObject;

	// delta time for latency checker.
	float deltaTime = 0.0f;

	// show framerate UI
	bool showPerf = false;
	
	bool vrActive = false;

	// received time tester from WebVR browser
	public void TestTime() {
		Debug.Log ("Time tester received in Unity");
		TestTimeReturn ();
	}

	public void TogglePerf() {
		showPerf = showPerf == false ? true : false;
	}

	private IEnumerator endOfFrame()
	{
		// wait until end of frame to report back to WebVR browser to submit frame.
		yield return new WaitForEndOfFrame();
		PostRender ();
	}

	void Awake() {
		webVRManager = WebVRManager.instance;
		webVRManager.OnVrStateChange += handleVrStateChange;
	}

	void Start()
	{
		cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
		cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
		cameraR = GameObject.Find("CameraR").GetComponent<Camera>();

		// clp = cameraL.projectionMatrix;
		// crp = cameraR.projectionMatrix;

		// clv = cameraL.worldToCameraMatrix;
		// crv = cameraR.worldToCameraMatrix;
	}

	private void handleVrStateChange()
	{
		vrActive = webVRManager.vrState == VrState.ENABLED;
	}

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

		Matrix4x4 sitStand = webVRManager.hmd.sitStand;
		
		if (vrActive) {
			cameraMain.enabled = false;
			cameraL.enabled = true;
			cameraR.enabled = true;	

			Matrix4x4 clp = webVRManager.hmd.leftProjectionMatrix;
			Matrix4x4 clv = webVRManager.hmd.leftViewMatrix;
			Matrix4x4 crp = webVRManager.hmd.rightProjectionMatrix;
			Matrix4x4 crv = webVRManager.hmd.rightViewMatrix;

			SetTransformFromViewMatrix (cameraL.transform, clv * sitStand.inverse);
			cameraL.projectionMatrix = clp;
			SetTransformFromViewMatrix (cameraR.transform, crv * sitStand.inverse);
			cameraR.projectionMatrix = crp;
			SetHeadTransform ();
		} else {
			// polyfill handles mouse look, so we apply left view to cameraMain so we can look around.
			// will discontinue with https://github.com/mozilla/unity-webvr-export/issues/125 and implement
			// behavior within a component in Unity.
			
			cameraMain.enabled = true;
			cameraL.enabled = false;
			cameraR.enabled = false;

			Matrix4x4 clv = webVRManager.hmd.leftViewMatrix;
			cameraMain.worldToCameraMatrix = clv * sitStand.inverse * transform.worldToLocalMatrix;
		}
		
		// #if UNITY_EDITOR
		// if (leftHandObject) {
		// 	leftHandObject.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
		// 	leftHandObject.transform.position = transform.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
		// }
		// if (rightHandObject) {
		// 	rightHandObject.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
		// 	rightHandObject.transform.position = transform.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
		// }
		// #endif

		#if !UNITY_EDITOR && UNITY_WEBGL
		// if (leftHandObject) {
		// 	leftHandObject.transform.rotation = lhr;
		// 	leftHandObject.transform.position = lhp + transform.position;
		// }
		// if (rightHandObject) {
		// 	rightHandObject.transform.rotation = rhr;
		// 	rightHandObject.transform.position = rhp + transform.position;
		// }

		StartCoroutine(endOfFrame());
		#endif
	}

	// According to https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
	private void SetTransformFromViewMatrix(Transform transform, Matrix4x4 webVRViewMatrix) {
		Matrix4x4 trs = TransformViewMatrixToTRS(webVRViewMatrix);
		transform.localPosition = trs.GetColumn(3);
		transform.localRotation = Quaternion.LookRotation(trs.GetColumn(2), trs.GetColumn(1));
		transform.localScale = new Vector3(
			trs.GetColumn(0).magnitude,
			trs.GetColumn(1).magnitude,
			trs.GetColumn(2).magnitude
        );
    }

	// According to https://forum.unity.com/threads/reproducing-cameras-worldtocameramatrix.365645/#post-2367177
	private Matrix4x4 TransformViewMatrixToTRS(Matrix4x4 openGLViewMatrix) {
		openGLViewMatrix.m20 *= -1;
		openGLViewMatrix.m21 *= -1;
		openGLViewMatrix.m22 *= -1;
		openGLViewMatrix.m23 *= -1;
		return openGLViewMatrix.inverse;
	}

	private void SetHeadTransform() {
		Transform leftTransform = cameraL.transform;
		Transform rightTransform = cameraR.transform;
		cameraMain.transform.localPosition =
			(rightTransform.localPosition - leftTransform.localPosition) / 2f + leftTransform.localPosition;
		cameraMain.transform.localRotation = leftTransform.localRotation;
		cameraMain.transform.localScale = leftTransform.localScale;
	}
	void OnGUI()
	{
		if (!showPerf)
			return;

		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(w / 4, h / 2, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 1.0f, 1.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}