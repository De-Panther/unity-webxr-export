public partial class WebXRManager
{
    public static event WebXRSubsystem.AnchorCreated OnAnchorCreated
    {
      add => WebXRSubsystem.OnAnchorCreated += value;
      remove => WebXRSubsystem.OnAnchorCreated -= value;
    }
    
    public static event WebXRSubsystem.AnchorDeleted OnAnchorDeleted
    {
      add => WebXRSubsystem.OnAnchorDeleted += value;
      remove => WebXRSubsystem.OnAnchorDeleted -= value;
    }
    
    public static event WebXRSubsystem.AnchorUpdate OnAnchorUpdate
    {
      add => WebXRSubsystem.OnAnchorUpdate += value;
      remove => WebXRSubsystem.OnAnchorUpdate -= value;
    }

    public void CreateAnchorFromViewerHitTest()
    {
      subsystem?.CreateAnchorFromViewerHitTest();
    }
    
    public void CreateAnchorFromPose(Vector3 position, Quaternion rotation)
    {
      subsystem?.CreateAnchorFromPose(position, rotation);
    }
    
    public void DeleteAnchor(int anchorId)
    {
      subsystem?.DeleteAnchor(anchorId);
    }
    
    public void DeleteAllAnchors()
    {
      subsystem?.DeleteAllAnchors();
    }
}