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

    public void PlaceAnchorAtCurrentHitTest()
    {
        WebXRManager.Instance.CreateAnchorFromViewerHitTest();
    }

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