using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public enum VrState { ENABLED, NORMAL }

public delegate void OnVrStateChangeHandler();

public class WebVRManager : MonoBehaviour {
    public static WebVRManager instance;
	public VrState vrState;
	public event OnVrStateChangeHandler OnVrStateChange;

	[Tooltip("Name of the key used to alternate between VR and normal mode. Leave blank to disable.")]
	public string toggleVRKeyName;

	void Awake() {
		if (instance == null) {
        	instance = this;
			DontDestroyOnLoad(gameObject);
		}
    }
	
	void Start() {
		SetVrState(VrState.NORMAL);

		#if !UNITY_EDITOR && UNITY_WEBGL
		ConfigureToggleVRKeyName(toggleVRKeyName);
		#endif
	}

	void Update() {
		#if UNITY_EDITOR || !UNITY_WEBGL
		bool quickToggleEnabled = toggleVRKeyName != null && toggleVRKeyName != "";
		if (quickToggleEnabled) {
			if (Input.GetKeyUp(toggleVRKeyName)) {
				toggleVrState();
			}
		}
		#endif
	}

	[DllImport("__Internal")]
	private static extern void ConfigureToggleVRKeyName(string keyName);
	
	[System.Serializable]
	private class Controller
	{
		public int index;
		public string hand;
		public float[] orientation;
		public float[] position;
	}

    [System.Serializable]
	private class VRData
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

	public class Hmd {
		public Matrix4x4 leftViewMatrix;
		public Matrix4x4 leftProjectionMatrix;
		public Matrix4x4 rightViewMatrix;
		public Matrix4x4 rightProjectionMatrix;
		public Matrix4x4 sitStand;

		public Hmd() {
			leftViewMatrix = Matrix4x4.identity;
			leftProjectionMatrix = Matrix4x4.identity;
			rightViewMatrix = Matrix4x4.identity;
			rightProjectionMatrix = Matrix4x4.identity;
			sitStand = Matrix4x4.identity;
		}
	}
	
    public Hmd hmd = new Hmd();

	// left and right hand position and rotation
	Vector3 lhp;
	Vector3 rhp;
	Quaternion lhr;
	Quaternion rhr;

	// sit stand room transform
	//Matrix4x4 sitStand = Matrix4x4.Translate (new Vector3 (0, 1.2f, 0));
	// Matrix4x4 sitStand = Matrix4x4.identity;

	VRData data;

    // WebVR data passed from browser
    public void WebVRData (string jsonString) {
        data = VRData.CreateFromJSON (jsonString);

        // left projection matrix
		hmd.leftProjectionMatrix = numbersToMatrix (data.leftProjectionMatrix);
	
		// left view matrix
		hmd.leftViewMatrix = numbersToMatrix(data.leftViewMatrix);

		// right projection matrix
		hmd.rightProjectionMatrix = numbersToMatrix (data.rightProjectionMatrix);

		// right view matrix
		hmd.rightViewMatrix = numbersToMatrix (data.rightViewMatrix);

		// sit stand matrix
		if (data.sitStand.Length > 0) {
			hmd.sitStand = numbersToMatrix (data.sitStand);
		}

		// controllers
		// if (data.controllers.Length > 0) {
		// 	foreach (Controller control in data.controllers) {
		// 		Vector3 position = new Vector3 (control.position [0], control.position [1], control.position [2]);
		// 		Quaternion rotation = new Quaternion (control.orientation [0], control.orientation [1], control.orientation [2], control.orientation [3]);

		// 		Quaternion sitStandRotation = Quaternion.LookRotation (
		// 			hmd.sitStand.GetColumn (2),
		// 			hmd.sitStand.GetColumn (1)
		// 		);
		// 		Vector3 p = sitStand.MultiplyPoint(position);
		// 		Quaternion r = sitStandRotation * rotation;

		// 		if (control.hand == "left") {
		// 			lhp = p;
		// 			lhr = r;
		// 		}
		// 		if (control.hand == "right") {
		// 			rhp = p;
		// 			rhr = r;
		// 		}
		// 	}
		// }
    }

	public void toggleVrState() {
		#if UNITY_EDITOR || !UNITY_WEBGL
		if (this.vrState == VrState.ENABLED) {
			SetVrState(VrState.NORMAL);
		} else {
			SetVrState(VrState.ENABLED);
		}
		#endif	
	}

	public void SetVrState(VrState state) {
		this.vrState = state;
		OnVrStateChange();
	}

	// received enter VR from WebVR browser
	public void EnterVR()
	{
		SetVrState(VrState.ENABLED);
	}

	// receive exit VR from WebVR browser
	public void ExitVR()
	{
		SetVrState(VrState.NORMAL);
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