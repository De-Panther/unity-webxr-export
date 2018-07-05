using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum WebVRControllerHand { NONE, LEFT, RIGHT };

[System.Serializable]
public class WebVRControllerButton
{
    public bool pressed;
    public bool prevPressedState;
    public bool touched;
    public float value;

    public WebVRControllerButton(bool isPressed, float buttonValue)
    {
        pressed = isPressed;
        prevPressedState = false;
        value = buttonValue;
    }
}

public class WebVRController : MonoBehaviour
{
    [Tooltip("Controller hand to use.")]
    public WebVRControllerHand hand = WebVRControllerHand.NONE;
    [Tooltip("Controller input settings.")]
    public WebVRControllerInputMap inputMap;
    [HideInInspector]
    public int index;
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Quaternion rotation;
    [HideInInspector]
    public Matrix4x4 sitStand;
    private float[] axes;

    private Dictionary<string, WebVRControllerButton> buttonStates = new Dictionary<string, WebVRControllerButton>();

    // Updates button states from Web gamepad API.
    private void UpdateButtons(WebVRControllerButton[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            WebVRControllerButton button = buttons[i];
            foreach (WebVRControllerInput input in inputMap.inputs)
            {
                if (input.gamepadId == i)
                    SetButtonState(input.actionName, button.pressed, button.value);
            }
        }
    }

    public float GetAxis(string action)
    {
        for (var i = 0; i < inputMap.inputs.Count; i++)
        {
            WebVRControllerInput input = inputMap.inputs[i];
            if (action == input.actionName)
            {
                if (UnityEngine.XR.XRDevice.isPresent && !input.unityInputIsButton)
                {
                    return Input.GetAxis(input.unityInputName);
                }
                else
                {
                    if (input.gamepadIsButton)
                    {
                        if (!buttonStates.ContainsKey(action))
                            return 0;
                        return buttonStates[action].value;
                    }
                    else
                        return axes[i];
                }
            }
        }
        return 0;
    }

    public bool GetButton(string action)
    {
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            foreach(WebVRControllerInput input in inputMap.inputs)
            {
                if (action == input.actionName && input.unityInputIsButton)
                    return Input.GetButton(input.unityInputName);
            }
        }

        if (!buttonStates.ContainsKey(action))
            return false;
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
            buttonStates.Add(action, new WebVRControllerButton(isPressed, value));
    }

    private void SetPastButtonState(string action, bool isPressed)
    {
        if (!buttonStates.ContainsKey(action))
            return;
        buttonStates[action].prevPressedState = isPressed;
    }

    public bool GetButtonDown(string action)
    {
        // Use Unity Input Manager when XR is enabled and WebVR is not being used (eg: standalone or from within editor).
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            foreach(WebVRControllerInput input in inputMap.inputs)
            {
                if (action == input.actionName && input.unityInputIsButton)
                    return Input.GetButtonDown(input.unityInputName);
            }
        }

        if (GetButton(action) && !GetPastButtonState(action))
        {
            SetPastButtonState(action, true);
            return true;
        }
        return false;
    }

    public bool GetButtonUp(string action)
    {
        // Use Unity Input Manager when XR is enabled and WebVR is not being used (eg: standalone or from within editor).
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            foreach(WebVRControllerInput input in inputMap.inputs)
            {
                if (action == input.actionName && input.unityInputIsButton)
                    return Input.GetButtonUp(input.unityInputName);
            }
        }

        if (!GetButton(action) && GetPastButtonState(action))
        {
            SetPastButtonState(action, false);
            return true;
        }
        return false;
    }

    private void onControllerUpdate(
        int index, string handValue, Vector3 position, Quaternion rotation, Matrix4x4 sitStand, WebVRControllerButton[] buttonValues, float[] axesValues)
    {
        if (handFromString(handValue) == hand)
        {
            // Apply controller orientation and position.
            Quaternion sitStandRotation = Quaternion.LookRotation (
                sitStand.GetColumn (2),
                sitStand.GetColumn (1)
            );
            transform.rotation = sitStandRotation * rotation;
            transform.position = sitStand.MultiplyPoint(position);

            UpdateButtons(buttonValues);
            axes = axesValues;
        }	
    }

    private WebVRControllerHand handFromString(string handValue)
    {
        WebVRControllerHand handParsed = WebVRControllerHand.NONE;

        if (!String.IsNullOrEmpty(handValue)) {
            try
            {
                handParsed = (WebVRControllerHand) Enum.Parse(typeof(WebVRControllerHand), handValue.ToUpper(), true);
            }
            catch
            {
                Debug.LogError("Unrecognized controller Hand '" + handValue + "'!");
            }
        }
        return handParsed;
    }

    void Update()
    {
        // Use Unity XR Input when enabled. When using WebVR, updates are performed onControllerUpdate.
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            if (hand == WebVRControllerHand.LEFT)
            {
                transform.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
                transform.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
            }

            if (hand == WebVRControllerHand.RIGHT)
            {
                transform.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
                transform.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
            }

            foreach(WebVRControllerInput input in inputMap.inputs)
            {
                if (!input.unityInputIsButton)
                {
                    if (Input.GetAxis(input.unityInputName) != 0)
                        SetButtonState(input.actionName, true, Input.GetAxis(input.unityInputName));
                    if (Input.GetAxis(input.unityInputName) < 1)
                        SetButtonState(input.actionName, false, Input.GetAxis(input.unityInputName));
                }
            }
        }
    }

    void OnEnable()
    {
        if (inputMap == null)
        {
            Debug.LogError("A Input Map must be assigned to WebVRController!");
            return;
        }
        WebVRManager.Instance.OnControllerUpdate += onControllerUpdate;
    }

    void OnDisabled()
    {
        WebVRManager.Instance.OnControllerUpdate -= onControllerUpdate;
    }
}
