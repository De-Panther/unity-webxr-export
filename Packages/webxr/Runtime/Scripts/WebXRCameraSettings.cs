using UnityEngine;

namespace WebXR
{
  public class WebXRCameraSettings : MonoBehaviour
  {
    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private Transform m_transform;

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

    [SerializeField]
    private bool m_updateNormalFieldOfView = true;

    [SerializeField]
    private bool m_useNormalFieldOfViewFromAwake = true;

    [SerializeField]
    private float m_normalFieldOfView = 60f;

    [SerializeField]
    private bool m_updateNormalLocalPose = true;

    [SerializeField]
    private bool m_useNormalPoseFromAwake = true;

    [SerializeField]
    private Vector3 m_normalLocalPosition = Vector3.zero;

    [SerializeField]
    private Quaternion m_normalLocalRotation = Quaternion.identity;

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

    public bool UpdateNormalFieldOfView
    {
      get { return m_updateNormalFieldOfView; }
      set { m_updateNormalFieldOfView = value; }
    }

    public bool UseNormalFieldOfViewFromAwake
    {
      get { return m_useNormalFieldOfViewFromAwake; }
      set { m_useNormalFieldOfViewFromAwake = value; }
    }

    public float NormalFieldOfView
    {
      get { return m_normalFieldOfView; }
      set { m_normalFieldOfView = value; }
    }

    public bool UpdateNormalLocalPose
    {
      get { return m_updateNormalLocalPose; }
      set { m_updateNormalLocalPose = value; }
    }

    public bool UseNormalPoseFromAwake
    {
      get { return m_useNormalPoseFromAwake; }
      set { m_useNormalPoseFromAwake = value; }
    }

    public Vector3 NormalLocalPosition
    {
      get { return m_normalLocalPosition; }
      set { m_normalLocalPosition = value; }
    }

    public Quaternion NormalLocalRotation
    {
      get { return m_normalLocalRotation; }
      set { m_normalLocalRotation = value; }
    }

    private void Awake()
    {
      if (m_camera == null)
      {
        return;
      }
      m_transform = m_camera.transform;
      if (m_useNormalFieldOfViewFromAwake)
      {
        m_normalFieldOfView = m_camera.fieldOfView;
      }
      if (m_useNormalPoseFromAwake)
      {
        m_normalLocalPosition = m_transform.localPosition;
        m_normalLocalRotation = m_transform.localRotation;
      }
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
          if (m_updateNormalFieldOfView)
          {
            m_camera.fieldOfView = m_normalFieldOfView;
            m_camera.ResetProjectionMatrix();
          }
          if (m_updateNormalLocalPose)
          {
#if HAS_POSITION_AND_ROTATION
            m_transform.SetLocalPositionAndRotation(m_normalLocalPosition, m_normalLocalRotation);
#else
            m_transform.localPosition = m_normalLocalPosition;
            m_transform.localRotation = m_normalLocalRotation;
#endif
          }
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
