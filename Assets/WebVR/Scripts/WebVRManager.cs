using UnityEngine;
using UnityEngine.XR;
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
    [Tooltip("Preserve the manager across scenes changes.")]
    public bool dontDestroyOnLoad = true;
    [Header("Tracking")]
    [Tooltip("Default height of camera if no room-scale transform is present.")]
    public float DefaultHeight = 1.2f;
    [Tooltip("Represents the size of physical space available for XR.")]
    public TrackingSpaceType TrackingSpace = TrackingSpaceType.RoomScale;

    private static string GlobalName = "WebVRCameraSet";
    private static WebVRManager instance;
    [HideInInspector]
    public WebVRState vrState = WebVRState.NORMAL;

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
        WebVRControllerButton[] buttons,
        float[] axes);
    public event ControllerUpdate OnControllerUpdate;

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

    private WebVRDisplayCapabilities capabilities;

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
        if (XRDevice.isPresent)
        {
            XRDevice.SetTrackingSpaceType(WebVRManager.Instance.TrackingSpace);
            Debug.Log("Tracking Space: " + XRDevice.GetTrackingSpaceType());
        }
    }

    // Handles WebVR data from browser
    public void OnWebVRData (string jsonString)
    {
        WebVRData webVRData = WebVRData.CreateFromJSON (jsonString);

        // Update controllers
        if (webVRData.controllers.Length > 0)
        {
            foreach (WebVRControllerData controllerData in webVRData.controllers)
            {
                if (OnControllerUpdate != null)
                    OnControllerUpdate(controllerData.id,
                        controllerData.index,
                        controllerData.hand,
                        controllerData.hasOrientation,
                        controllerData.hasPosition,
                        new Quaternion (controllerData.orientation [0], controllerData.orientation [1], controllerData.orientation [2], controllerData.orientation [3]),
                        new Vector3 (controllerData.position [0], controllerData.position [1], controllerData.position [2]),
                        new Vector3 (controllerData.linearAcceleration [0], controllerData.linearAcceleration [1], controllerData.linearAcceleration [2]),
                        new Vector3 (controllerData.linearVelocity [0], controllerData.linearVelocity [1], controllerData.linearVelocity [2]),
                        controllerData.buttons,
                        controllerData.axes);
            }
        }
    }

    // Handles WebVR capabilities from browser
    public void OnVRCapabilities(string json) {
        WebVRDisplayCapabilities capabilities = JsonUtility.FromJson<WebVRDisplayCapabilities>(json);
        OnVRCapabilities(capabilities);
    }

    public void OnVRCapabilities(WebVRDisplayCapabilities capabilities) {
        #if !UNITY_EDITOR && UNITY_WEBGL
        this.capabilities = capabilities;
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

    float[] GetFromSharedArray(int index)
    {
        float[] newArray = new float[16];
        for (int i = 0; i < newArray.Length; i++) {
            newArray[i] = sharedArray[index * 16 + i];
        }
        return newArray;
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

    void Update()
    {
        #if UNITY_EDITOR || !UNITY_WEBGL
        bool quickToggleEnabled = toggleVRKeyName != null && toggleVRKeyName != "";
        if (quickToggleEnabled && Input.GetKeyUp(toggleVRKeyName))
            toggleVrState();
        #endif
    }

    void LateUpdate()
    {
        if (OnHeadsetUpdate != null && this.vrState == WebVRState.ENABLED) {
            Matrix4x4 leftProjectionMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(0));
            Matrix4x4 rightProjectionMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(1));
            Matrix4x4 leftViewMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(2));
            Matrix4x4 rightViewMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(3));
            Matrix4x4 sitStandMatrix = WebVRMatrixUtil.NumbersToMatrix(GetFromSharedArray(4));
            if (!this.capabilities.hasPosition)
            {
                sitStandMatrix = Matrix4x4.Translate(new Vector3(0, this.DefaultHeight, 0));
            }

            OnHeadsetUpdate(
                leftProjectionMatrix,
                rightProjectionMatrix,
                leftViewMatrix,
                rightViewMatrix,
                sitStandMatrix);
         }
    }
}
