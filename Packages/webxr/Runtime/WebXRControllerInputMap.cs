using System.Collections.Generic;
using UnityEngine;

namespace WebXR
{
  [CreateAssetMenu(menuName = "WebXRControllerInputMap")]
  public class WebXRControllerInputMap : ScriptableObject
  {
    [Header("WebXR Controller Input Map")]
    public List<WebXRControllerInput> inputs;
  }

  [System.Serializable]
  public class WebXRControllerInput
  {
    [Tooltip("A meaningful name describing the gesture performed on the controller.")]
    public string actionName;

    [Header("Web Gamepad API configuration")]
    [Tooltip("Button or axes ID from Web Gamepad API.")]
    public int gamepadId;
    [Tooltip("Whether gesture derives its value from a Gamepad API `GamepadButton.pressed` property.")]
    public bool gamepadIsButton;

    [Header("Unity Input Manager configuration")]
    [Tooltip("Input name defined in Unity Input Manager.")]
    public string unityInputName;
    [Tooltip("Whether gesture derives its value from Unity using `Input.GetButton` function.")]
    public bool unityInputIsButton;
  }
}
