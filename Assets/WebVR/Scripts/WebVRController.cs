using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

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
    [Tooltip("Simulate 3dof controller")]
    public bool simulate3dof = false;
    [Tooltip("Vector from head to elbow")]
    public Vector3 eyesToElbow = new Vector3(0.1f, -0.4f, 0.15f);
    [Tooltip("Vector from elbow to hand")]
    public Vector3 elbowHand = new Vector3(0, 0, 0.25f);
    private Matrix4x4 sitStand;
    private float[] axes;

    private XRNode handNode;
    private Quaternion headRotation;
    private Vector3 headPosition;
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

    private void onHeadsetUpdate(Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix)
    {
        Matrix4x4 trs = WebVRMatrixUtil.TransformViewMatrixToTRS(leftViewMatrix);
        this.headRotation = WebVRMatrixUtil.GetRotationFromMatrix(trs);
        this.headPosition = WebVRMatrixUtil.GetTranslationFromMatrix(trs);
        this.sitStand = sitStandMatrix;
    }

    private void onControllerUpdate(string id,
        int index,
        string handValue,
        bool hasOrientation,
        bool hasPosition,
        Quaternion orientation,
        Vector3 position,
        Vector3 linearAcceleration,
        Vector3 linearVelocity,
        WebVRControllerButton[] buttonValues,
        float[] axesValues)
    {
        if (handFromString(handValue) == hand)
        {
            SetVisible(true);

            Quaternion sitStandRotation = WebVRMatrixUtil.GetRotationFromMatrix(this.sitStand);
            Quaternion rotation = sitStandRotation * orientation;

            if (!hasPosition || this.simulate3dof) {
                position = applyArmModel(
                    this.sitStand.MultiplyPoint(this.headPosition),
                    rotation,
                    this.headRotation);
            }
            else
            {
                position = this.sitStand.MultiplyPoint(position);
            }

            transform.rotation = rotation;
            transform.position = position;

            UpdateButtons(buttonValues);
            this.axes = axesValues;
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

    private void SetVisible(bool visible) {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>();
        {
            foreach (Renderer component in rendererComponents) {
                component.enabled = visible;
            }
        }
    }

    // Arm model adapted from: https://github.com/aframevr/aframe/blob/master/src/components/tracked-controls.js
    private Vector3 applyArmModel(Vector3 controllerPosition, Quaternion controllerRotation, Quaternion headRotation)
    {
        // Set offset for degenerate "arm model" to elbow.
        Vector3 deltaControllerPosition = new Vector3(
            this.eyesToElbow.x * (this.hand == WebVRControllerHand.LEFT ? -1 : this.hand == WebVRControllerHand.RIGHT ? 1 : 0),
            this.eyesToElbow.y,
            this.eyesToElbow.z);

        // Apply camera Y rotation (not X or Z, so you can look down at your hand).
        Quaternion headYRotation = Quaternion.Euler(0, headRotation.eulerAngles.y, 0);
        deltaControllerPosition = (headYRotation * deltaControllerPosition);
        controllerPosition += deltaControllerPosition;

        // Set offset for forearm sticking out from elbow.
        deltaControllerPosition.Set(this.elbowHand.x, this.elbowHand.y, this.elbowHand.z);
        deltaControllerPosition = Quaternion.Euler(controllerRotation.eulerAngles.x, controllerRotation.eulerAngles.y, 0) * deltaControllerPosition;
        controllerPosition += deltaControllerPosition;
        return controllerPosition;
    }

    void Update()
    {
        // Use Unity XR Input when enabled. When using WebVR, updates are performed onControllerUpdate.
        if (XRDevice.isPresent)
        {
            SetVisible(true);

            if (this.hand == WebVRControllerHand.LEFT)
                handNode = XRNode.LeftHand;

            if (this.hand == WebVRControllerHand.RIGHT)
               handNode = XRNode.RightHand;

            if (this.simulate3dof)
            {
                transform.position = this.applyArmModel(
                    InputTracking.GetLocalPosition(XRNode.Head), // we use head position as origin
                    InputTracking.GetLocalRotation(handNode),
                    InputTracking.GetLocalRotation(XRNode.Head)
                );
                transform.rotation = InputTracking.GetLocalRotation(handNode);
            }
            else
            {
                transform.position = InputTracking.GetLocalPosition(handNode);
                transform.rotation = InputTracking.GetLocalRotation(handNode);
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
        WebVRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;
        SetVisible(false);
    }

    void OnDisabled()
    {
        WebVRManager.Instance.OnControllerUpdate -= onControllerUpdate;
        WebVRManager.Instance.OnHeadsetUpdate -= onHeadsetUpdate;
        SetVisible(false);
    }
}
