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

    private void onControllerUpdate(WebXRControllerData2 controllerData)
    {
      if (controllerData.hand == (int)hand)
      {
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

    void OnEnable()
    {
      if (inputMap == null)
      {
        Debug.LogError("A Input Map must be assigned to WebXRController!");
        return;
      }
      WebXRManager.Instance.OnControllerUpdate += onControllerUpdate;
      WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;
      SetVisible(false);
    }

    void OnDisabled()
    {
      WebXRManager.Instance.OnControllerUpdate -= onControllerUpdate;
      WebXRManager.Instance.OnHeadsetUpdate -= onHeadsetUpdate;
      SetVisible(false);
    }
  }
}
