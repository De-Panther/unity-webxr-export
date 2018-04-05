using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class FreeFlightController : MonoBehaviour {
	[Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
	public bool rotationEnabled = true;

	[Tooltip("Enable/disable translation control. For use in Unity editor only.")]
	public bool translationEnabled = true;

	[Tooltip("Speed of rotation in degrees/seconds.")]
	public float rotationSpeed = 90;

	[Tooltip("Speed of translation in meters/seconds.")]
	public float translationSpeed = 7;

	[Tooltip("Minimum distance the mouse must move to start registering movement.")]
	public float rotationDeadDistance = 0.001f;

	[Tooltip("Keys to move forward.")]
	public List<KeyCode> moveForwardKeys =
		new List<KeyCode> { KeyCode.W, KeyCode.UpArrow };

	[Tooltip("Keys to move backward.")]
	public List<KeyCode> moveBackwardKeys =
		new List<KeyCode> { KeyCode.S, KeyCode.DownArrow };

	[Tooltip("Keys to stride to the right.")]
	public List<KeyCode> strideRightKeys =
		new List<KeyCode> { KeyCode.D, KeyCode.RightArrow };

	[Tooltip("Keys to stride to the left.")]
	public List<KeyCode> strideLeftKeys =
		new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow };

	[Tooltip("Keys to move upward.")]
	public List<KeyCode> moveUpwardKeys = new List<KeyCode> { KeyCode.R };

	[Tooltip("Keys to move downward.")]
	public List<KeyCode> moveDownwardKeys = new List<KeyCode> { KeyCode.F };

	private WebVRManager webVRManager;
	private VRDisplayCapabilities capabilities;

	bool inDesktopLike {
		get {
			return capabilities.hasExternalDisplay;
		}
	}

	Vector3 mouseMovement = Vector3.zero;

	Vector3 lastMousePosition;

	void Awake()
	{
		webVRManager = WebVRManager.Instance;
	}

	void Start()
	{
		WebVRManager.OnVrChange += handleVrChange;
		WebVRManager.OnCapabilitiesUpdate += handleCapabilitiesUpdate;
	}

	private void handleVrChange()
	{
		if (webVRManager.vrState == VrState.ENABLED)
		{
			DisableEverything();
		}
		else
		{
			EnableAccordingToPlatform();
		}
	}

	private void handleCapabilitiesUpdate(VRDisplayCapabilities vrCapabilities)
	{
		capabilities = vrCapabilities;
		EnableAccordingToPlatform();
	}

	void Update() {
		if (translationEnabled) {
			Translate();
		}
		if (rotationEnabled) {
			Rotate();
		}
	}

	void DisableEverything() {
		translationEnabled = false;
		rotationEnabled = false;
	}

	/// Enables rotation and translation control for desktop environments.
	/// For mobile environments, it enables rotation or translation according to
	/// the device capabilities.
	void EnableAccordingToPlatform() {
		rotationEnabled = inDesktopLike || !capabilities.hasOrientation;
		translationEnabled = inDesktopLike || !capabilities.hasPosition;
	}

	void Translate() {
		transform.Translate(
			SideMovement(),
			ElevationMovement(),
			ForwardMovement()
		);
	}

	void Rotate() {
		RegisterMouseMovement();
		transform.Rotate(Vector3.up, YawMovement(), Space.World);
		transform.Rotate(Vector3.right, -PitchMovement(), Space.Self);
	}

	float SideMovement() {
		return TranslationPerFrame(DirectionFromKeys(
			strideRightKeys,
			strideLeftKeys
		));
	}

	float ElevationMovement() {
		return TranslationPerFrame(DirectionFromKeys(
			moveUpwardKeys,
			moveDownwardKeys
		));
	}

	float ForwardMovement() {
		return TranslationPerFrame(DirectionFromKeys(
			moveForwardKeys,
			moveBackwardKeys
		));
	}

	float TranslationPerFrame(float direction) {
		return direction * translationSpeed * Time.deltaTime;
	}

	float DirectionFromKeys(List<KeyCode> positive, List<KeyCode> negative) {
		if (AnyKeyIsPressed(positive)) {
			return 1;
		}
		if (AnyKeyIsPressed(negative)) {
			return -1;
		}
		return 0;
	}

	bool AnyKeyIsPressed(List<KeyCode> keys) {
		return keys.FindAll(k => Input.GetKey(k)).Count > 0;
	}

	void RegisterMouseMovement() {
		bool mouseStoppedDragging = Input.GetMouseButtonUp(0);
		bool mouseIsDragging = Input.GetMouseButton(0);

		if (mouseStoppedDragging) {
			mouseMovement.Set(0, 0, 0);
		}
		else if (mouseIsDragging) {
			mouseMovement = Input.mousePosition - lastMousePosition;
		}

		lastMousePosition = Input.mousePosition;
	}

	float YawMovement() {
		return RotationPerFrame(
			DirectionFromMovement(mouseMovement.x, rotationDeadDistance));
	}

	float PitchMovement() {
		return RotationPerFrame(
			DirectionFromMovement(mouseMovement.y, rotationDeadDistance));
	}

	float RotationPerFrame(float direction) {
		return direction * rotationSpeed * Time.deltaTime;
	}

	float DirectionFromMovement(float number, float threshold=0.001f) {
		if (number > threshold) {
			return 1;
		}
		if (number < -threshold) {
			return -1;
		}
		return 0;
	}
}
