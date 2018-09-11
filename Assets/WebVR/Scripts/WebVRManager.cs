using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public enum WebVRState { ENABLED, NORMAL }

public class WebVRManager : MonoBehaviour
{
    private static string GlobalName = "WebVRCameraSet";  
  
    [Tooltip("Name of the key used to alternate between VR and normal mode. Leave blank to disable.")]
    public string toggleVRKeyName;

    [HideInInspector]
    public WebVRState vrState = WebVRState.NORMAL;
    
    private static WebVRManager instance;

    [Tooltip("Preserve the manager across scenes changes.")]
    public bool dontDestroyOnLoad = true;
    
    [Header("Tracking")]

    [Tooltip("Default height of camera if no room-scale transform is present.")]
    public float DefaultHeight = 1.2f;

    [Tooltip("Represents the size of physical space available for XR.")]
    public UnityEngine.XR.TrackingSpaceType TrackingSpace = UnityEngine.XR.TrackingSpaceType.RoomScale;

    public delegate void VRCapabilitiesUpdate(WebVRDisplayCapabilities capabilities);
    public event VRCapabilitiesUpdate OnVRCapabilitiesUpdate;
    
    public delegate void VRChange(WebVRState state);
    public event VRChange OnVRChange;
    
    public delegate void HeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix);
    public event HeadsetUpdate OnHeadsetUpdate;
   
    public delegate void ControllerUpdate(string id,
        int index, 
        string hand,
        bool hasOrientation,
        bool hasPosition,
        Quaternion orientation,
        Vector3 position,
        Vector3 linearAcceleration,
        Vector3 linearVelocity,
        Matrix4x4 sitStand,
        WebVRControllerButton[] buttons,
        float[] axes);
    public event ControllerUpdate OnControllerUpdate;

    public static WebVRManager Instance {
        get
        {
            if (instance == null)
            {
                var managerInScene = FindObjectOfType<WebVRManager>();
                var name = GlobalName;

                if (managerInScene != null)
                {
                    instance = managerInScene;
                    instance.name = name;
                }
                else
                {
                    GameObject go = new GameObject(name);
                    go.AddComponent<WebVRManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        Debug.Log("Active Graphics Tier: " + Graphics.activeTier);
        instance = this;
        
        if(!GlobalName.Equals(instance.name)) {
           Debug.LogError("The webvr.js script requires the WebVRManager gameobject to be named " 
           + GlobalName + " for proper functioning");
        }
                
        if (instance.dontDestroyOnLoad)
        {
            DontDestroyOnLoad(instance);
        }
    }

    private void SetTrackingSpaceType()
    {
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            UnityEngine.XR.XRDevice.SetTrackingSpaceType(WebVRManager.Instance.TrackingSpace);
            Debug.Log("Tracking Space: " + UnityEngine.XR.XRDevice.GetTrackingSpaceType());
        }
    }

    // Handles WebVR data from browser
    public void OnWebVRData (string jsonString)
    {
        WebVRData webVRData = WebVRData.CreateFromJSON (jsonString);

        // Reset RoomScale matrix if we are using Stationary tracking space.
        if (TrackingSpace == UnityEngine.XR.TrackingSpaceType.Stationary)
            sitStand = Matrix4x4.identity;

        // Update controllers
        if (webVRData.controllers.Length > 0)
        {
            foreach (WebVRControllerData controllerData in webVRData.controllers)
            {
                Vector3 position = new Vector3 (controllerData.position [0], controllerData.position [1], controllerData.position [2]);
                Quaternion orientation = new Quaternion (controllerData.orientation [0], controllerData.orientation [1], controllerData.orientation [2], controllerData.orientation [3]);
                Vector3 linearAcceleration = new Vector3 (controllerData.linearAcceleration [0], controllerData.linearAcceleration [1], controllerData.linearAcceleration [2]);
                Vector3 linearVelocity = new Vector3 (controllerData.linearVelocity [0], controllerData.linearVelocity [1], controllerData.linearVelocity [2]);

                if (OnControllerUpdate != null)
                    OnControllerUpdate(controllerData.id,
                        controllerData.index,
                        controllerData.hand,
                        controllerData.hasOrientation,
                        controllerData.hasPosition,
                        orientation,
                        position,
                        linearAcceleration,
                        linearVelocity,
                        sitStand,
                        controllerData.buttons,
                        controllerData.axes);
            }
        }
    }

    // Handles WebVR capabilities from browser
    public void OnVRCapabilities(string json) {
        OnVRCapabilities(JsonUtility.FromJson<WebVRDisplayCapabilities>(json));
    }

    public void OnVRCapabilities(WebVRDisplayCapabilities capabilities) {
        #if !UNITY_EDITOR && UNITY_WEBGL
        if (!capabilities.canPresent)
            WebVRUI.displayElementId("novr");
        #endif

        if (OnVRCapabilitiesUpdate != null)
            OnVRCapabilitiesUpdate(capabilities);
    }

    public void toggleVrState()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        if (this.vrState == WebVRState.ENABLED)
            setVrState(WebVRState.NORMAL);
        else
            setVrState(WebVRState.ENABLED);
        #endif
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
        Instance.setVrState(WebVRState.ENABLED);        
    }

    // receive end VR from WebVR browser
    public void OnEndVR()
    {
        Instance.setVrState(WebVRState.NORMAL);
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
    private static extern void InitSharedArray(float[] array, int length);

    [DllImport("__Internal")]
    private static extern void ListenWebVRData();

    // Shared array which we will load headset data in from webvr.jslib
    // Array stores  5 matrices, each 16 values, stored linearly.
    float[] sharedArray = new float[5 * 16];

    // show framerate UI
    private bool showPerf = false;

    private WebVRData webVRData;
    private Matrix4x4 sitStand = Matrix4x4.identity;

    // Data classes for WebVR data
    [System.Serializable]
    private class WebVRData
    {
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
        public string id = null;
        public int index = 0;
        public string hand = null;
        public bool hasOrientation = false;
        public bool hasPosition = false;
        public float[] orientation = null;
        public float[] position = null;
        public float[] linearAcceleration = null;
        public float[] linearVelocity = null;
        public float[] axes = null;
        public WebVRControllerButton[] buttons = new WebVRControllerButton[0];
    }    

    void Start()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        ConfigureToggleVRKeyName(toggleVRKeyName);
        InitSharedArray(sharedArray, sharedArray.Length);
        ListenWebVRData();
        #endif
        SetTrackingSpaceType();
    }

    float[] GetFromSharedArray(int index)
    {
        float[] newArray = new float[16];
        for (int i = 0; i < newArray.Length; i++) {
            newArray[i] = sharedArray[index * 16 + i];
        }
        return newArray;
    }

    void Update()
    {
        #if UNITY_EDITOR || !UNITY_WEBGL
        bool quickToggleEnabled = toggleVRKeyName != null && toggleVRKeyName != "";
        if (quickToggleEnabled && Input.GetKeyUp(toggleVRKeyName))
            toggleVrState();
        #endif

        if (OnHeadsetUpdate != null) {
            Matrix4x4 leftProjectionMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(0));
            Matrix4x4 rightProjectionMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(1));
            Matrix4x4 leftViewMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(2));
            Matrix4x4 rightViewMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(3));
            Matrix4x4 sitStandMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(4));

            sitStand = sitStandMatrix;

            OnHeadsetUpdate(
                leftProjectionMatrix,
                rightProjectionMatrix,
                leftViewMatrix,
                rightViewMatrix,
                sitStand);
         }
    }
}
