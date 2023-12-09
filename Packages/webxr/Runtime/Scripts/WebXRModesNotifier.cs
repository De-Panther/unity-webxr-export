using UnityEngine;
using UnityEngine.Events;

namespace WebXR
{
  public class WebXRModesNotifier : MonoBehaviour
  {
    [SerializeField]
    private UnityEvent m_OnSwitchedToAR;

    public UnityEvent OnSwitchedToAR
    {
      get { return m_OnSwitchedToAR; }
      set { m_OnSwitchedToAR = value; }
    }

    [SerializeField]
    private UnityEvent m_OnSwitchedFromAR;

    public UnityEvent OnSwitchedFromAR
    {
      get { return m_OnSwitchedFromAR; }
      set { m_OnSwitchedFromAR = value; }
    }

    [SerializeField]
    private UnityEvent m_OnSwitchedToVR;

    public UnityEvent OnSwitchedToVR
    {
      get { return m_OnSwitchedToVR; }
      set { m_OnSwitchedToVR = value; }
    }

    [SerializeField]
    private UnityEvent m_OnSwitchedFromVR;

    public UnityEvent OnSwitchedFromVR
    {
      get { return m_OnSwitchedFromVR; }
      set { m_OnSwitchedFromVR = value; }
    }

    [SerializeField]
    private UnityEvent m_OnSwitchedToNormal;

    public UnityEvent OnSwitchedToNormal
    {
      get { return m_OnSwitchedToNormal; }
      set { m_OnSwitchedToNormal = value; }
    }

    [SerializeField]
    private UnityEvent m_OnSwitchedFromNormal;

    public UnityEvent OnSwitchedFromNormal
    {
      get { return m_OnSwitchedFromNormal; }
      set { m_OnSwitchedFromNormal = value; }
    }

    WebXRState currentState = WebXRState.NORMAL;

    private void OnEnable()
    {
      currentState = WebXRManager.Instance.XRState;
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
      if (currentState != state)
      {
        switch (currentState)
        {
          case WebXRState.AR:
            m_OnSwitchedFromAR?.Invoke();
            break;
          case WebXRState.VR:
            m_OnSwitchedFromVR?.Invoke();
            break;
          case WebXRState.NORMAL:
            m_OnSwitchedFromNormal?.Invoke();
            break;
        }
      }
      currentState = state;
      switch (state)
      {
        case WebXRState.AR:
          m_OnSwitchedToAR?.Invoke();
          break;
        case WebXRState.VR:
          m_OnSwitchedToVR?.Invoke();
          break;
        case WebXRState.NORMAL:
          m_OnSwitchedToNormal?.Invoke();
          break;
      }
    }
  }
}
