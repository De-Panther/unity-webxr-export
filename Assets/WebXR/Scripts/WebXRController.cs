using UnityEngine;
using System;
using System.Collections.Generic;

namespace WebXR
{

  public class WebXRController : MonoBehaviour
  {
    [Tooltip("Controller hand to use.")]
    public WebXRControllerHand hand = WebXRControllerHand.NONE;
    [Tooltip("Controller input settings.")]
    public WebXRControllerInputMap inputMap;
    [Tooltip("Simulate 3dof controller")]
    public bool simulate3dof = false;
    [Tooltip("Vector from head to elbow")]
    public Vector3 eyesToElbow = new Vector3(0.1f, -0.4f, 0.15f);
    [Tooltip("Vector from elbow to hand")]
    public Vector3 elbowHand = new Vector3(0, 0, 0.25f);

    public Transform handJointPrefab;


    public GameObject[] showGOs;

    private Matrix4x4 sitStand;

    private float trigger;
    private float squeeze;
    private float thumbstick;
    private float thumbstickX;
    private float thumbstickY;
    private float touchpad;
    private float touchpadX;
    private float touchpadY;
    private float buttonA;
    private float buttonB;

    private Quaternion headRotation;
    private Vector3 headPosition;
    private Dictionary<string, WebXRControllerButton> buttonStates = new Dictionary<string, WebXRControllerButton>();

    private Dictionary<int, Transform> handJoints = new Dictionary<int, Transform>();
    private bool handJointsVisible = false;

    // Updates button states from Web gamepad API.
    private void UpdateButtons(WebXRControllerButton[] buttons)
    {
      for (int i = 0; i < buttons.Length; i++)
      {
        WebXRControllerButton button = buttons[i];
        foreach (WebXRControllerInput input in inputMap.inputs)
        {
          if (input.gamepadId == i)
            SetButtonState(input.actionName, button.pressed, button.value);
        }
      }
    }

    public float GetAxis(string action)
    {
      switch (action)
      {
        case "Grip":
          return squeeze;
        case "Trigger":
          return trigger;
      }
      return 0;
    }

    public bool GetButton(string action)
    {
      if (!buttonStates.ContainsKey(action))
      {
        return false;
      }
      return buttonStates[action].pressed;
    }

    private bool GetPastButtonState(string action)
    {
      if (!buttonStates.ContainsKey(action))
        return false;
      return buttonStates[action].prevPressedState;
    }

    private void SetButtonState(string action, bool isPressed, float value)
    {
      if (buttonStates.ContainsKey(action))
      {
        buttonStates[action].pressed = isPressed;
        buttonStates[action].value = value;
      }
      else
        buttonStates.Add(action, new WebXRControllerButton(isPressed, value));
    }

    private void SetPastButtonState(string action, bool isPressed)
    {
      if (!buttonStates.ContainsKey(action))
        return;
      buttonStates[action].prevPressedState = isPressed;
    }

    public bool GetButtonDown(string action)
    {
      if (GetButton(action) && !GetPastButtonState(action))
      {
        SetPastButtonState(action, true);
        return true;
      }
      return false;
    }

    public bool GetButtonUp(string action)
    {
      if (!GetButton(action) && GetPastButtonState(action))
      {
        SetPastButtonState(action, false);
        return true;
      }
      return false;
    }

    private void onHeadsetUpdate(Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix)
    {
      Matrix4x4 trs = WebXRMatrixUtil.TransformViewMatrixToTRS(leftViewMatrix);
      this.headRotation = WebXRMatrixUtil.GetRotationFromMatrix(trs);
      this.headPosition = WebXRMatrixUtil.GetTranslationFromMatrix(trs);
      this.sitStand = sitStandMatrix;
    }

    private void OnControllerUpdate(WebXRControllerData controllerData)
    {
      if (controllerData.hand == (int)hand)
      {
        if (!controllerData.enabled)
        {
          SetVisible(false);
          return;
        }
        SetVisible(true);

        transform.localRotation = controllerData.rotation;
        transform.localPosition = controllerData.position;

        trigger = controllerData.trigger;
        squeeze = controllerData.squeeze;
        thumbstick = controllerData.thumbstick;
        thumbstickX = controllerData.thumbstickX;
        thumbstickY = controllerData.thumbstickY;
        touchpad = controllerData.touchpad;
        touchpadX = controllerData.touchpadX;
        touchpadY = controllerData.touchpadY;
        buttonA = controllerData.buttonA;
        buttonB = controllerData.buttonB;

        WebXRControllerButton[] buttons = new WebXRControllerButton[6];
        buttons[0] = new WebXRControllerButton(trigger==1, trigger);
        buttons[1] = new WebXRControllerButton(squeeze==1, squeeze);
        buttons[2] = new WebXRControllerButton(thumbstick==1, thumbstick);
        buttons[3] = new WebXRControllerButton(touchpad==1, touchpad);
        buttons[4] = new WebXRControllerButton(buttonA==1, buttonA);
        buttons[5] = new WebXRControllerButton(buttonB==1, buttonB);
        UpdateButtons(buttons);
      }
    }

    private void OnHandUpdate(WebXRHandData handData)
    {
      if (handData.hand == (int)hand)
      {
        if (!handData.enabled)
        {
          SetHandJointsVisible(false);
          return;
        }
        SetVisible(false);
        SetHandJointsVisible(true);

        transform.localPosition = handData.joints[0].position;
        transform.localRotation = handData.joints[0].rotation;

        Quaternion rotationOffset = Quaternion.Inverse(handData.joints[0].rotation);

        for(int i=0; i<=WebXRHandData.LITTLE_PHALANX_TIP; i++)
        {
          if (handData.joints[i].enabled)
          {
            if (handJoints.ContainsKey(i))
            {
              handJoints[i].localPosition = rotationOffset * (handData.joints[i].position - handData.joints[0].position);
              handJoints[i].localRotation = rotationOffset * handData.joints[i].rotation;
            }
            else
            {
              var clone = Instantiate(handJointPrefab,
                                      rotationOffset * (handData.joints[i].position - handData.joints[0].position),
                                      rotationOffset * handData.joints[i].rotation,
                                      transform);
              if (handData.joints[i].radius > 0f)
              {
                clone.localScale = new Vector3(handData.joints[i].radius, handData.joints[i].radius, handData.joints[i].radius);
              }
              else
              {
                clone.localScale = new Vector3(0.005f, 0.005f, 0.005f);
              }
              handJoints.Add(i, clone);
            }
          }
        }

        trigger = handData.trigger;
        squeeze = handData.squeeze;

        WebXRControllerButton[] buttons = new WebXRControllerButton[2];
        buttons[0] = new WebXRControllerButton(trigger==1, trigger);
        buttons[1] = new WebXRControllerButton(squeeze==1, squeeze);
        UpdateButtons(buttons);
      }
    }

    private WebXRControllerHand handFromString(string handValue)
    {
      WebXRControllerHand handParsed = WebXRControllerHand.NONE;

      if (!String.IsNullOrEmpty(handValue))
      {
        try
        {
          handParsed = (WebXRControllerHand)Enum.Parse(typeof(WebXRControllerHand), handValue.ToUpper(), true);
        }
        catch
        {
          Debug.LogError("Unrecognized controller Hand '" + handValue + "'!");
        }
      }
      return handParsed;
    }

    private void SetVisible(bool visible)
    {
      foreach (var showGO in showGOs)
      {
        showGO.SetActive(visible);
      }
    }

    private void SetHandJointsVisible(bool visible)
    {
      if (handJointsVisible == visible)
      {
        return;
      }
      handJointsVisible = visible;
      foreach (var handJoint in handJoints)
      {
        handJoint.Value.gameObject.SetActive(visible);
      }
    }

    // intensity 0 to 1, duration milliseconds
    public void Pulse(float intensity, float duration)
    {
      WebXRManager.Instance.HapticPulse(hand, intensity, duration);
    }

    void OnEnable()
    {
      if (inputMap == null)
      {
        Debug.LogError("A Input Map must be assigned to WebXRController!");
        return;
      }
      WebXRManager.Instance.OnControllerUpdate += OnControllerUpdate;
      WebXRManager.Instance.OnHandUpdate += OnHandUpdate;
      WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;
      SetVisible(false);
    }

    void OnDisabled()
    {
      WebXRManager.Instance.OnControllerUpdate -= OnControllerUpdate;
      WebXRManager.Instance.OnHandUpdate -= OnHandUpdate;
      WebXRManager.Instance.OnHeadsetUpdate -= onHeadsetUpdate;
      SetVisible(false);
    }
  }
}
