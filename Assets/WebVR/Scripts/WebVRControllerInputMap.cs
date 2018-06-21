using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WebVRControllerInputMap")]
public class WebVRControllerInputMap : ScriptableObject {
	[Header("WebVR Controller Input Map")]
	public List<WebVRControllerInput> inputs;
}

[System.Serializable]
public class WebVRControllerInput {
	[Tooltip("A meaningful name describing the gesture performed on the controller.")]
	public string actionName;
	[Tooltip("Button ID from Web Gamepad API.")]
	public int gamepadButtonId;
	[Tooltip("Button name defined in Unity Input Manager.")]
	public string unityInputName;
}