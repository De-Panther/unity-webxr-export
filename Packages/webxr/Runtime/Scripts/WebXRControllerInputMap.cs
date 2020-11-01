using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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
  }
}
