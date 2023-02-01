/*
Default layers:
16 Webcam
17 WebcamLighting
18 Spectator

Expected Hierarchy:
- SpectatorCameraHolder
  - SpectatorCamera (Camera, SpectatorCamera)
    - StackCameras
      - SpectatorBackgroundCamera (Camera)
      - SpectatorWebcamCamera (Camera)
      - SpectatorForegroundCamera (Camera)
      - Background (Quad MeshFilter, MeshRenderer, UnlitTransparent Material)
      - Foreground (Quad MeshFilter, MeshRenderer, UnlitTransparent Material)
  - MixedRealityCaptureController (MixedRealityCaptureController)
    - CameraPoint (Visual reference for moving point)
    - TopPoint (Visual reference for moving point)
    - BottomPoint (Visual reference for moving point)
  - WebcamHolder
    - WebcamQuad (Quad MeshFilter, MeshRenderer, ChromaKeyUnlit Material)
      - WebcamLightingQuad (Quad MeshFilter, MeshRenderer, White Legacy Diffuse Material)
    - CameraHint (Visual reference for point on webcam)
    - TopHint (Visual reference for point on webcam)
    - BottomHint (Visual reference for point on webcam)
*/
using System.Collections;
using UnityEngine;

namespace WebXR.Interactions
{
  public class MixedRealityCaptureController : MonoBehaviour
  {
    private enum ControllerState
    {
      None,
      SetCameraPoint,
      SetTopPoint,
      SetBottomPoint,
      Confirm,
      CalcAndSet,
      Playing,
      Ended
    }

    private const float WEBCAM_DISTANCE_CALIBRATION = 0.5f;
    private const float WEBCAM_SIZE_CALIBRATION = 0.25f;
    private const float WEBCAM_MIN_SIZE = 0.001f;
    private readonly Quaternion LEFT_STEP_ROTATION = Quaternion.Euler(0, -90, 0);

    [SerializeField]
    private bool enableInXR = false;
    private WebXRState currentXRState = WebXRState.NORMAL;

    [SerializeField]
    private LayerMask webcamLayer;
    [SerializeField]
    private LayerMask mixedRealityOnLayers;
    [SerializeField]
    private Transform camerasBase;
    [SerializeField]
    private Transform cameraFollower;
    [SerializeField]
    private Camera[] xrCameras;
    [SerializeField]
    private Camera spectatorCamera;
    [SerializeField]
    private GameObject stackCameras;
    [SerializeField]
    private Camera spectatorBackgroundCamera;
    [SerializeField]
    private Camera spectatorForegroundCamera;
    [SerializeField]
    private Camera spectatorWebcamLightingCamera;
    [SerializeField]
    private Transform spectatorCameraTransform;
    [SerializeField]
    private Transform spectatorCameraParent;
    [SerializeField]
    private Renderer backgroundPlaneRenderer;
    [SerializeField]
    private Renderer foregroundPlaneRenderer;
    [SerializeField]
    private Transform backgroundPlaneTransform;
    [SerializeField]
    private Transform foregroundPlaneTransform;
    [SerializeField]
    private Material defaultPlaneMaterial;
    [SerializeField]
    private Transform webcamParent;
    [SerializeField]
    private PlayWebcam webcam;
    [SerializeField]
    private Transform calibrationPointCamera;
    [SerializeField]
    private Transform calibrationPointTop;
    [SerializeField]
    private Transform calibrationPointBottom;
    [SerializeField]
    private GameObject calibrationHintCamera;
    [SerializeField]
    private GameObject calibrationHintTop;
    [SerializeField]
    private GameObject calibrationHintBottom;
    [SerializeField]
    private WebXRController leftController;
    [SerializeField]
    private WebXRController rightController;
    [SerializeField]
    private int webcamFramesDelaySize = 0;

    private ControllerState state = ControllerState.None;
    private float webcamBaseSize = 1f;

    private RenderTexture[] backgroundStack;
    private RenderTexture[] foregroundStack;
    private RenderTexture[] webcamStack;
    private int currentRenderFrame = 0;
    private Material backgroundDisplay;
    private Material foregroundDisplay;

    private Vector3 storedSpectatorParentPosition;
    private Quaternion storedSpectatorParentRotation;
    private Vector3 storedSpectatorPosition;
    private Quaternion storedSpectatorRotation;
    private Vector3 storedWebcamParentPosition;
    private Quaternion storedWebcamParentRotation;
    private Vector3 storedWebcamParentScale;
    private int storedSpectatorCullingMask;
    private CameraClearFlags storedSpectatorClearFlags;
    private float storedSpectatorFieldOfView;
    private float sotredSpectatorNearClipPlane;
    private float sotredSpectatorFarClipPlane;
    private bool storedSpectatorOrthographic;
    private float storedSpectatorOrthographicSize;
    private bool storedWebcamParentActive;

    private void OnEnable()
    {
      WebXRManager.OnXRChange += HandleOnXRChange;
      currentXRState = WebXRManager.Instance.XRState;
      TryUpdateControllerState();
    }

    private void OnDisable()
    {
      StopAllCoroutines();
      WebXRManager.OnXRChange -= HandleOnXRChange;
      Ended();
    }

    private void HandleOnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      currentXRState = state;
      TryUpdateControllerState();
    }

    public void EnableInXR(bool value)
    {
      enableInXR = value;
      TryUpdateControllerState();
    }

    public void TrySetFramesDelay(string value)
    {
      if ((state == ControllerState.None || state == ControllerState.Ended)
          && int.TryParse(value, out int intValue))
      {
        webcamFramesDelaySize = Mathf.Clamp(intValue + 1, 0, 100);
      }
    }

    public void TrySetFramesDelay(int value)
    {
      if ((state == ControllerState.None || state == ControllerState.Ended))
      {
        webcamFramesDelaySize = Mathf.Clamp(value + 1, 0, 100);
      }
    }

    private void TryUpdateControllerState()
    {
      bool enableState = enableInXR && currentXRState != WebXRState.NORMAL;
      if (enableState && (state == ControllerState.None || state == ControllerState.Ended))
      {
        StartController();
      }
      else if (!enableState && state != ControllerState.Ended)
      {
        state = ControllerState.Ended;
      }
    }

    private void StartController()
    {
      StartCoroutine(ControllerProcess());
    }

    private IEnumerator ControllerProcess()
    {
      Prepare();
      while (state != ControllerState.Ended)
      {
        switch (state)
        {
          case ControllerState.SetCameraPoint:
            WhileCalibrating();
            SetPoint(calibrationPointCamera, calibrationHintCamera, ControllerState.SetTopPoint, calibrationHintTop, calibrationPointTop);
            break;
          case ControllerState.SetTopPoint:
            WhileCalibrating();
            SetPoint(calibrationPointTop, calibrationHintTop, ControllerState.SetBottomPoint, calibrationHintBottom, calibrationPointBottom);
            break;
          case ControllerState.SetBottomPoint:
            WhileCalibrating();
            SetBottomPoint();
            break;
          case ControllerState.Confirm:
            WhileCalibrating();
            Confirm();
            break;
          case ControllerState.CalcAndSet:
            CalcAndSet();
            RemoveWebcamLayer();
            state = ControllerState.Playing;
            break;
          case ControllerState.Playing:
            Playing();
            break;
        }
        yield return null;
      }
      Ended();
    }

    private void Prepare()
    {
      state = ControllerState.None;

      storedSpectatorParentPosition = spectatorCameraParent.position;
      storedSpectatorParentRotation = spectatorCameraParent.rotation;
      storedSpectatorPosition = spectatorCameraTransform.position;
      storedSpectatorRotation = spectatorCameraTransform.rotation;
      storedSpectatorCullingMask = spectatorCamera.cullingMask;
      storedSpectatorClearFlags = spectatorCamera.clearFlags;
      storedSpectatorFieldOfView = spectatorCamera.fieldOfView;
      sotredSpectatorNearClipPlane = spectatorCamera.nearClipPlane;
      sotredSpectatorFarClipPlane = spectatorCamera.farClipPlane;
      storedSpectatorOrthographic = spectatorCamera.orthographic;
      storedSpectatorOrthographicSize = spectatorCamera.orthographicSize;
      storedWebcamParentPosition = webcamParent.position;
      storedWebcamParentRotation = webcamParent.rotation;
      storedWebcamParentScale = webcamParent.localScale;
      storedWebcamParentActive = webcamParent.gameObject.activeSelf;

      calibrationPointCamera.gameObject.SetActive(false);
      calibrationPointTop.gameObject.SetActive(false);
      calibrationPointBottom.gameObject.SetActive(false);
      calibrationHintCamera.SetActive(true);
      calibrationHintTop.SetActive(false);
      calibrationHintBottom.SetActive(false);
      webcamParent.localScale = Vector3.one * WEBCAM_SIZE_CALIBRATION;
      webcamParent.gameObject.SetActive(true);
      AddWebcamLayer();
      spectatorCameraParent.SetPositionAndRotation(camerasBase.position, camerasBase.rotation);
      state = ControllerState.SetCameraPoint;
      calibrationPointCamera.gameObject.SetActive(true);
    }

    private void Ended()
    {
      calibrationPointCamera.gameObject.SetActive(false);
      calibrationPointTop.gameObject.SetActive(false);
      calibrationPointBottom.gameObject.SetActive(false);
      calibrationHintCamera.SetActive(false);
      calibrationHintTop.SetActive(false);
      calibrationHintBottom.SetActive(false);
      webcamParent.gameObject.SetActive(storedWebcamParentActive);
      stackCameras.SetActive(false);
      ClearRenderTextures();
      spectatorCameraParent.position = storedSpectatorParentPosition;
      spectatorCameraParent.rotation = storedSpectatorParentRotation;
      spectatorCameraTransform.position = storedSpectatorPosition;
      spectatorCameraTransform.rotation = storedSpectatorRotation;
      spectatorCamera.cullingMask = storedSpectatorCullingMask;
      spectatorCamera.clearFlags = storedSpectatorClearFlags;
      spectatorCamera.fieldOfView = storedSpectatorFieldOfView;
      spectatorCamera.nearClipPlane = sotredSpectatorNearClipPlane;
      spectatorCamera.farClipPlane = sotredSpectatorFarClipPlane;
      spectatorCamera.orthographic = storedSpectatorOrthographic;
      spectatorCamera.orthographicSize = storedSpectatorOrthographicSize;
      webcamParent.position = storedWebcamParentPosition;
      webcamParent.rotation = storedWebcamParentRotation;
      webcamParent.localScale = storedWebcamParentScale;
      webcam.TrySetLightingTexture(null);
    }

    private void SetPoint(Transform point, GameObject hint, ControllerState nextState, GameObject nextHint, Transform nextPoint)
    {
      point.position = rightController.transform.position;
      if (GetControllersButtonDown())
      {
        hint.SetActive(false);
        nextHint.SetActive(true);
        nextPoint.gameObject.SetActive(true);
        state = nextState;
      }
    }

    private void SetBottomPoint()
    {
      float cameraToTopDistance = Vector3.Distance(calibrationPointCamera.position, calibrationPointTop.position);
      Vector3 cameraToBottomDirection = (rightController.transform.position - calibrationPointCamera.position).normalized;
      calibrationPointBottom.position = calibrationPointCamera.position + cameraToBottomDirection * cameraToTopDistance;
      if (GetControllersButtonDown())
      {
        calibrationHintBottom.SetActive(false);
        state = ControllerState.Confirm;
      }
    }

    private void Confirm()
    {
      if (GetControllersButtonDown())
      {
        calibrationPointCamera.gameObject.SetActive(false);
        calibrationPointTop.gameObject.SetActive(false);
        calibrationPointBottom.gameObject.SetActive(false);
        state = ControllerState.CalcAndSet;
      }
    }

    private void CalcAndSet()
    {
      SetCameraVerticalFOV();
      SetCameraPositionRotation();
      SetupRenders();
    }

    private void WhileCalibrating()
    {
      spectatorCameraTransform.SetPositionAndRotation(cameraFollower.position, cameraFollower.rotation);
      webcamParent.SetPositionAndRotation(cameraFollower.position + cameraFollower.forward * WEBCAM_DISTANCE_CALIBRATION, cameraFollower.rotation);
    }

    private void Playing()
    {
      var headPosition = spectatorCameraTransform.InverseTransformPoint(cameraFollower.position);
      var newWebcamPosition = spectatorCameraTransform.TransformPoint(new Vector3(0, 0, headPosition.z));
      webcamParent.SetPositionAndRotation(newWebcamPosition, spectatorCameraTransform.rotation);
      if (webcamFramesDelaySize < 2)
      {
        float webcamSize = Mathf.Max(Mathf.LerpUnclamped(0f, webcamBaseSize, headPosition.z), WEBCAM_MIN_SIZE);
        webcamParent.localScale = Vector3.one * webcamSize;
        return;
      }
      PlayUsingRenders(headPosition.z);
    }

    private void SetCameraVerticalFOV()
    {
      float distanceA = Vector3.Distance(calibrationPointTop.position, calibrationPointBottom.position);
      float distanceB = Vector3.Distance(calibrationPointCamera.position, calibrationPointTop.position);
      float distanceC = Vector3.Distance(calibrationPointCamera.position, calibrationPointBottom.position);
      float A = Mathf.Acos((distanceB * distanceB + distanceC * distanceC - distanceA * distanceA) / (2 * distanceB * distanceC));
      spectatorCamera.fieldOfView = Mathf.Rad2Deg * A;
      webcamBaseSize = Mathf.Tan(A * 0.5f) * 2f; // Half of FOV as direction * 2
    }

    private void SetCameraPositionRotation()
    {
      Plane plane = new Plane(calibrationPointCamera.position, calibrationPointTop.position, calibrationPointBottom.position);
      Vector3 up = (calibrationPointTop.position - calibrationPointBottom.position).normalized;
      spectatorCameraTransform.SetPositionAndRotation(calibrationPointCamera.position, Quaternion.LookRotation(plane.normal, up) * LEFT_STEP_ROTATION);
    }

    private bool GetControllersButtonDown()
    {
      bool leftDown = (leftController.isHandActive || leftController.isControllerActive)
                      && leftController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
      if (leftDown)
      {
        return true;
      }
      bool rightDown = (rightController.isHandActive || rightController.isControllerActive)
                      && rightController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
      if (rightDown)
      {
        return true;
      }
      return false;
    }

    private void AddWebcamLayer()
    {
      for (int i = 0; i < xrCameras.Length; i++)
      {
        if (xrCameras[i] == null)
        {
          Debug.LogError("Missing reference to XR Camera");
        }
        else
        {
          xrCameras[i].cullingMask |= webcamLayer;
        }
      }
    }

    private void RemoveWebcamLayer()
    {
      for (int i = 0; i < xrCameras.Length; i++)
      {
        if (xrCameras[i] == null)
        {
          Debug.LogError("Missing reference to XR Camera");
        }
        else
        {
          xrCameras[i].cullingMask &= ~webcamLayer;
        }
      }
    }

    private void SetupRenders()
    {
      if (webcamFramesDelaySize < 2)
      {
        return;
      }
      spectatorBackgroundCamera.fieldOfView = spectatorCamera.fieldOfView;
      spectatorForegroundCamera.fieldOfView = spectatorCamera.fieldOfView;
      spectatorWebcamLightingCamera.fieldOfView = spectatorCamera.fieldOfView;
      spectatorBackgroundCamera.nearClipPlane = sotredSpectatorNearClipPlane;
      spectatorForegroundCamera.nearClipPlane = sotredSpectatorNearClipPlane;
      spectatorWebcamLightingCamera.nearClipPlane = sotredSpectatorNearClipPlane;
      spectatorBackgroundCamera.farClipPlane = sotredSpectatorFarClipPlane;
      spectatorForegroundCamera.farClipPlane = sotredSpectatorFarClipPlane;
      spectatorWebcamLightingCamera.farClipPlane = sotredSpectatorFarClipPlane;
      CreateRenderTextures();
      backgroundPlaneRenderer.sharedMaterial = Instantiate(defaultPlaneMaterial);
      foregroundPlaneRenderer.sharedMaterial = Instantiate(defaultPlaneMaterial);
      spectatorCamera.orthographic = true;
      spectatorCamera.orthographicSize = 0.5f;
      spectatorCamera.cullingMask = mixedRealityOnLayers;
      spectatorCamera.clearFlags = CameraClearFlags.Color;
      stackCameras.SetActive(true);
      webcamParent.localScale = Vector3.one;
      float ratio = (float)Screen.width / (float)Screen.height;
      backgroundPlaneTransform.localScale = new Vector3(ratio, 1, 1);
      foregroundPlaneTransform.localScale = new Vector3(ratio, 1, 1);
    }

    private void PlayUsingRenders(float headDistance)
    {
      headDistance = Mathf.Clamp(headDistance, sotredSpectatorNearClipPlane + 0.01f, sotredSpectatorFarClipPlane - 0.01f);
      spectatorBackgroundCamera.nearClipPlane = headDistance;
      spectatorForegroundCamera.farClipPlane = headDistance;
      spectatorBackgroundCamera.targetTexture = backgroundStack[currentRenderFrame];
      spectatorForegroundCamera.targetTexture = foregroundStack[currentRenderFrame];
      spectatorWebcamLightingCamera.targetTexture = webcamStack[currentRenderFrame];
      currentRenderFrame = (currentRenderFrame + 1) % webcamFramesDelaySize;
      backgroundPlaneRenderer.sharedMaterial.mainTexture = backgroundStack[currentRenderFrame];
      foregroundPlaneRenderer.sharedMaterial.mainTexture = foregroundStack[currentRenderFrame];
      webcam.TrySetLightingTexture(webcamStack[currentRenderFrame]);
      backgroundPlaneTransform.localPosition = new Vector3(0, 0, headDistance + 0.005f);
      foregroundPlaneTransform.localPosition = new Vector3(0, 0, headDistance - 0.005f);
    }

    private void ClearRenderTextures()
    {
      if (backgroundStack == null)
      {
        return;
      }
      for (int i = 0; i < backgroundStack.Length; i++)
      {
        backgroundStack[i].Release();
        foregroundStack[i].Release();
        webcamStack[i].Release();
        Destroy(backgroundStack[i]);
        Destroy(foregroundStack[i]);
        Destroy(webcamStack[i]);
      }
      backgroundStack = null;
      foregroundStack = null;
      webcamStack = null;
      Destroy(backgroundPlaneRenderer.sharedMaterial);
      Destroy(foregroundPlaneRenderer.sharedMaterial);
    }

    private void CreateRenderTextures()
    {
      backgroundStack = new RenderTexture[webcamFramesDelaySize];
      foregroundStack = new RenderTexture[webcamFramesDelaySize];
      webcamStack = new RenderTexture[webcamFramesDelaySize];
      RenderTextureDescriptor envDesc = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default);
      RenderTextureDescriptor webcamDesc = new RenderTextureDescriptor(Mathf.RoundToInt(100 * webcam.transform.localScale.x), 100, RenderTextureFormat.Default);
      for (int i = 0; i < webcamFramesDelaySize; i++)
      {
        backgroundStack[i] = new RenderTexture(envDesc);
        foregroundStack[i] = new RenderTexture(envDesc);
        webcamStack[i] = new RenderTexture(webcamDesc);
      }
    }
  }
}
