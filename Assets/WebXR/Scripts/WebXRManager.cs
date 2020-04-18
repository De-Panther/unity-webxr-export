using AOT;
using UnityEngine;
using UnityEngine.XR;
using System;
using System.Runtime.InteropServices;

namespace WebXR
{
  public enum WebXRState { VR, AR, NORMAL }

  public class WebXRManager : MonoBehaviour
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

    private static string GlobalName = "WebXRCameraSet";
    private static WebXRManager instance;
    [HideInInspector]
    public WebXRState xrState = WebXRState.NORMAL;

    public delegate void XRCapabilitiesUpdate(WebXRDisplayCapabilities capabilities);
    public event XRCapabilitiesUpdate OnXRCapabilitiesUpdate;

    public delegate void XRChange(WebXRState state, int viewsCount);
    public event XRChange OnXRChange;

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
        WebXRControllerButton[] buttons,
        float[] axes);
    public event ControllerUpdate OnControllerUpdate;

    // link WebGL plugin for interacting with browser scripts.
    [DllImport("__Internal")]
    private static extern void ConfigureToggleVRKeyName(string keyName);

    [DllImport("__Internal")]
    private static extern void InitSharedArray(float[] array, int length);

    [DllImport("__Internal")]
    private static extern void ListenWebXRData();

    [DllImport("__Internal")]
    private static extern void set_webxr_events(Action<int> on_start_ar,
                                                Action<int> on_start_vr,
                                                Action on_end_xr,
                                                Action<string> on_xr_capabilities,
                                                Action<string> on_webxr_data);

    // Shared array which we will load headset data in from webxr.jslib
    // Array stores  5 matrices, each 16 values, stored linearly.
    float[] sharedArray = new float[5 * 16];

    private WebXRDisplayCapabilities capabilities;

    public static WebXRManager Instance
    {
      get
      {
        if (instance == null)
        {
          var managerInScene = FindObjectOfType<WebXRManager>();
          var name = GlobalName;

          if (managerInScene != null)
          {
            instance = managerInScene;
            instance.name = name;
          }
          else
          {
            GameObject go = new GameObject(name);
            go.AddComponent<WebXRManager>();
          }
        }
        return instance;
      }
    }

    private void Awake()
    {
      Debug.Log("Active Graphics Tier: " + Graphics.activeTier);
      instance = this;

      if (!GlobalName.Equals(instance.name))
      {
        Debug.LogError("The webxr.jspre script requires the WebXRManager gameobject to be named "
        + GlobalName + " for proper functioning");
      }

      if (instance.dontDestroyOnLoad)
      {
        DontDestroyOnLoad(instance);
      }
      xrState = WebXRState.NORMAL;
    }

    private void SetTrackingSpaceType()
    {
      if (XRDevice.isPresent)
      {
        XRDevice.SetTrackingSpaceType(WebXRManager.Instance.TrackingSpace);
        Debug.Log("Tracking Space: " + XRDevice.GetTrackingSpaceType());
      }
    }

    // Handles WebXR data from browser
    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void OnWebXRData(string jsonString)
    {
      WebXRData webXRData = WebXRData.CreateFromJSON(jsonString);

      // Update controllers
      if (webXRData.controllers.Length > 0)
      {
        foreach (WebXRControllerData controllerData in webXRData.controllers)
        {
          if (instance.OnControllerUpdate != null)
            instance.OnControllerUpdate(controllerData.id,
                controllerData.index,
                controllerData.hand,
                controllerData.hasOrientation,
                controllerData.hasPosition,
                new Quaternion(controllerData.orientation[0], controllerData.orientation[1], controllerData.orientation[2], controllerData.orientation[3]),
                new Vector3(controllerData.position[0], controllerData.position[1], controllerData.position[2]),
                new Vector3(controllerData.linearAcceleration[0], controllerData.linearAcceleration[1], controllerData.linearAcceleration[2]),
                new Vector3(controllerData.linearVelocity[0], controllerData.linearVelocity[1], controllerData.linearVelocity[2]),
                controllerData.buttons,
                controllerData.axes);
        }
      }
    }

    // Handles WebXR capabilities from browser
    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void OnXRCapabilities(string json)
    {
      WebXRDisplayCapabilities capabilities = JsonUtility.FromJson<WebXRDisplayCapabilities>(json);
      instance.OnXRCapabilities(capabilities);
    }

    public void OnXRCapabilities(WebXRDisplayCapabilities capabilities)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        this.capabilities = capabilities;
        if (!capabilities.canPresentVR)
            WebXRUI.displayElementId("novr");
#endif

      if (OnXRCapabilitiesUpdate != null)
        OnXRCapabilitiesUpdate(capabilities);
    }

    public void setXrState(WebXRState state, int viewsCount)
    {
      this.xrState = state;
      if (OnXRChange != null)
        OnXRChange(state, viewsCount);
    }

    // received start VR from WebVR browser
    [MonoPInvokeCallback(typeof(Action<int>))]
    public static void OnStartAR(int viewsCount)
    {
      Instance.setXrState(WebXRState.AR, viewsCount);
    }

    [MonoPInvokeCallback(typeof(Action<int>))]
    public static void OnStartVR(int viewsCount)
    {
      Instance.setXrState(WebXRState.VR, viewsCount);
    }

    // receive end VR from WebVR browser
    [MonoPInvokeCallback(typeof(Action))]
    public static void OnEndXR()
    {
      Instance.setXrState(WebXRState.NORMAL, 1);
    }

    float[] GetFromSharedArray(int index)
    {
      float[] newArray = new float[16];
      for (int i = 0; i < newArray.Length; i++)
      {
        newArray[i] = sharedArray[index * 16 + i];
      }
      return newArray;
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        set_webxr_events(OnStartAR, OnStartVR, OnEndXR, OnXRCapabilities, OnWebXRData);
        ConfigureToggleVRKeyName(toggleVRKeyName);
        InitSharedArray(sharedArray, sharedArray.Length);
        ListenWebXRData();
#endif
      SetTrackingSpaceType();
    }

    void LateUpdate()
    {
      if (OnHeadsetUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        Matrix4x4 leftProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(0));
        Matrix4x4 rightProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(1));
        Matrix4x4 leftViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(2));
        Matrix4x4 rightViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(3));
        Matrix4x4 sitStandMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(4));
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
}
