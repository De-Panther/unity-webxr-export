#if UNITY_WEBGL && !UNITY_EDITOR
using AOT;
using System;
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace WebXR
{
  public enum WebXRState { VR, AR, NORMAL }

  public class WebXRManager : MonoBehaviour
  {
    [Tooltip("Preserve the manager across scenes changes.")]
    public bool dontDestroyOnLoad = true;
    [Header("Tracking")]
    [Tooltip("Default height of camera if no room-scale transform is present.")]
    public float DefaultHeight = 1.2f;

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

    public delegate void ControllerUpdate(WebXRControllerData2 controllerData);
    public event ControllerUpdate OnControllerUpdate;

#if UNITY_WEBGL && !UNITY_EDITOR
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
#endif

    // Shared array which we will load headset data in from webxr.jslib
    // Array stores  5 matrices, each 16 values, stored linearly.
    float[] sharedArray = new float[5 * 16 + 2 * 19];

    private WebXRDisplayCapabilities capabilities = new WebXRDisplayCapabilities();

    public static WebXRManager Instance
    {
      get
      {
        if (instance == null)
        {
          var managerInScene = FindObjectOfType<WebXRManager>();

          if (managerInScene != null)
          {
            instance = managerInScene;
          }
          else
          {
            GameObject go = new GameObject("WebXRCameraSet");
            go.AddComponent<WebXRManager>();
          }
        }
        return instance;
      }
    }

    private void Awake()
    {
      Debug.Log("Active Graphics Tier: " + Graphics.activeTier);
      if (null == instance) {
        instance = this;
      } else if (instance != this) {
        Destroy(gameObject);
      }

      if (instance.dontDestroyOnLoad)
      {
        DontDestroyOnLoad(instance);
      }
      xrState = WebXRState.NORMAL;
    }

    // Handles WebXR data from browser
    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action<string>))]
    #endif
    public static void OnWebXRData(string jsonString)
    {
      //WebXRData webXRData = WebXRData.CreateFromJSON(jsonString);

      // Update controllers
      /*if (webXRData.controllers.Length > 0)
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
      }*/
    }

    // Handles WebXR capabilities from browser
    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action<string>))]
    #endif
    public static void OnXRCapabilities(string json)
    {
      WebXRDisplayCapabilities capabilities = JsonUtility.FromJson<WebXRDisplayCapabilities>(json);
      instance.OnXRCapabilities(capabilities);
    }

    public void OnXRCapabilities(WebXRDisplayCapabilities capabilities)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        this.capabilities = capabilities;
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
    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action<int>))]
    #endif
    public static void OnStartAR(int viewsCount)
    {
      Instance.setXrState(WebXRState.AR, viewsCount);
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action<int>))]
    #endif
    public static void OnStartVR(int viewsCount)
    {
      Instance.setXrState(WebXRState.VR, viewsCount);
    }

    // receive end VR from WebVR browser
    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action))]
    #endif
    public static void OnEndXR()
    {
      Instance.setXrState(WebXRState.NORMAL, 1);
    }

    float[] GetMatrixFromSharedArray(int index)
    {
      float[] newArray = new float[16];
      for (int i = 0; i < newArray.Length; i++)
      {
        newArray[i] = sharedArray[index * 16 + i];
      }
      return newArray;
    }

    WebXRControllerData2 GetGamepadFromSharedArray(int startIndex, int controllerIndex)
    {
      WebXRControllerData2 newControllerData = new WebXRControllerData2();
      int arrayPosition = startIndex + controllerIndex * 19;
      newControllerData.enabled = sharedArray[arrayPosition++] != 0;
      newControllerData.hand = (int)sharedArray[arrayPosition++];
      newControllerData.position = new Vector3(sharedArray[arrayPosition++], sharedArray[arrayPosition++], sharedArray[arrayPosition++]);
      newControllerData.rotation = new Quaternion(sharedArray[arrayPosition++], sharedArray[arrayPosition++], sharedArray[arrayPosition++], sharedArray[arrayPosition++]);
      newControllerData.trigger = sharedArray[arrayPosition++];
      newControllerData.squeeze = sharedArray[arrayPosition++];
      newControllerData.thumbstick = sharedArray[arrayPosition++];
      newControllerData.thumbstickX = sharedArray[arrayPosition++];
      newControllerData.thumbstickY = sharedArray[arrayPosition++];
      newControllerData.touchpad = sharedArray[arrayPosition++];
      newControllerData.touchpadX = sharedArray[arrayPosition++];
      newControllerData.touchpadY = sharedArray[arrayPosition++];
      newControllerData.buttonA = sharedArray[arrayPosition++];
      newControllerData.buttonB = sharedArray[arrayPosition];
      return newControllerData;
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        set_webxr_events(OnStartAR, OnStartVR, OnEndXR, OnXRCapabilities, OnWebXRData);
        InitSharedArray(sharedArray, sharedArray.Length);
        ListenWebXRData();
#endif
    }

    void Update()
    {
      if (OnControllerUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        var controller1 = GetGamepadFromSharedArray(80, 0);
        if (controller1.enabled)
        {
          OnControllerUpdate(controller1);
        }
        var controller2 = GetGamepadFromSharedArray(80, 1);
        if (controller2.enabled)
        {
          OnControllerUpdate(controller2);
        }
      }
    }

    void LateUpdate()
    {
      if (OnHeadsetUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        Matrix4x4 leftProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(0));
        Matrix4x4 rightProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(1));
        Matrix4x4 leftViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(2));
        Matrix4x4 rightViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(3));
        Matrix4x4 sitStandMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(4));
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
