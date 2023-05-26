using UnityEngine;

namespace WebXR
{
  public enum WebXRState { VR, AR, NORMAL }
  public enum WebXRVisibilityState
  {
    VISIBLE,
    VISIBLE_BLURRED,
    HIDDEN
  }

  [DefaultExecutionOrder(-2020)]
  #if UNITY_XR_MANAGEMENT_4_3_1_OR_NEWER
  public class WebXRManager : SubsystemLifecycleManager<WebXRSubsystem, WebXRSubsystemDescriptor, WebXRSubsystemProvider>
  #else
  public class WebXRManager : SubsystemLifecycleManager<WebXRSubsystem, WebXRSubsystemDescriptor>
  #endif
  {
    private static readonly Rect defaultRect = new Rect(0, 0, 1, 1);

    public static WebXRManager Instance { get; private set; }

    public WebXRState XRState => subsystem == null ? WebXRState.NORMAL : subsystem.xrState;
    public int ViewsCount => subsystem == null ? 1 : subsystem.viewsCount;
    public Rect ViewsLeftRect => subsystem == null ? defaultRect : subsystem.leftRect;
    public Rect ViewsRightRect => subsystem == null ? defaultRect : subsystem.rightRect;

    public static event WebXRSubsystem.XRCapabilitiesUpdate OnXRCapabilitiesUpdate
    {
      add => WebXRSubsystem.OnXRCapabilitiesUpdate += value;
      remove => WebXRSubsystem.OnXRCapabilitiesUpdate -= value;
    }

    public static event WebXRSubsystem.XRChange OnXRChange
    {
      add => WebXRSubsystem.OnXRChange += value;
      remove => WebXRSubsystem.OnXRChange -= value;
    }

    public static event WebXRSubsystem.VisibilityChange OnVisibilityChange
    {
      add => WebXRSubsystem.OnVisibilityChange += value;
      remove => WebXRSubsystem.OnVisibilityChange -= value;
    }

    public static event WebXRSubsystem.ControllerUpdate OnControllerUpdate
    {
      add => WebXRSubsystem.OnControllerUpdate += value;
      remove => WebXRSubsystem.OnControllerUpdate -= value;
    }

    public static event WebXRSubsystem.HandUpdate OnHandUpdate
    {
      add => WebXRSubsystem.OnHandUpdate += value;
      remove => WebXRSubsystem.OnHandUpdate -= value;
    }

    public static event WebXRSubsystem.HeadsetUpdate OnHeadsetUpdate
    {
      add => WebXRSubsystem.OnHeadsetUpdate += value;
      remove => WebXRSubsystem.OnHeadsetUpdate -= value;
    }

    public static event WebXRSubsystem.HitTestUpdate OnViewerHitTestUpdate
    {
      add => WebXRSubsystem.OnViewerHitTestUpdate += value;
      remove => WebXRSubsystem.OnViewerHitTestUpdate -= value;
    }

    public bool isSupportedAR
    {
      get
      {
        return subsystem == null ? false : subsystem.capabilities.canPresentAR;
      }
    }

    public bool isSupportedVR
    {
      get
      {
        return subsystem == null ? false : subsystem.capabilities.canPresentVR;
      }
    }

    public WebXRVisibilityState visibilityState
    {
      get
      {
        if (subsystem == null)
        {
          return WebXRVisibilityState.VISIBLE;
        }
        return subsystem.visibilityState;
      }
    }

    public void ToggleAR()
    {
      subsystem?.ToggleAR();
    }

    public void ToggleVR()
    {
      subsystem?.ToggleVR();
    }

    public void HapticPulse(WebXRControllerHand hand, float intensity, float duration)
    {
      subsystem?.HapticPulse(hand, intensity, duration);
    }

    public void PreRenderSpectatorCamera()
    {
      subsystem?.PreRenderSpectatorCamera();
    }

    public void StartViewerHitTest()
    {
      subsystem?.StartViewerHitTest();
    }

    public void StopViewerHitTest()
    {
      subsystem?.StopViewerHitTest();
    }

    protected override void Awake()
    {
      base.Awake();
      if (Instance != null)
      {
        Debug.LogError("More than one WebXRManager components in scene. Disabling previous one.");
        Instance.enabled = false;
      }
      Instance = this;
      enabled = subsystem != null;
    }

    private void Update()
    {
      subsystem.OnUpdate();
    }
  }
}
