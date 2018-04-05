using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
	[Tooltip("Default height of camera if no room-scale transform is present.")]
	public float DefaultHeight = 1.2f;
	private Matrix4x4 sitStand;

	private WebVRManager webVRManager;
	private Camera cameraMain, cameraL, cameraR;
	private bool vrActive = false;

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
		WebVRManager.OnVrChange += handleVrChange;
		WebVRManager.OnHeadsetUpdate += handleHeadsetUpdate;

		cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
		cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
		cameraR = GameObject.Find("CameraR").GetComponent<Camera>();

		cameraMain.transform.Translate(new Vector3(0, DefaultHeight, 0));
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
		StartCoroutine(endOfFrame());
		#endif
	}

	private void handleVrChange()
	{
		vrActive = webVRManager.vrState == VrState.ENABLED;
	}

	private void handleHeadsetUpdate (
		Matrix4x4 leftProjectionMatrix,
		Matrix4x4 leftViewMatrix,
		Matrix4x4 rightProjectionMatrix,
		Matrix4x4 rightViewMatrix,
		Matrix4x4 sitStandMatrix)
	{
		if (vrActive)
		{
			SetTransformFromViewMatrix (cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);
			cameraL.projectionMatrix = leftProjectionMatrix;
			SetTransformFromViewMatrix (cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);
			cameraR.projectionMatrix = rightProjectionMatrix;
			SetHeadTransform ();
		}
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