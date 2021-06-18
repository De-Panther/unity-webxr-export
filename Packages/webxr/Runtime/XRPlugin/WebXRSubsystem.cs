using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace WebXR
{
  // TODO: we need an XRInputSubsystem implementation - this can only be done via native code

  public class WebXRSubsystemDescriptor : SubsystemDescriptor<WebXRSubsystem>
  {
  }

  public class WebXRSubsystem : Subsystem<WebXRSubsystemDescriptor>
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterDescriptor()
    {
      var res = SubsystemRegistration.CreateDescriptor(new WebXRSubsystemDescriptor()
      {
        id = typeof(WebXRSubsystem).FullName,
        subsystemImplementationType = typeof(WebXRSubsystem)
      });
      if (res)
        Debug.Log("Registered " + nameof(WebXRSubsystemDescriptor));
      else Debug.Log("Failed registering " + nameof(WebXRSubsystemDescriptor));
    }

    public override void Start()
    {
      if (running) return;
      Debug.Log("Start " + nameof(WebXRSubsystem));
      _running = true;
      Instance = this;
      InternalStart();
    }

    public override void Stop()
    {
      if (!_running) return;
      Debug.Log("Stop " + nameof(WebXRSubsystem));
      _running = false;
      Instance = null;
    }

    protected override void OnDestroy()
    {
      if (!running) return;
      Debug.Log("Destroy " + nameof(WebXRSubsystem));
      _running = false;
      Instance = null;
    }

    private void UpdateControllersOnEnd()
    {
      if (OnHandUpdate != null)
      {
        if (GetHandFromHandsArray(0, ref leftHand))
        {
          OnHandUpdate?.Invoke(leftHand);
        }

        if (GetHandFromHandsArray(1, ref rightHand))
        {
          OnHandUpdate?.Invoke(rightHand);
        }
      }

      if (OnControllerUpdate != null)
      {
        if (GetGamepadFromControllersArray(0, ref controller1))
        {
          OnControllerUpdate?.Invoke(controller1);
        }

        if (GetGamepadFromControllersArray(1, ref controller2))
        {
          OnControllerUpdate?.Invoke(controller2);
        }
      }
    }

    internal void OnUpdate()
    {
      if (!reportedXRStateSwitch)
      {
        reportedXRStateSwitch = true;
        OnXRChange?.Invoke(xrState, viewsCount, leftRect, rightRect);
      }
      if (!updatedControllersOnEnd)
      {
        updatedControllersOnEnd = true;
        UpdateControllersOnEnd();
      }
      if (visibilityStateChanged)
      {
        visibilityStateChanged = false;
        OnVisibilityChange?.Invoke(visibilityState);
      }
      if (this.xrState == WebXRState.NORMAL)
      {
        return;
      }
      UpdateXRCameras();
      bool hasHandsData = false;
      if (OnHandUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        if (GetHandFromHandsArray(0, ref leftHand))
        {
          OnHandUpdate?.Invoke(leftHand);
        }

        if (GetHandFromHandsArray(1, ref rightHand))
        {
          OnHandUpdate?.Invoke(rightHand);
        }

        hasHandsData = leftHand.enabled || rightHand.enabled;
      }

      if (!hasHandsData && OnControllerUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        if (GetGamepadFromControllersArray(0, ref controller1))
        {
          OnControllerUpdate?.Invoke(controller1);
        }

        if (GetGamepadFromControllersArray(1, ref controller2))
        {
          OnControllerUpdate?.Invoke(controller2);
        }
      }

      if (OnViewerHitTestUpdate != null && this.xrState == WebXRState.AR)
      {
        if (GetHitTestPoseFromViewerHitTestPoseArray(ref viewerHitTestPose))
        {
          OnViewerHitTestUpdate?.Invoke(viewerHitTestPose);
        }
      }
    }

    private void UpdateXRCameras()
    {
      if (OnHeadsetUpdate != null && this.xrState != WebXRState.NORMAL)
      {
        GetMatrixFromSharedArray(0, ref leftProjectionMatrix);
        GetMatrixFromSharedArray(16, ref rightProjectionMatrix);
        GetQuaternionFromSharedArray(32, ref leftRotation);
        GetQuaternionFromSharedArray(36, ref rightRotation);
        GetVector3FromSharedArray(40, ref leftPosition);
        GetVector3FromSharedArray(43, ref rightPosition);

        OnHeadsetUpdate?.Invoke(
            leftProjectionMatrix,
            rightProjectionMatrix,
            leftRotation,
            rightRotation,
            leftPosition,
            rightPosition);
      }
    }

    private bool _running;
    public override bool running => _running;

    private static WebXRSubsystem Instance;

    private void InternalStart()
    {
#if UNITY_WEBGL
      Native.SetWebXREvents(OnStartAR, OnStartVR, UpdateVisibilityState, OnEndXR, OnXRCapabilities, OnInputProfiles);
      Native.InitControllersArray(controllersArray);
      Native.InitHandsArray(handsArray);
      Native.InitViewerHitTestPoseArray(viewerHitTestPoseArray);
      Native.InitXRSharedArray(sharedArray);
#endif
    }

#if UNITY_WEBGL
    private static class Native
    {
      [DllImport("__Internal")]
      public static extern void InitXRSharedArray(float[] array);

      [DllImport("__Internal")]
      public static extern void InitControllersArray(float[] array);

      [DllImport("__Internal")]
      public static extern void InitHandsArray(float[] array);

      [DllImport("__Internal")]
      public static extern void InitViewerHitTestPoseArray(float[] array);

      [DllImport("__Internal")]
      public static extern void ToggleAR();

      [DllImport("__Internal")]
      public static extern void ToggleVR();

      [DllImport("__Internal")]
      public static extern void ToggleViewerHitTest();

      [DllImport("__Internal")]
      public static extern void ControllerPulse(int controller, float intensity, float duration);

      [DllImport("__Internal")]
      public static extern void SetWebXREvents(Action<int, float, float, float, float, float, float, float, float> on_start_ar,
          Action<int, float, float, float, float, float, float, float, float> on_start_vr,
          Action<int> on_visibility_change,
          Action on_end_xr,
          Action<bool, bool> on_xr_capabilities,
          Action<string> on_input_profiles);
    }
#endif

    internal WebXRState xrState = WebXRState.NORMAL;
    private int viewsCount = 1;
    private Rect leftRect;
    private Rect rightRect;
    private bool reportedXRStateSwitch = true;
    internal WebXRVisibilityState visibilityState = WebXRVisibilityState.VISIBLE;
    private bool visibilityStateChanged = false;

    public delegate void XRCapabilitiesUpdate(WebXRDisplayCapabilities capabilities);

    internal static event XRCapabilitiesUpdate OnXRCapabilitiesUpdate;

    public delegate void XRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect);

    internal static event XRChange OnXRChange;

    public delegate void VisibilityChange(WebXRVisibilityState visibilityState);

    internal static event VisibilityChange OnVisibilityChange;

    public delegate void HeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Quaternion leftRotation,
        Quaternion rightRotation,
        Vector3 leftPosition,
        Vector3 rightPosition);

    internal static event HeadsetUpdate OnHeadsetUpdate;

    public delegate void ControllerUpdate(WebXRControllerData controllerData);

    internal static event ControllerUpdate OnControllerUpdate;

    public delegate void HandUpdate(WebXRHandData handData);

    internal static event HandUpdate OnHandUpdate;

    public delegate void HitTestUpdate(WebXRHitPoseData hitPoseData);

    internal static event HitTestUpdate OnViewerHitTestUpdate;

    // Cameras calculations helpers
    private Matrix4x4 leftProjectionMatrix = new Matrix4x4();
    private Matrix4x4 rightProjectionMatrix = new Matrix4x4();
    private Vector3 leftPosition = new Vector3();
    private Vector3 rightPosition = new Vector3();
    private Quaternion leftRotation = Quaternion.identity;
    private Quaternion rightRotation = Quaternion.identity;

    // Shared array which we will load headset data in from webxr.jslib
    // Array stores 2 matrices, each 16 values, 2 Quaternions and 2 Vector3, stored linearly.
    float[] sharedArray = new float[(2 * 16) + (2 * 7)];

    // Shared array for controllers data
    float[] controllersArray = new float[2 * 28];

    // Shared array for hands data
    float[] handsArray = new float[2 * (25 * 8 + 5)];

    // Shared array for hit-test pose data
    float[] viewerHitTestPoseArray = new float[9];

    bool viewerHitTestOn = false;

    private bool updatedControllersOnEnd = true;

    private WebXRHandData leftHand = new WebXRHandData();
    private WebXRHandData rightHand = new WebXRHandData();

    private WebXRControllerData controller1 = new WebXRControllerData();
    private WebXRControllerData controller2 = new WebXRControllerData();

    private WebXRHitPoseData viewerHitTestPose = new WebXRHitPoseData();

    internal WebXRDisplayCapabilities capabilities = new WebXRDisplayCapabilities();

    // Handles WebXR capabilities from browser
    [MonoPInvokeCallback(typeof(Action<bool, bool>))]
    public static void OnXRCapabilities(bool isARSupported, bool isVRSupported)
    {
      Instance.capabilities.canPresentAR = isARSupported;
      Instance.capabilities.canPresentVR = isVRSupported;
      Instance.OnXRCapabilities(Instance.capabilities);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void OnInputProfiles(string json)
    {
      WebXRControllersProfiles controllersProfiles = JsonUtility.FromJson<WebXRControllersProfiles>(json);
      Instance.OnInputProfiles(controllersProfiles);
    }

    public void OnXRCapabilities(WebXRDisplayCapabilities cap)
    {
      this.capabilities = cap;
      OnXRCapabilitiesUpdate?.Invoke(cap);
    }

    public void OnInputProfiles(WebXRControllersProfiles controllersProfiles)
    {
      controller1.profiles = controllersProfiles.conrtoller1;
      controller2.profiles = controllersProfiles.conrtoller2;
    }

    public void setXrState(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      visibilityState = WebXRVisibilityState.VISIBLE;
      this.xrState = state;
      this.viewsCount = viewsCount;
      this.leftRect = leftRect;
      this.rightRect = rightRect;
      viewerHitTestOn = false;
      reportedXRStateSwitch = false;
      if (state != WebXRState.NORMAL)
      {
        visibilityStateChanged = true;
      }
    }

    // received start AR from WebXR browser
    [MonoPInvokeCallback(typeof(Action<int, float, float, float, float, float, float, float, float>))]
    public static void OnStartAR(int viewsCount,
        float left_x, float left_y, float left_w, float left_h,
        float right_x, float right_y, float right_w, float right_h)
    {
      Instance.setXrState(WebXRState.AR, viewsCount,
          new Rect(left_x, left_y, left_w, left_h),
          new Rect(right_x, right_y, right_w, right_h));
    }

    // received start VR from WebXR browser
    [MonoPInvokeCallback(typeof(Action<int, float, float, float, float, float, float, float, float>))]
    public static void OnStartVR(int viewsCount,
        float left_x, float left_y, float left_w, float left_h,
        float right_x, float right_y, float right_w, float right_h)
    {
      Instance.setXrState(WebXRState.VR, viewsCount,
          new Rect(left_x, left_y, left_w, left_h),
          new Rect(right_x, right_y, right_w, right_h));
    }

    [MonoPInvokeCallback(typeof(Action<int>))]
    public static void UpdateVisibilityState(int visibilityState)
    {
      if (Instance.visibilityState != (WebXRVisibilityState)visibilityState)
      {
        Instance.visibilityState = (WebXRVisibilityState)visibilityState;
        Instance.visibilityStateChanged = true;
      }
    }

    // receive end VR from WebVR browser
    [MonoPInvokeCallback(typeof(Action))]
    public static void OnEndXR()
    {
      Instance.updatedControllersOnEnd = false;
      Instance.setXrState(WebXRState.NORMAL, 1, new Rect(), new Rect());
    }

    public void ToggleAR()
    {
#if UNITY_WEBGL
      if (capabilities.canPresentAR)
      {
        Native.ToggleAR();
      }
#endif
    }

    public void ToggleVR()
    {
#if UNITY_WEBGL
      if (capabilities.canPresentVR)
      {
        Native.ToggleVR();
      }
#endif
    }

    public void StartViewerHitTest()
    {
#if UNITY_WEBGL
      if (xrState == WebXRState.AR && !viewerHitTestOn)
      {
        viewerHitTestOn = true;
        Native.ToggleViewerHitTest();
      }
#endif
    }

    public void StopViewerHitTest()
    {
#if UNITY_WEBGL
      if (xrState == WebXRState.AR && viewerHitTestOn)
      {
        viewerHitTestOn = false;
        Native.ToggleViewerHitTest();
      }
#endif
    }

    public void HapticPulse(WebXRControllerHand hand, float intensity, float duration)
    {
#if UNITY_WEBGL
      Native.ControllerPulse((int)hand, intensity, duration);
#endif
    }

    void GetMatrixFromSharedArray(int index, ref Matrix4x4 matrix)
    {
      for (int i = 0; i < 16; i++)
      {
        matrix[i] = sharedArray[index + i];
      }
    }

    void GetQuaternionFromSharedArray(int index, ref Quaternion quaternion)
    {
      quaternion.x = sharedArray[index];
      quaternion.y = sharedArray[index + 1];
      quaternion.z = sharedArray[index + 2];
      quaternion.w = sharedArray[index + 3];
    }

    void GetVector3FromSharedArray(int index, ref Vector3 vec3)
    {
      vec3.x = sharedArray[index];
      vec3.y = sharedArray[index + 1];
      vec3.z = sharedArray[index + 2];
    }

    bool GetGamepadFromControllersArray(int controllerIndex, ref WebXRControllerData newControllerData)
    {
      int arrayPosition = controllerIndex * 28;
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
      newControllerData.rotation = new Quaternion(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++],
          controllersArray[arrayPosition++]);
      newControllerData.trigger = controllersArray[arrayPosition++];
      newControllerData.squeeze = controllersArray[arrayPosition++];
      newControllerData.thumbstick = controllersArray[arrayPosition++];
      newControllerData.thumbstickX = controllersArray[arrayPosition++];
      newControllerData.thumbstickY = controllersArray[arrayPosition++];
      newControllerData.touchpad = controllersArray[arrayPosition++];
      newControllerData.touchpadX = controllersArray[arrayPosition++];
      newControllerData.touchpadY = controllersArray[arrayPosition++];
      newControllerData.buttonA = controllersArray[arrayPosition++];
      newControllerData.buttonB = controllersArray[arrayPosition++];
      if (controllersArray[arrayPosition] == 1)
      {
        controllersArray[arrayPosition++] = 2;
        newControllerData.gripPosition = new Vector3(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++]);
        newControllerData.gripRotation = new Quaternion(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++],
            controllersArray[arrayPosition++]);
        Quaternion rotationOffset = Quaternion.Inverse(newControllerData.rotation);
        newControllerData.gripPosition = rotationOffset * (newControllerData.gripPosition - newControllerData.position);
        newControllerData.gripRotation = rotationOffset * newControllerData.gripRotation;
      }
      return true;
    }

    bool GetHandFromHandsArray(int handIndex, ref WebXRHandData handObject)
    {
      int arrayPosition = handIndex * 205;
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

      for (int i = 0; i <= (int)WebXRHandJoint.pinky_finger_tip; i++)
      {
        handObject.joints[i].position = new Vector3(handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++]);
        handObject.joints[i].rotation = new Quaternion(handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++],
            handsArray[arrayPosition++]);
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

      hitPoseData.position = new Vector3(viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++],
          viewerHitTestPoseArray[arrayPosition++]);
      hitPoseData.rotation = new Quaternion(viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++],
          viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++]);
      return true;
    }
  }
}