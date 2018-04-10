using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WVRControllerInteraction : MonoBehaviour
{
	[Tooltip("Map GameObject to controller hand name.")]
	public Hand hand = Hand.NONE;

	private WebVRControllerManager controllerManager;

	void Awake()
	{
		controllerManager = WebVRControllerManager.Instance;
	}

	void Update()
	{
		WVRController controller = controllerManager.GetController(gameObject, hand);

		if (controller != null)
		{
			Matrix4x4 sitStand = controller.sitStand;
			Quaternion sitStandRotation = Quaternion.LookRotation (
				sitStand.GetColumn (2),
				sitStand.GetColumn (1)
			);
			gameObject.transform.rotation = sitStandRotation * controller.rotation;
			gameObject.transform.position = sitStand.MultiplyPoint(controller.position);

			if (controller.GetButton(InputAction.Trigger)) {
				Debug.Log(hand + " trigger");
			}

			if (controller.GetButtonDown(InputAction.Trigger)) {
				Debug.Log(hand + " trigger down");
			}

			if (controller.GetButtonUp(InputAction.Trigger)) {
				Debug.Log(hand + " trigger up");
			}

			if (controller.GetButton(InputAction.Grip)) {
				Debug.Log(hand + " Grip");
			}

			if (controller.GetButtonDown(InputAction.Grip)) {
				Debug.Log(hand + " Grip down");
			}

			if (controller.GetButtonUp(InputAction.Grip)) {
				Debug.Log(hand + " Grip up");
			}
		}
	}
}
