using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRController : MonoBehaviour
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
		Controller controller = controllerManager.GetController(gameObject, hand);

		if (controller != null)
		{
			Matrix4x4 sitStand = controller.sitStand;
			Quaternion sitStandRotation = Quaternion.LookRotation (
				sitStand.GetColumn (2),
				sitStand.GetColumn (1)
			);
			gameObject.transform.rotation = sitStandRotation * controller.rotation;
			gameObject.transform.position = sitStand.MultiplyPoint(controller.position);
		}
	}
}
