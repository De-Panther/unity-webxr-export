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
    private Transform camerasBase;
    [SerializeField]
    private Transform cameraFollower;
    [SerializeField]
    private Camera[] xrCameras;
    [SerializeField]
    private Camera spectatorCamera;
    [SerializeField]
    private Transform spectatorCameraTransform;
    [SerializeField]
    private Transform spectatorCameraParent;
    [SerializeField]
    private Transform webcamParent;
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

    private ControllerState state = ControllerState.None;
    private float webcamBaseSize = 1f;

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
      state = ControllerState.None;
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
      while (state != ControllerState.Ended)
      {
        switch (state)
        {
          case ControllerState.SetCameraPoint:
            WhileCalibrating();
            SetPoint(calibrationPointCamera, calibrationHintCamera, ControllerState.SetTopPoint, calibrationHintTop);
            break;
          case ControllerState.SetTopPoint:
            WhileCalibrating();
            SetPoint(calibrationPointTop, calibrationHintTop, ControllerState.SetBottomPoint, calibrationHintBottom);
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

    private void Ended()
    {
      calibrationPointCamera.gameObject.SetActive(false);
      calibrationPointTop.gameObject.SetActive(false);
      calibrationPointBottom.gameObject.SetActive(false);
      webcamParent.gameObject.SetActive(false);
    }

    private void SetPoint(Transform point, GameObject hint, ControllerState nextState, GameObject nextHint)
    {
      if (GetControllersButtonDown(out Vector3 position))
      {
        point.position = position;
        point.gameObject.SetActive(true);
        hint.SetActive(false);
        nextHint.SetActive(true);
        state = nextState;
      }
    }

    private void SetBottomPoint()
    {
      if (GetControllersButtonDown(out Vector3 position))
      {
        float cameraToTopDistance = Vector3.Distance(calibrationPointCamera.position, calibrationPointTop.position);
        Vector3 cameraToBottomDirection = (position - calibrationPointCamera.position).normalized;
        calibrationPointBottom.position = calibrationPointCamera.position + cameraToBottomDirection * cameraToTopDistance;
        calibrationPointBottom.gameObject.SetActive(true);
        calibrationHintBottom.SetActive(false);
        state = ControllerState.Confirm;
      }
    }

    private void Confirm()
    {
      if (GetControllersButtonDown(out Vector3 position))
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
      float webcamSize = Mathf.Max(Mathf.LerpUnclamped(0f, webcamBaseSize, headPosition.z), WEBCAM_MIN_SIZE);
      webcamParent.localScale = Vector3.one * webcamSize;
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

    private bool GetControllersButtonDown(out Vector3 position)
    {
      bool leftDown = (leftController.isHandActive || leftController.isControllerActive)
                      && leftController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
      if (leftDown)
      {
        position = leftController.transform.position;
        return true;
      }
      bool rightDown = (rightController.isHandActive || rightController.isControllerActive)
                      && rightController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
      if (rightDown)
      {
        position = rightController.transform.position;
        return true;
      }
      position = Vector3.zero;
      return false;
    }

    private void AddWebcamLayer()
    {
      for (int i = 0; i < xrCameras.Length; i++)
      {
        xrCameras[i].cullingMask |= webcamLayer;
      }
    }

    private void RemoveWebcamLayer()
    {
      for (int i = 0; i < xrCameras.Length; i++)
      {
        xrCameras[i].cullingMask &= ~webcamLayer;
      }
    }
  }
}
