using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public enum WebVRState { ENABLED, NORMAL }

public class WebVRManager : MonoBehaviour
{
    [Tooltip("Name of the key used to alternate between VR and normal mode. Leave blank to disable.")]
    public string toggleVRKeyName;

    [HideInInspector]
    public WebVRState vrState = WebVRState.NORMAL;
    public static WebVRManager instance;
    public delegate void VRCapabilitiesUpdate(WebVRDisplayCapabilities capabilities);
    public static event VRCapabilitiesUpdate OnVRCapabilitiesUpdate;
    public delegate void VRChange(WebVRState state);
    public static event VRChange OnVRChange;
    public delegate void HeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix);
    public static event HeadsetUpdate OnHeadsetUpdate;
    public delegate void ControllerUpdate(int index, string hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand, WebVRControllerButton[] buttons);
    public static event ControllerUpdate OnControllerUpdate;

    public static WebVRManager Instance {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("WebVRManager");
                go.AddComponent<WebVRManager>();
            }
            return instance;
        }
    }

    // Handles WebVR data from browser
    public void OnWebVRData (string jsonString)
    {
        WebVRData webVRData = WebVRData.CreateFromJSON (jsonString);

        if (webVRData.sitStand.Length > 0)
            sitStand = numbersToMatrix (webVRData.sitStand);

        if (OnHeadsetUpdate != null)
            OnHeadsetUpdate(
                numbersToMatrix (webVRData.leftProjectionMatrix),
                numbersToMatrix (webVRData.leftViewMatrix),
                numbersToMatrix (webVRData.rightProjectionMatrix),
                numbersToMatrix (webVRData.rightViewMatrix),
                sitStand);

        if (webVRData.controllers.Length > 0)
        {
            foreach (WebVRControllerData controllerData in webVRData.controllers)
            {
                Vector3 position = new Vector3 (controllerData.position [0], controllerData.position [1], controllerData.position [2]);
                Quaternion rotation = new Quaternion (controllerData.orientation [0], controllerData.orientation [1], controllerData.orientation [2], controllerData.orientation [3]);

                if (OnControllerUpdate != null)
                    OnControllerUpdate(controllerData.index, controllerData.hand, position, rotation, sitStand, controllerData.buttons);
            }
        }
    }

    // Handles WebVR capabilities from browser
    public void OnVRCapabilities(string json) {
        OnVRCapabilities(JsonUtility.FromJson<WebVRDisplayCapabilities>(json));
    }

    public void OnVRCapabilities(WebVRDisplayCapabilities capabilities) {
        #if !UNITY_EDITOR && UNITY_WEBGL
        if (!capabilities.canPresent) {
            WebVRUI.ShowPanel("novr");
        }
        #endif

        if (OnVRCapabilitiesUpdate != null)
        {
            OnVRCapabilitiesUpdate(capabilities);
        }
    }

    public void toggleVrState()
    {
        if (this.vrState == WebVRState.ENABLED)
            setVrState(WebVRState.NORMAL);
        else
            setVrState(WebVRState.ENABLED);
    }

    public void setVrState(WebVRState state)
    {
        this.vrState = state;
        if (OnVRChange != null)
            OnVRChange(state);
    }

    // received start VR from WebVR browser
    public void OnStartVR()
    {
        setVrState(WebVRState.ENABLED);
    }

    // receive end VR from WebVR browser
    public void OnEndVR()
    {
        setVrState(WebVRState.NORMAL);
    }

    // Latency test from browser
    public void TestTime()
    {
        Debug.Log ("Time tester received in Unity");
        TestTimeReturn ();
    }

    // Toggles performance HUD
    public void TogglePerf()
    {
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

    private WebVRData webVRData;
    private Matrix4x4 sitStand = Matrix4x4.identity;

    // Data classes for WebVR data
    [System.Serializable]
    private class WebVRData
    {
        public float[] leftProjectionMatrix = null;
        public float[] rightProjectionMatrix = null;
        public float[] leftViewMatrix = null;
        public float[] rightViewMatrix = null;
        public float[] sitStand = null;
        public WebVRControllerData[] controllers = new WebVRControllerData[0];
        public static WebVRData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<WebVRData> (jsonString);
        }
    }

    [System.Serializable]
    private class WebVRControllerData
    {
        public int index = 0;
        public string hand = null;
        public float[] orientation = null;
        public float[] position = null;
        public WebVRControllerButton[] buttons = new WebVRControllerButton[0];
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        ConfigureToggleVRKeyName(toggleVRKeyName);
        #endif
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        #if UNITY_EDITOR || !UNITY_WEBGL
        bool quickToggleEnabled = toggleVRKeyName != null && toggleVRKeyName != "";
        if (quickToggleEnabled && Input.GetKeyUp(toggleVRKeyName))
            toggleVrState();
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
    private Matrix4x4 numbersToMatrix(float[] array)
    {
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
