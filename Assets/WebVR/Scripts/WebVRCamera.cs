using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FinishLoading();

	[DllImport("__Internal")]
	private static extern void TestTimeReturn();

	[DllImport("__Internal")]
	private static extern void PostRender();

	Camera cameraMain, cameraL, cameraR;

    Quaternion cq;
    Quaternion lhq;
    Quaternion rhq;
    Vector3 cp;

	// left and right hand position and rotation
	Vector3 lhp;
	Vector3 rhp;
	Quaternion lhr;
	Quaternion rhr;

	// camera view and projection matrices
	Matrix4x4 clp = Matrix4x4.identity; // left projection matrix
	Matrix4x4 clv = Matrix4x4.identity; // left view matrix
	Matrix4x4 crp = Matrix4x4.identity; // right projection matrix
	Matrix4x4 crv = Matrix4x4.identity; // left view matrix
	Matrix4x4 cmv = Matrix4x4.identity; // main 2D camera view matrix

	// sit stand room transform
	//Matrix4x4 sitStand = Matrix4x4.Translate (new Vector3 (0, 1.2f, 0));
	Matrix4x4 sitStand = Matrix4x4.identity;

    bool active = false; // vr mode
    
	public GameObject leftHandObj;
    public GameObject rightHandObj;


	// delta time for latency checker.
	float deltaTime = 0.0f;

	// show framerate UI
	bool showPerf = false;

	[System.Serializable]
	public class Controller
	{
		public int index;
		public string hand;
		public float[] orientation;
		public float[] position;
	}

	[System.Serializable]
	public class VRData
	{
		public float[] id;
		public float[] leftProjectionMatrix;
		public float[] rightProjectionMatrix;
		public float[] leftViewMatrix;
		public float[] rightViewMatrix;
		public float[] sitStand;
		public Controller[] controllers;
		public static VRData CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<VRData> (jsonString);
		}
	}

	// received enter VR from WebVR browser
	public void Begin()
	{
		changeMode("vr");
	}

	// receive exit VR from WebVR browser
	public void End()
	{
		changeMode("normal");
	}

	// receive WebVR data from browser.
	public void WebVRData (string jsonString) {
		VRData data = VRData.CreateFromJSON (jsonString);

		// left projection matrix
		clp = numbersToMatrix (data.leftProjectionMatrix);
	
		// left view matrix
		clv = numbersToMatrix (data.leftViewMatrix);

		// right projection matrix
		crp = numbersToMatrix (data.rightProjectionMatrix);

		// right view matrix
		crv = numbersToMatrix (data.rightViewMatrix);

		// sit stand matrix
		if (data.sitStand.Length > 0) {
			sitStand = numbersToMatrix (data.sitStand);
		}

		// controllers
		if (data.controllers.Length > 0) {
			foreach (Controller control in data.controllers) {
				Vector3 position = new Vector3 (control.position [0], control.position [1], control.position [2]);
				Quaternion rotation = new Quaternion (control.orientation [0], control.orientation [1], control.orientation [2], control.orientation [3]);

				Quaternion sitStandRotation = Quaternion.LookRotation (
					sitStand.GetColumn (2),
					sitStand.GetColumn (1)
				);
				Vector3 p = sitStand.MultiplyPoint(position);
				Quaternion r = sitStandRotation * rotation;

				if (control.hand == "left") {
					lhp = p;
					lhr = r;
				}
				if (control.hand == "right") {
					rhp = p;
					rhr = r;
				}
			}
		}
	}

	// received time tester from WebVR browser
	public void TestTime() {
		Debug.Log ("Time tester received in Unity");
		TestTimeReturn ();
	}

	public void TogglePerf() {
		showPerf = showPerf == false ? true : false;
	}

	private void toggleMode() {
		active = active == true ? false : true;
		string mode = active == true ? "vr" : "normal";
		changeMode (mode);
	}

	private void changeMode(string mode)
	{
		Debug.Log("Switching to " + mode);

		switch (mode)
		{
		case "normal":
			cameraMain.enabled = true;
			cameraL.enabled = false;
			cameraR.enabled = false;
			active = false;
			break;
		case "vr":
			cameraMain.enabled = false;
			cameraL.enabled = true;
			cameraR.enabled = true;
			active = true;
			break;
		}
	}

	private IEnumerator endOfFrame()
	{
		// wait until end of frame to report back to WebVR browser to submit frame.
		yield return new WaitForEndOfFrame();
		PostRender ();
	}

	void Start()
	{
		cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
		cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
		cameraR = GameObject.Find("CameraR").GetComponent<Camera>();

		clp = cameraL.projectionMatrix;
		crp = cameraR.projectionMatrix;

		clv = cameraL.worldToCameraMatrix;
		crv = cameraR.worldToCameraMatrix;

		changeMode("normal");

		#if !UNITY_EDITOR && UNITY_WEBGL
		FinishLoading();
		#endif
	}

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

		if (Input.GetKeyDown("space")) 
		{
			toggleMode ();
		}

		if (leftHandObj) {
			leftHandObj.transform.rotation = lhr;
			leftHandObj.transform.position = lhp;
		}
		if (rightHandObj) {
			rightHandObj.transform.rotation = rhr;
			rightHandObj.transform.position = rhp;
		}

		if (active) {
			cameraL.worldToCameraMatrix = clv * sitStand.inverse;
			cameraL.projectionMatrix = clp;
			cameraR.worldToCameraMatrix = crv * sitStand.inverse;
			cameraR.projectionMatrix = crp;
		} else {
			// apply left 
			cameraMain.worldToCameraMatrix = clv * sitStand.inverse;
		}

		#if !UNITY_EDITOR && UNITY_WEBGL
		StartCoroutine(endOfFrame());
		#endif
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
		
	// Utility functions
	private Matrix4x4 numbersToMatrix(float[] array) {
		var mat = new Matrix4x4 ();
		mat.m00 = array[0];
		mat.m01 = array[1];
		mat.m02 = array[2];
		mat.m03 = array[3];
		mat.m10 = array[4];
		mat.m11 = array[5];
		mat.m12 = array[6];
		mat.m13 = array[7];
		mat.m20 = array[8];
		mat.m21 = array[9];
		mat.m22 = array[10];
		mat.m23 = array[11];
		mat.m30 = array[12];
		mat.m31 = array[13];
		mat.m32 = array[14];
		mat.m33 = array[15];
		return mat;
	}
}