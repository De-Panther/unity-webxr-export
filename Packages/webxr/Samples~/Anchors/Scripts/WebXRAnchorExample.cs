using System.Collections.Generic;
using UnityEngine;
using WebXR;

public class WebXRAnchorExample : MonoBehaviour
{
    [SerializeField]
    private Transform anchoredPrefab;

    private readonly Dictionary<int, Transform> instances = new Dictionary<int, Transform>();

    private void OnEnable()
    {
        WebXRManager.OnAnchorUpdate += OnAnchorUpdate;
        WebXRManager.OnAnchorDeleted += OnAnchorDeleted;
    }

    private void OnDisable()
    {
        WebXRManager.OnAnchorUpdate -= OnAnchorUpdate;
        WebXRManager.OnAnchorDeleted -= OnAnchorDeleted;
    }

    public void StartHitTest()
    {
        WebXRManager.Instance.StartViewerHitTest();
    }

    /// <summary>
    /// Creates an anchor at the current hit test result. The anchor will be created at the position and rotation of the current hit test result, which is updated every frame when a hit test result is available. If no hit test result is available, no anchor will be created.<br/>
    /// This method does not work reliably because of timing.. better use <see cref="PlaceAnchorAtNextHitTest"/>
    /// </summary>
    public void PlaceAnchorAtCurrentHitTest()
    {
        WebXRManager.Instance.CreateAnchorFromViewerHitTest();
    }

    /// <summary>
    /// Creates an anchor at the next hit test result. The anchor will be created at the position and rotation of the next hit test result, which is updated every frame when a hit test result is available.
    /// </summary>
    public void PlaceAnchorAtNextHitTest()
    {
        WebXRManager.Instance.CreateAnchorFromWaitingForViewerHitTest();
    }

    /// <summary>
    /// Creates an anchor at the position and rotation of the given transform. The anchor will be created at the position and rotation of the given transform. <br/>
    /// <i>Anchors are more reliable when created from hit test results, so prefer the other methods if possible.</i>
    /// </summary>
    /// <param name="source">Any Transform (this method will just read its position and rotation)</param>
    public void PlaceAnchorAtTransform(Transform source)
    {
        WebXRManager.Instance.CreateAnchorFromPose(source.position, source.rotation);
    }

    private void OnAnchorUpdate(WebXRAnchorData anchor)
    {
        if (!anchor.tracked)
            return;

        if (!instances.TryGetValue(anchor.id, out Transform instance))
        {
            instance = Instantiate(anchoredPrefab);
            instances.Add(anchor.id, instance);
        }

        instance.SetPositionAndRotation(anchor.position, anchor.rotation);
    }

    private void OnAnchorDeleted(int anchorId)
    {
        if (instances.TryGetValue(anchorId, out Transform instance))
        {
            Destroy(instance.gameObject);
            instances.Remove(anchorId);
        }
    }
}