using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;


public class WebVRManager : MonoBehaviour {
	// link WebGL plugin for interacting with browser scripts.
	[DllImport("__Internal")]
	private static extern void ConfigureToggleVRKeyName(string keyName);
	
	public enum VrState { ENABLED, NORMAL }

    public static WebVRManager instance;
	public delegate void VrStateChange();
	public static event VrStateChange OnVrStateChange;

	public Headset headset = new Headset();
	public StageParameters stageParameters = new StageParameters();
	public List<Controller> controllers = new List<Controller>();

	private VRData data;

	[Tooltip("Name of the key used to alternate between VR and normal mode. Leave blank to disable.")]
	public string toggleVRKeyName;

	public static WebVRManager Instance {
		get {
			if (instance == null) {
				GameObject go = new GameObject("WebVRManager");
				go.AddComponent<WebVRManager>();
			}
			return instance;
		}
	}

	void Awake() {
		instance = this;
		setVrState(VrState.NORMAL);
    }
	
	void Start() {
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

	[System.Serializable]
	private class VRController
	{
		public int index = 0;
		public string hand = null;
		public float[] orientation = null;
		public float[] position = null;
	}

	[System.Serializable]
	private class VRData
	{
		public float[] leftProjectionMatrix = null;
		public float[] rightProjectionMatrix = null;
		public float[] leftViewMatrix = null;	
		public float[] rightViewMatrix = null;
		public float[] sitStand = null;
		public VRController[] controllers = new VRController[0];
		public static VRData CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<VRData> (jsonString);
		}
	}

	public class Controller
	{
		public int index;
		public string hand;
		public Vector3 position;
		public Quaternion rotation;
		
		public Controller(int index, string hand, Vector3 position, Quaternion rotation) {
			this.index = index;
			this.hand = hand;
			this.position = position;
			this.rotation = rotation;
		}
	}

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
	
	// left and right hand position and rotation
	Vector3 lhp;
	Vector3 rhp;
	Quaternion lhr;
	Quaternion rhr;

	// sit stand room transform
	//Matrix4x4 sitStand = Matrix4x4.Translate (new Vector3 (0, 1.2f, 0));
	// Matrix4x4 sitStand = Matrix4x4.identity;

    // WebVR data passed from browser
    public void WebVRData (string jsonString) {
        data = VRData.CreateFromJSON (jsonString);

        // headset
		headset.LeftProjectionMatrix = numbersToMatrix (data.leftProjectionMatrix);
		headset.LeftViewMatrix = numbersToMatrix(data.leftViewMatrix);
		headset.RightProjectionMatrix = numbersToMatrix (data.rightProjectionMatrix);
		headset.RightViewMatrix = numbersToMatrix (data.rightViewMatrix);

		// sit stand matrix
		if (data.sitStand.Length > 0) {
			stageParameters.SitStand = numbersToMatrix (data.sitStand);
		}

		if (data.controllers.Length > 0) {
			List<VRController> cList = data.controllers.ToList();

			// remove controllers no longer active.
			foreach (Controller c in controllers) {
				VRController cRemove = cList.Find((x) => x.index == c.index);
				if (cRemove == null) {
					controllers.Remove(controllers.Find((x) => x.index == c.index));
					Debug.Log("Removing index: " + c.index);
					Debug.Log("- Controller Count: " + controllers.Count);
				}

			}

			// add or update controller data
			foreach (VRController control in data.controllers) {
				Vector3 position = new Vector3 (control.position [0], control.position [1], control.position [2]);
				Quaternion rotation = new Quaternion (control.orientation [0], control.orientation [1], control.orientation [2], control.orientation [3]);

				Controller controller = controllers.Find((x) => x.index == control.index);

				if (controller == null) {
					controllers.Add(new Controller(control.index, control.hand, position, rotation));
					Debug.Log("Adding index: " + controller.index);
					Debug.Log("Adding hand: " + controller.hand);
					Debug.Log("+ Controller Count: " + controllers.Count);
				} else {
					controller.position = position;
					controller.rotation = rotation;
				}
				// Quaternion sitStandRotation = Quaternion.LookRotation (
				// 	hmd.sitStand.GetColumn (2),
				// 	hmd.sitStand.GetColumn (1)
				// );
				// Vector3 p = sitStand.MultiplyPoint(position);
				// Quaternion r = sitStandRotation * rotation;

				// if (control.hand == "left") {
				// 	lhp = p;
				// 	lhr = r;
				// }
				// if (control.hand == "right") {
				// 	rhp = p;
				// 	rhr = r;
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