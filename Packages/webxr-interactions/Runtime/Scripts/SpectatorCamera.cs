using UnityEngine;

namespace WebXR.Interactions
{
  /// <summary>
  /// Controls the Spectator Camera, enable render it when in XR mode.
  /// Notice that the Depth of the attached Camera should be higher than the other cameras on the scene.
  /// </summary>
  [RequireComponent(typeof(Camera))]
  public class SpectatorCamera : MonoBehaviour
  {
    private Camera _camera;
    [SerializeField]
    private bool enableInXR = false;
    private WebXRState currentXRState = WebXRState.NORMAL;

    private void Awake()
    {
      _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
      WebXRManager.OnXRChange += HandleOnXRChange;
      currentXRState = WebXRManager.Instance.XRState;
      TryUpdateCameraState();
    }

    private void OnDisable()
    {
      WebXRManager.OnXRChange -= HandleOnXRChange;
    }

    private void OnPreRender()
    {
      WebXRManager.Instance.PreRenderSpectatorCamera();
    }

    private void HandleOnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      currentXRState = state;
      TryUpdateCameraState();
    }

    private void TryUpdateCameraState()
    {
      bool newState = enableInXR && currentXRState != WebXRState.NORMAL;
      if (_camera.enabled != newState)
      {
        _camera.enabled = newState;
      }
    }

    public void EnableCameraInXR(bool value)
    {
      enableInXR = value;
      TryUpdateCameraState();
    }
  }
}
