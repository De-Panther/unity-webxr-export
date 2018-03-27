using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public enum VrState { ENABLED, NORMAL }

public class WebVRManager : MonoBehaviour {
	public VrState vrState;
    public static WebVRManager instance;
	public delegate void VrStateChange();
	public static event VrStateChange OnVrStateChange;
	public WebVRControllerManager controllerManager;
	public Headset headset = new Headset();
	public StageParameters stageParameters = new StageParameters();

	[Tooltip("Name of the key used to alternate between VR and normal mode. Leave blank to disable.")]
	public string toggleVRKeyName;

	public class StageParameters
	{
		public Matrix4x4 SitStand = Matrix4x4.identity;
	}

	public class Headset 
	{
		public Matrix4x4 LeftViewMatrix = Matrix4x4.identity;
		public Matrix4x4 LeftProjectionMatrix = Matrix4x4.identity;
		public Matrix4x4 RightViewMatrix = Matrix4x4.identity;
		public Matrix4x4 RightProjectionMatrix = Matrix4x4.identity;
	}

	public static WebVRManager Instance {
		get {
			if (instance == null) {
				GameObject go = new GameObject("WebVRManager");
				go.AddComponent<WebVRManager>();
			}
			return instance;
		}
	}

	// Handles WebVR data from browser
	public void WebVRData (string jsonString) {
		wvrData = WVRData.CreateFromJSON (jsonString);

        headset.LeftProjectionMatrix = numbersToMatrix (wvrData.leftProjectionMatrix);
		headset.LeftViewMatrix = numbersToMatrix(wvrData.leftViewMatrix);
		headset.RightProjectionMatrix = numbersToMatrix (wvrData.rightProjectionMatrix);
		headset.RightViewMatrix = numbersToMatrix (wvrData.rightViewMatrix);

		if (wvrData.sitStand.Length > 0) {
			stageParameters.SitStand = numbersToMatrix (wvrData.sitStand);
		}

		if (wvrData.controllers.Length > 0) {
			foreach (WVRController controller in wvrData.controllers) {
				Vector3 position = new Vector3 (controller.position [0], controller.position [1], controller.position [2]);
				Quaternion rotation = new Quaternion (controller.orientation [0], controller.orientation [1], controller.orientation [2], controller.orientation [3]);
				
				controllerManager.AddOrUpdate(controller.index, controller.hand, position, rotation);
				
				// track of active controllers
				// if (!activeControllers.Contains(controller.index)) {
				// 	activeControllers.Add(controller.index);
				// }
			}
		}
    }

	public void toggleVrState() {
		#if UNITY_EDITOR || !UNITY_WEBGL
		if (this.vrState == VrState.ENABLED) {
			setVrState(VrState.NORMAL);
		} else {
			setVrState(VrState.ENABLED);
		}
		#endif	
	}

	public void setVrState(VrState state) {
		this.vrState = state;
		if (OnVrStateChange != null) {
			OnVrStateChange();
		}
	}

	// received enter VR from WebVR browser
	public void EnterVR()
	{
		setVrState(VrState.ENABLED);
	}

	// receive exit VR from WebVR browser
	public void ExitVR()
	{
		setVrState(VrState.NORMAL);
	}

	// Latency test from browser
	public void TestTime() {
		Debug.Log ("Time tester received in Unity");
		TestTimeReturn ();
	}

	// Toggles performance HUD
	public void TogglePerf() {
		showPerf = showPerf == false ? true : false;
	}

	// link WebGL plugin for interacting with browser scripts.
	[DllImport("__Internal")]
	private static extern void ConfigureToggleVRKeyName(string keyName);

	[DllImport("__Internal")]
	private static extern void TestTimeReturn();

	// delta time for latency checker.
	private float deltaTime = 0.0f;

	// show framerate UI
	private bool showPerf = false;

	// Handles WebVR data passed from browser
	//private List<int> activeControllers = new List<int>();
	private WVRData wvrData;
	
	// Data classes for WebVR data
	[System.Serializable]
	private class WVRController
	{
		public int index = 0;
		public string hand = null;
		public float[] orientation = null;
		public float[] position = null;
	}

	[System.Serializable]
	private class WVRData
	{
		public float[] leftProjectionMatrix = null;
		public float[] rightProjectionMatrix = null;
		public float[] leftViewMatrix = null;	
		public float[] rightViewMatrix = null;
		public float[] sitStand = null;
		public WVRController[] controllers = new WVRController[0];
		public static WVRData CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<WVRData> (jsonString);
		}
	}
	
	void Awake() {
		instance = this;
		controllerManager = WebVRControllerManager.Instance;
		setVrState(VrState.NORMAL);
    }
	
	void Start() {
		#if !UNITY_EDITOR && UNITY_WEBGL
		ConfigureToggleVRKeyName(toggleVRKeyName);
		#endif
	}

	void Update() {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

		#if UNITY_EDITOR || !UNITY_WEBGL
		bool quickToggleEnabled = toggleVRKeyName != null && toggleVRKeyName != "";
		if (quickToggleEnabled) {
			if (Input.GetKeyUp(toggleVRKeyName)) {
				toggleVrState();
			}
		}
		#endif
	}

	// void OnGUI()
	// {
	// 	if (!showPerf)
	// 		return;
		
	// 	int w = Screen.width, h = Screen.height;

	// 	GUIStyle style = new GUIStyle();

	// 	Rect rect = new Rect(w / 4, h / 2, w, h * 2 / 100);
	// 	style.alignment = TextAnchor.UpperLeft;
	// 	style.fontSize = h * 2 / 100;
	// 	style.normal.textColor = new Color (0.0f, 1.0f, 1.0f, 1.0f);
	// 	float msec = deltaTime * 1000.0f;
	// 	float fps = 1.0f / deltaTime;
	// 	string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
	// 	GUI.Label(rect, text, style);
	// }

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