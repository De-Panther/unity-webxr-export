using UnityEngine;

namespace WebXR
{
  public class WebXRCameraSettings : MonoBehaviour
  {
    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private CameraClearFlags m_normalClearFlags;

    [SerializeField]
    private Color m_normalBackgroundColor;

    [SerializeField]
    private LayerMask m_normalCullingMask;

    [SerializeField]
    private CameraClearFlags m_vrClearFlags;

    [SerializeField]
    private Color m_vrBackgroundColor;

    [SerializeField]
    private LayerMask m_vrCullingMask;

    [SerializeField]
    private CameraClearFlags m_arClearFlags;

    [SerializeField]
    private Color m_arBackgroundColor;

    [SerializeField]
    private LayerMask m_arCullingMask;

    public Camera Camera
    {
      get { return m_camera; }
      set { m_camera = value; }
    }

    public CameraClearFlags NormalClearFlags
    {
      get { return m_normalClearFlags; }
      set { m_normalClearFlags = value; }
    }

    public Color NormalBackgroundColor
    {
      get { return m_normalBackgroundColor; }
      set { m_normalBackgroundColor = value; }
    }

    public LayerMask NormalCullingMask
    {
      get { return m_normalCullingMask; }
      set { m_normalCullingMask = value; }
    }

    public CameraClearFlags VRClearFlags
    {
      get { return m_vrClearFlags; }
      set { m_vrClearFlags = value; }
    }

    public Color VRBackgroundColor
    {
      get { return m_vrBackgroundColor; }
      set { m_vrBackgroundColor = value; }
    }

    public LayerMask VRCullingMask
    {
      get { return m_vrCullingMask; }
      set { m_vrCullingMask = value; }
    }

    public CameraClearFlags ARClearFlags
    {
      get { return m_arClearFlags; }
      set { m_arClearFlags = value; }
    }

    public Color ARBackgroundColor
    {
      get { return m_arBackgroundColor; }
      set { m_arBackgroundColor = value; }
    }

    public LayerMask ARCullingMask
    {
      get { return m_arCullingMask; }
      set { m_arCullingMask = value; }
    }

    private void OnEnable()
    {
      WebXRManager.OnXRChange += OnXRChange;
      OnXRChange(WebXRManager.Instance.XRState,
                  WebXRManager.Instance.ViewsCount,
                  WebXRManager.Instance.ViewsLeftRect,
                  WebXRManager.Instance.ViewsRightRect);
    }

    private void OnDisable()
    {
      WebXRManager.OnXRChange -= OnXRChange;
    }

    private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      if (m_camera == null)
      {
        return;
      }
      switch (state)
      {
        case WebXRState.NORMAL:
          m_camera.clearFlags = m_normalClearFlags;
          m_camera.backgroundColor = m_normalBackgroundColor;
          m_camera.cullingMask = m_normalCullingMask;
          break;
        case WebXRState.VR:
          m_camera.clearFlags = m_vrClearFlags;
          m_camera.backgroundColor = m_vrBackgroundColor;
          m_camera.cullingMask = m_vrCullingMask;
          break;
        case WebXRState.AR:
          m_camera.clearFlags = m_arClearFlags;
          m_camera.backgroundColor = m_arBackgroundColor;
          m_camera.cullingMask = m_arCullingMask;
          break;
      }
    }
  }
}
