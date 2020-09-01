
namespace WebXR
{
  public enum WebXRState { VR, AR, NORMAL }

  public class WebXRManager : SubsystemLifecycleManager<WebXRSubsystem, WebXRSubsystemDescriptor>
  {
      public static WebXRManager Instance { get; private set; }

      public WebXRState XRState => subsystem.xrState;
      
      public event WebXRSubsystem.XRChange OnXRChange
      {
          add => subsystem.OnXRChange += value;
          remove => subsystem.OnXRChange -= value;
      }
      public event WebXRSubsystem.ControllerUpdate OnControllerUpdate
      {
          add => subsystem.OnControllerUpdate += value;
          remove => subsystem.OnControllerUpdate -= value;
      }
      public event WebXRSubsystem.HandUpdate OnHandUpdate
      {
          add => subsystem.OnHandUpdate += value;
          remove => subsystem.OnHandUpdate -= value;
      }
      public event WebXRSubsystem.HeadsetUpdate OnHeadsetUpdate
      {
          add => subsystem.OnHeadsetUpdate += value;
          remove => subsystem.OnHeadsetUpdate -= value;
      }
      public event WebXRSubsystem.HitTestUpdate OnViewerHitTestUpdate
      {
          add => subsystem.OnViewerHitTestUpdate += value;
          remove => subsystem.OnViewerHitTestUpdate -= value;
      }
      
      public void HapticPulse(WebXRControllerHand hand, float intensity, float duration)
      {
          subsystem.HapticPulse(hand, intensity, duration);
      }

      public void StartViewerHitTest()
      {
          subsystem.StartViewerHitTest();
      }

      public void StopViewerHitTest()
      {
          subsystem.StopViewerHitTest();
      }
      
      protected override void Awake()
      {
          base.Awake();
          Instance = this;
      }

      private void Update()
      {
          subsystem.OnUpdate();
      }

      private void LateUpdate()
      {
          subsystem.OnLateUpdate();
      }
  }
}
