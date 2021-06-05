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
  public class WebXRManager : SubsystemLifecycleManager<WebXRSubsystem, WebXRSubsystemDescriptor>
  {
    public static WebXRManager Instance { get; private set; }

    public WebXRState XRState => subsystem.xrState;

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
        return subsystem.capabilities.canPresentAR;
      }
    }

    public bool isSupportedVR
    {
      get
      {
        return subsystem.capabilities.canPresentVR;
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
      Instance = this;
      enabled = subsystem != null;
    }

    private void Update()
    {
      subsystem.OnUpdate();
    }
  }
}
