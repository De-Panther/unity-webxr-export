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
	Matrix4x4 clp = Matrix4x4.identity;
	Matrix4x4 clv = Matrix4x4.identity;
	Matrix4x4 crp = Matrix4x4.identity;
	Matrix4x4 crv = Matrix4x4.identity;

	// sit stand room transform
	Matrix4x4 sitStand = Matrix4x4.Translate (new Vector3 (0, 1.2f, 0));

    bool active = false; // vr mode
    
	public GameObject leftHandObj;
    public GameObject rightHandObj;

	public bool handControllers = false;

	// delta time for latency checker.
	float deltaTime = 0.0f;

	// show framerate UI
	bool showPerf = false;

	[System.Serializable]
	public class Gamepads
	{
		public Controller[] controllers;

		public static Gamepads CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<Gamepads>(jsonString);
		}
	}

	[System.Serializable]
	public class Controller
	{
		public int index;
		public string hand;
		public string orientation;
		public string position;

		public static Controller CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<Controller>(jsonString);
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

	// receive view and projection matrices from WebVR browser
	public void HMDViewProjection (string viewProjectionNumbersStr) {
		float[] array = viewProjectionNumbersStr.Split(',').Select(float.Parse).ToArray();
		// left projection matrix
		clp = numbersToMatrix(array.Skip(16 * 0).Take (16).ToArray ());
		// left view matrix
		clv = numbersToMatrix(array.Skip(16 * 1).Take (16).ToArray ());
		// right projection matrix
		crp = numbersToMatrix(array.Skip(16 * 2).Take (16).ToArray ());
		// right view matrix
		crv = numbersToMatrix(array.Skip(16 * 3).Take (16).ToArray ());
	}

	// received sit and stand room transform from WebVR browser
	public void HMDSittingToStandingTransform (string sitStandStr) {
		float[] array = sitStandStr.Split(',').Select(float.Parse).ToArray();
		sitStand = numbersToMatrix (array);
	}

	// receive gamepad data from WebVR browser
	public void VRGamepads (string jsonString) {
		Gamepads list = Gamepads.CreateFromJSON(jsonString);

		handControllers = list.controllers.Length > 0 ? true : false;

		foreach (Controller control in list.controllers) {
			float[] pos = control.position.Split(',').Select(float.Parse).ToArray();
			float[] rot = control.orientation.Split(',').Select(float.Parse).ToArray();
			Vector3 position = new Vector3 (pos [0], pos [1], pos [2]);
			Quaternion rotation = new Quaternion (rot [0], rot [1], rot [2], rot[3]);

			Quaternion sitStandRotation = Quaternion.LookRotation (
				sitStand.GetColumn (2),
				sitStand.GetColumn (1)
			);
			Vector3 p = sitStand.MultiplyPoint(position);
			Quaternion r = rotation * sitStandRotation;

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

		if (active == true) {
			leftHandObj.transform.rotation = lhr;
			leftHandObj.transform.position = lhp;
			rightHandObj.transform.rotation = rhr;
			rightHandObj.transform.position = rhp;

			// apply camera projection and view matrices from webVR api.
			if (!clv.isIdentity || !clp.isIdentity || !crv.isIdentity || !crp.isIdentity) {
				// apply sit stand transform
				clv *= sitStand.inverse;
				crv *= sitStand.inverse;

				cameraL.worldToCameraMatrix = clv;
				cameraL.projectionMatrix = clp;
				cameraR.worldToCameraMatrix = crv;
				cameraR.projectionMatrix = crp;
			}
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