#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace WebXR.InputSystem
{
  using InputSystem = UnityEngine.InputSystem.InputSystem;

#if UNITY_EDITOR
  [UnityEditor.InitializeOnLoad]
#endif
  [Preserve, InputControlLayout(displayName = "WebXR Tracked Display")]
  internal class WebXRTrackedDisplay : XRHMD
  {
    static WebXRTrackedDisplay()
    {
      InputSystem.RegisterLayout<WebXRTrackedDisplay>(
        matches: new InputDeviceMatcher()
        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
        .WithProduct("WebXR Tracked Display"));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInPlayer() { }
  }

  [Preserve]
  public struct WebXRControllerState : IInputStateTypeInfo
  {
    public static FourCC Format => new FourCC('W', 'X', 'R', 'C');

    public readonly FourCC format => Format;

    [Preserve, InputControl(bit = 0, offset = 0, layout = "Axis", usage = "Trigger")]
    public float trigger;

    [Preserve, InputControl(bit = 0, offset = 4, layout = "Button", usage = "TriggerButton")]
    public bool triggerPressed;

    [Preserve, InputControl(bit = 0, offset = 5, layout = "Button", usage = "TriggerTouch")]
    public bool triggerTouched;

    [Preserve, InputControl(bit = 0, offset = 8, layout = "Axis", usage = "Grip")]
    public float squeeze;

    [Preserve, InputControl(bit = 0, offset = 12, layout = "Button", usage = "GripButton")]
    public bool squeezePressed;

    [Preserve, InputControl(bit = 0, offset = 13, layout = "Button", usage = "GripTouch")]
    public bool squeezeTouched;

    [Preserve, InputControl(bit = 0, offset = 16, layout = "Vector2", usage = "Primary2DAxis",
       alias = "joystick")]
    public Vector2 thumbstick;

    [Preserve, InputControl(bit = 0, offset = 24, layout = "Button", usage = "Primary2DAxisClick",
       aliases = new[] { "thumbstickClicked", "joystickPressed", "joystickClicked"})]
    public bool thumbstickPressed;

    [Preserve, InputControl(bit = 0, offset = 25, layout = "Button", usage = "Primary2DAxisTouch")]
    public bool thumbstickTouched;

    [Preserve, InputControl(bit = 0, offset = 28, layout = "Vector2", usage = "Secondary2DAxis",
       alias = "trackpad")]
    public Vector2 touchpad;

    [Preserve, InputControl(bit = 0, offset = 36, layout = "Button", usage = "Secondary2DAxisClick",
       aliases = new[] { "touchpadClicked", "trackpadPressed", "trackpadClicked"})]
    public bool touchpadPressed;

    [Preserve, InputControl(bit = 0, offset = 37, layout = "Button", usage = "Secondary2DAxisTouch")]
    public bool touchpadTouched;

    [Preserve, InputControl(bit = 0, offset = 38, layout = "Button", usage = "PrimaryButton")]
    public bool buttonA;

    [Preserve, InputControl(bit = 0, offset = 39, layout = "Button", usage = "PrimaryTouch")]
    public bool buttonATouched;

    [Preserve, InputControl(bit = 0, offset = 40, layout = "Button", usage = "SecondaryButton")]
    public bool buttonB;

    [Preserve, InputControl(bit = 0, offset = 41, layout = "Button", usage = "SecondaryTouch")]
    public bool buttonBTouched;

    [Preserve, InputControl(bit = 0, offset = 44, layout = "Integer")]
    public int trackingState;

    [Preserve, InputControl(bit = 0, offset = 48, sizeInBits = 1, layout = "Button")]
    public bool isTracked;

    [Preserve, InputControl(bit = 0, offset = 52, layout = "Vector3",
       aliases = new[] { "devicePosition", "gripPosition"},
       noisy = true, dontReset = true)]
    public Vector3 devicePosition;

    [Preserve, InputControl(bit = 0, offset = 64, layout = "Quaternion",
       aliases = new[] { "deviceRotation", "gripOrientation"},
       noisy = true, dontReset = true)]
    public Quaternion deviceRotation;

    [Preserve, InputControl(bit = 0, offset = 80, layout = "Vector3", noisy = true, dontReset = true)]
    public Vector3 pointerPosition;

    [Preserve, InputControl(bit = 0, offset = 92, layout = "Quaternion", noisy = true, dontReset = true)]
    public Quaternion pointerRotation;
  }

#if UNITY_EDITOR
  [UnityEditor.InitializeOnLoad]
#endif
  [InputControlLayout(stateType = typeof(WebXRControllerState),
    displayName = "WebXR Controller",
    commonUsages = new[] { "LeftHand", "RightHand" },
    hideInUI = true)]
  public class WebXRController : XRControllerWithRumble
  {
    [InputControl]
    public AxisControl trigger { get; protected set; }

    [InputControl]
    public ButtonControl triggerPressed { get; protected set; }

    [InputControl]
    public ButtonControl triggerTouched { get; protected set; }

    [InputControl]
    public AxisControl squeeze { get; protected set; }

    [InputControl]
    public ButtonControl squeezePressed { get; protected set; }

    [InputControl]
    public ButtonControl squeezeTouched { get; protected set; }

    [InputControl]
    public Vector2Control thumbstick { get; protected set; }

    [InputControl]
    public ButtonControl thumbstickPressed { get; protected set; }

    [InputControl]
    public ButtonControl thumbstickTouched { get; protected set; }

    [InputControl]
    public Vector2Control touchpad { get; protected set; }

    [InputControl]
    public ButtonControl touchpadPressed { get; protected set; }

    [InputControl]
    public ButtonControl touchpadTouched { get; protected set; }

    [InputControl]
    public ButtonControl buttonA { get; protected set; }

    [InputControl]
    public ButtonControl buttonATouched { get; protected set; }

    [InputControl]
    public ButtonControl buttonB { get; protected set; }

    [InputControl]
    public ButtonControl buttonBTouched { get; protected set; }

    [InputControl]
    public Vector3Control pointerPosition { get; protected set; }

    [InputControl]
    public QuaternionControl pointerRotation { get; protected set; }

    static WebXRController()
    {
      InputSystem.RegisterLayout<WebXRController>(
        matches: new InputDeviceMatcher()
        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
        .WithProduct("WebXR Controller"));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInPlayer() { }

    protected override void FinishSetup()
    {
      base.FinishSetup();

      trigger = GetChildControl<AxisControl>("trigger");
      triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
      triggerTouched = GetChildControl<ButtonControl>("triggerTouched");
      squeeze = GetChildControl<AxisControl>("squeeze");
      squeezePressed = GetChildControl<ButtonControl>("squeezePressed");
      squeezeTouched = GetChildControl<ButtonControl>("squeezeTouched");
      thumbstick = GetChildControl<Vector2Control>("thumbstick");
      thumbstickPressed = GetChildControl<ButtonControl>("thumbstickPressed");
      thumbstickTouched = GetChildControl<ButtonControl>("thumbstickTouched");
      touchpad = GetChildControl<Vector2Control>("touchpad");
      touchpadPressed = GetChildControl<ButtonControl>("touchpadPressed");
      touchpadTouched = GetChildControl<ButtonControl>("touchpadTouched");
      buttonA = GetChildControl<ButtonControl>("buttonA");
      buttonATouched = GetChildControl<ButtonControl>("buttonATouched");
      buttonB = GetChildControl<ButtonControl>("buttonB");
      buttonBTouched = GetChildControl<ButtonControl>("buttonBTouched");
      pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
      pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
    }

    public void OnControllerUpdate(WebXRControllerData controllerData)
    {
      var state = new WebXRControllerState
      {
        trigger = controllerData.trigger,
        triggerPressed = controllerData.trigger >= 0.9f,
        triggerTouched = controllerData.triggerTouched,
        squeeze = controllerData.squeeze,
        squeezePressed = controllerData.squeeze >= 0.9f,
        squeezeTouched = controllerData.squeezeTouched,
        thumbstick = new Vector2(controllerData.thumbstickX, controllerData.thumbstickY),
        thumbstickPressed = controllerData.thumbstick >= 0.9f,
        thumbstickTouched = controllerData.thumbstickTouched,
        touchpad = new Vector2(controllerData.touchpadX, controllerData.touchpadY),
        touchpadPressed = controllerData.touchpad >= 0.9f,
        touchpadTouched = controllerData.touchpadTouched,
        buttonA = controllerData.buttonA >= 0.9f,
        buttonATouched = controllerData.buttonATouched,
        buttonB = controllerData.buttonB >= 0.9f,
        buttonBTouched = controllerData.buttonBTouched,
        trackingState = 3,
        isTracked = true,
        devicePosition = controllerData.gripPosition,
        deviceRotation = controllerData.gripRotation,
        pointerPosition = controllerData.position,
        pointerRotation = controllerData.rotation
      };
      InputSystem.QueueStateEvent(this, state);
    }
  }
}
#endif
