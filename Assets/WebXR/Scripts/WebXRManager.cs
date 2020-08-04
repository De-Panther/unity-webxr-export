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

    public delegate void XRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect);
    public event XRChange OnXRChange;

    public delegate void HeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix);
    public event HeadsetUpdate OnHeadsetUpdate;

    public delegate void ControllerUpdate(WebXRControllerData controllerData);
    public event ControllerUpdate OnControllerUpdate;

    public delegate void HandUpdate(WebXRHandData handData);
    public event HandUpdate OnHandUpdate;

    public delegate void HitTestUpdate(WebXRHitPoseData hitPoseData);
    public event HitTestUpdate OnViewerHitTestUpdate;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void InitXRSharedArray(float[] array, int length);

    [DllImport("__Internal")]
    private static extern void InitControllersArray(float[] array, int length);

    [DllImport("__Internal")]
    private static extern void InitHandsArray(float[] array, int length);

    [DllImport("__Internal")]
    private static extern void InitViewerHitTestPoseArray(float[] array, int length);

    [DllImport("__Internal")]
    private static extern void ToggleViewerHitTest();

    [DllImport("__Internal")]
    private static extern void ControllerPulse(int controller, float intensity, float duration);

    [DllImport("__Internal")]
    private static extern void ListenWebXRData();

    [DllImport("__Internal")]
    private static extern void set_webxr_events(Action<int, float, float, float, float, float, float, float, float> on_start_ar,
                                                Action<int> on_start_vr,
                                                Action on_end_xr,
                                                Action<string> on_xr_capabilities);
#endif

    // Shared array which we will load headset data in from webxr.jslib
    // Array stores  5 matrices, each 16 values, stored linearly.
    float[] sharedArray = new float[5 * 16];

    // Shared array for controllers data
    float[] controllersArray = new float[2 * 20];

    // Shared array for hands data
    float[] handsArray = new float[2 * (25 * 9 + 5)];

    // Shared array for hit-test pose data
    float[] viewerHitTestPoseArray = new float[9];

    bool viewerHitTestOn = false;

    private WebXRHandData leftHand = new WebXRHandData();
    private WebXRHandData rightHand = new WebXRHandData();

    private WebXRControllerData controller1 = new WebXRControllerData();
    private WebXRControllerData controller2 = new WebXRControllerData();

    private WebXRHitPoseData viewerHitTestPose = new WebXRHitPoseData();

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

    public void setXrState(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      this.xrState = state;
      viewerHitTestOn = false;
      if (OnXRChange != null)
        OnXRChange(state, viewsCount, leftRect, rightRect);
    }

    // received start VR from WebVR browser
    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action<int, float, float, float, float, float, float, float, float>))]
    #endif
    public static void OnStartAR(int viewsCount,
      float left_x, float left_y, float left_w, float left_h,
      float right_x, float right_y, float right_w, float right_h)
    {
      Instance.setXrState(WebXRState.AR, viewsCount,
                          new Rect(left_x, left_y, left_w, left_h),
                          new Rect(right_x, right_y, right_w, right_h));
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action<int>))]
    #endif
    public static void OnStartVR(int viewsCount)
    {
      Instance.setXrState(WebXRState.VR, viewsCount, new Rect(), new Rect());
    }

    // receive end VR from WebVR browser
    #if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action))]
    #endif
    public static void OnEndXR()
    {
      Instance.setXrState(WebXRState.NORMAL, 1, new Rect(), new Rect());
    }

    public void StartViewerHitTest()
    {
      if (xrState == WebXRState.AR && !viewerHitTestOn)
      {
        viewerHitTestOn = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        ToggleViewerHitTest();
#endif
      }
    }

    public void StopViewerHitTest()
    {
      if (xrState == WebXRState.AR && viewerHitTestOn)
      {
        viewerHitTestOn = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        ToggleViewerHitTest();
#endif
      }
    }

    public void HapticPulse(WebXRControllerHand hand, float intensity, float duration)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ControllerPulse((int)hand, intensity, duration);
#endif
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

    bool GetGamepadFromControllersArray(int controllerIndex, ref WebXRControllerData newControllerData)
    {
      int arrayPosition = controllerIndex * 20;
      int frameNumber = (int)controllersArray[arrayPosition++];
      if (newControllerData.frame == frameNumber)
      {
        return false;
      }
      newControllerData.frame = frameNumber;
      newControllerData.enabled = controllersArray[arrayPosition++] != 0;
      newControllerData.hand = (int)controllersArray[arrayPosition++];
      if (!newControllerData.enabled)
      {
        return true;
      }
      newControllerData.position = new Vector3(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++]);
      newControllerData.rotation = new Quaternion(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++]);
      newControllerData.trigger = controllersArray[arrayPosition++];
      newControllerData.squeeze = controllersArray[arrayPosition++];
      newControllerData.thumbstick = controllersArray[arrayPosition++];
      newControllerData.thumbstickX = controllersArray[arrayPosition++];
      newControllerData.thumbstickY = controllersArray[arrayPosition++];
      newControllerData.touchpad = controllersArray[arrayPosition++];
      newControllerData.touchpadX = controllersArray[arrayPosition++];
      newControllerData.touchpadY = controllersArray[arrayPosition++];
      newControllerData.buttonA = controllersArray[arrayPosition++];
      newControllerData.buttonB = controllersArray[arrayPosition];
      return true;
    }

    bool GetHandFromHandsArray(int handIndex, ref WebXRHandData handObject)
    {
      int arrayPosition = handIndex * 230;
      int frameNumber = (int)handsArray[arrayPosition++];
      if (handObject.frame == frameNumber)
      {
        return false;
      }
      handObject.frame = frameNumber;
      handObject.enabled = handsArray[arrayPosition++] != 0;
      handObject.hand = (int)handsArray[arrayPosition++];
      handObject.trigger = handsArray[arrayPosition++];
      handObject.squeeze = handsArray[arrayPosition++];
      if (!handObject.enabled)
      {
        return true;
      }
      for (int i=0; i<=WebXRHandData.LITTLE_PHALANX_TIP; i++)
      {
        handObject.joints[i].enabled = handsArray[arrayPosition++] != 0;
        handObject.joints[i].position = new Vector3(handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++]);
        handObject.joints[i].rotation = new Quaternion(handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++]);
        handObject.joints[i].radius = handsArray[arrayPosition++];
      }
      return true;
    }

    bool GetHitTestPoseFromViewerHitTestPoseArray(ref WebXRHitPoseData hitPoseData)
    {
      int arrayPosition = 0;
      int frameNumber = (int)viewerHitTestPoseArray[arrayPosition++];
      if (hitPoseData.frame == frameNumber)
      {
        return false;
      }
      hitPoseData.frame = frameNumber;
      hitPoseData.available = viewerHitTestPoseArray[arrayPosition++] != 0;
      if (!hitPoseData.available)
      {
        return true;
      }
      hitPoseData.position = new Vector3(viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++]);
      hitPoseData.rotation = new Quaternion(viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++]);
      return true;
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        set_webxr_events(OnStartAR, OnStartVR, OnEndXR, OnXRCapabilities);
        InitControllersArray(controllersArray, controllersArray.Length);
        InitHandsArray(handsArray, handsArray.Length);
        InitViewerHitTestPoseArray(viewerHitTestPoseArray, viewerHitTestPoseArray.Length);
        InitXRSharedArray(sharedArray, sharedArray.Length);
        ListenWebXRData();
#endif
    }

    void Update()
    {
      bool hasHandsData = false;
      if (OnHandUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        if (GetHandFromHandsArray(0, ref leftHand))
        {
          OnHandUpdate(leftHand);
        }
        if (GetHandFromHandsArray(1, ref rightHand))
        {
          OnHandUpdate(rightHand);
        }
        hasHandsData = leftHand.enabled || rightHand.enabled;
      }
      
      if (!hasHandsData && OnControllerUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        if (GetGamepadFromControllersArray(0, ref controller1))
        {
          OnControllerUpdate(controller1);
        }
        if (GetGamepadFromControllersArray(1, ref controller2))
        {
          OnControllerUpdate(controller2);
        }
      }

      if (OnViewerHitTestUpdate != null && this.xrState == WebXRState.AR)
      {
        if (GetHitTestPoseFromViewerHitTestPoseArray(ref viewerHitTestPose))
        {
          OnViewerHitTestUpdate(viewerHitTestPose);
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
