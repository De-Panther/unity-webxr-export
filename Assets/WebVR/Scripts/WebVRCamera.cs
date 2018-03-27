using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
	WebVRManager webVRManager;

	Camera cameraMain, cameraL, cameraR;

	bool vrActive = false;

	[DllImport("__Internal")]
	private static extern void PostRender();

	private IEnumerator endOfFrame()
	{
		// wait until end of frame to report back to WebVR browser to submit frame.
		yield return new WaitForEndOfFrame();
		PostRender ();
	}

	void Awake()
	{
		webVRManager = WebVRManager.Instance;
	}

	void Start()
	{
		WebVRManager.OnVrStateChange += handleVrStateChange;
		cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
		cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
		cameraR = GameObject.Find("CameraR").GetComponent<Camera>();
	}

	void Update()
	{
		Matrix4x4 sitStand = webVRManager.stageParameters.SitStand;

		if (vrActive)
		{
			cameraMain.enabled = false;
			cameraL.enabled = true;
			cameraR.enabled = true;

			Matrix4x4 clp = webVRManager.headset.LeftProjectionMatrix;
			Matrix4x4 clv = webVRManager.headset.LeftViewMatrix;
			Matrix4x4 crp = webVRManager.headset.RightProjectionMatrix;
			Matrix4x4 crv = webVRManager.headset.RightViewMatrix;

			SetTransformFromViewMatrix (cameraL.transform, clv * sitStand.inverse);
			cameraL.projectionMatrix = clp;
			SetTransformFromViewMatrix (cameraR.transform, crv * sitStand.inverse);
			cameraR.projectionMatrix = crp;
			SetHeadTransform ();
		}
		else
		{
			// polyfill handles mouse look, so we apply left view to cameraMain so we can look around.
			// will discontinue with https://github.com/mozilla/unity-webvr-export/issues/125 and implement
			// behavior within a component in Unity.

			cameraMain.enabled = true;
			cameraL.enabled = false;
			cameraR.enabled = false;

			Matrix4x4 clv = webVRManager.headset.LeftViewMatrix;
			cameraMain.worldToCameraMatrix = clv * sitStand.inverse * transform.worldToLocalMatrix;
		}

		#if !UNITY_EDITOR && UNITY_WEBGL
		StartCoroutine(endOfFrame());
		#endif
	}

	private void handleVrStateChange()
	{
		vrActive = webVRManager.vrState == VrState.ENABLED;
	}

	// According to https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
	private void SetTransformFromViewMatrix(Transform transform, Matrix4x4 webVRViewMatrix)
	{
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
	private Matrix4x4 TransformViewMatrixToTRS(Matrix4x4 openGLViewMatrix)
	{
		openGLViewMatrix.m20 *= -1;
		openGLViewMatrix.m21 *= -1;
		openGLViewMatrix.m22 *= -1;
		openGLViewMatrix.m23 *= -1;
		return openGLViewMatrix.inverse;
	}

	private void SetHeadTransform()
	{
		Transform leftTransform = cameraL.transform;
		Transform rightTransform = cameraR.transform;
		cameraMain.transform.localPosition =
			(rightTransform.localPosition - leftTransform.localPosition) / 2f + leftTransform.localPosition;
		cameraMain.transform.localRotation = leftTransform.localRotation;
		cameraMain.transform.localScale = leftTransform.localScale;
	}
}