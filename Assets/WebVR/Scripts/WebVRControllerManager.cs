using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Hand { NONE, LEFT, RIGHT };

public enum InputAction
{
	Trigger = 1,
	Grip = 2
}

public class WebVRControllerManager : MonoBehaviour
{
	public static WebVRControllerManager instance;

	public List<WVRController> controllers = new List<WVRController>();

	public static WebVRControllerManager Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject go = new GameObject("WebVRControllerManager");
				go.AddComponent<WebVRControllerManager>();
			}
			return instance;
		}
	}

	void Start()
	{
		WebVRManager.OnControllerUpdate += onControllerUpdate;
	}

	void Awake()
	{
		instance = this;
	}

	void Update()
	{
		#if UNITY_EDITOR
		// update controllers using Unity XR support when in editor.
		WVRControllerButton[] buttons =  new WVRControllerButton[0];
		onControllerUpdate(
			0,
			"left",
			UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand),
			UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand),
			Matrix4x4.identity, buttons);

		onControllerUpdate(
			1,
			"right",
			UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand),
			UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand),
			Matrix4x4.identity, buttons);
		#endif
	}

	// registers GameObject to controller returning controller.
	public WVRController GetController(GameObject gameObject, Enum h)
	{
		WVRController controller = controllers.Where(x => x.gameObject == gameObject).FirstOrDefault();

		if (controller != null)
			return controller;

		WVRController unbound;
		if ((Hand)h == Hand.NONE)
			unbound = controllers.Where(x => x.gameObject == null).FirstOrDefault();
		else
			unbound = controllers.Where(x => (Hand)x.hand == (Hand)h).FirstOrDefault();

		if (unbound != null) {
			unbound.gameObject = gameObject;
			Debug.Log("Binding " + gameObject.name + " to " + unbound.hand);
			return unbound;
		}
		return null;
	}

	private void onControllerUpdate(
		int index, string hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand, WVRControllerButton[] b)
	{
		// add or update controller values.
		WVRController controller = controllers.Where(x => x.index == index).FirstOrDefault();

		if (controller == null)
		{
			// convert string to enum
			Enum h = String.IsNullOrEmpty(hand) ? Hand.NONE : (Hand) Enum.Parse(typeof(Hand), hand.ToUpper(), true);
			Debug.Log("Adding controller, Index:" + index + " Hand: " + h);
			controller = new WVRController(index, h, position, rotation, sitStand);
			controllers.Add(controller);
		}
		else
		{
			controller.position = position;
			controller.rotation = rotation;
			controller.sitStand = sitStand;
		}

		if (b != null)
			controller.UpdateButtons(b);
	}
}
