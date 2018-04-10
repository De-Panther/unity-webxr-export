using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum WebVRControllerHand { NONE, LEFT, RIGHT };

public enum WebVRInputAction
{
	Trigger = 1,
	Grip = 2
}

public class WebVRControllerManager : MonoBehaviour
{
	public static WebVRControllerManager instance;

	public List<WebVRController> controllers = new List<WebVRController>();

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
		WebVRControllerButton[] buttons =  new WebVRControllerButton[0];
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
	public WebVRController GetController(GameObject gameObject, Enum h)
	{
		WebVRController controller = controllers.Where(x => x.gameObject == gameObject).FirstOrDefault();

		if (controller != null)
			return controller;

		WebVRController unbound;
		if ((WebVRControllerHand)h == WebVRControllerHand.NONE)
			unbound = controllers.Where(x => x.gameObject == null).FirstOrDefault();
		else
			unbound = controllers.Where(x => (WebVRControllerHand)x.hand == (WebVRControllerHand)h).FirstOrDefault();

		if (unbound != null) {
			unbound.gameObject = gameObject;
			Debug.Log("Binding " + gameObject.name + " to " + unbound.hand);
			return unbound;
		}
		return null;
	}

	private void onControllerUpdate(
		int index, string h, Vector3 position, Quaternion rotation, Matrix4x4 sitStand, WebVRControllerButton[] b)
	{
		// add or update controller values.
		WebVRController controller = controllers.Where(x => x.index == index).FirstOrDefault();

		if (controller == null)
		{
			// convert string to enum
			Enum hand;
			if (String.IsNullOrEmpty(h))
				hand = WebVRControllerHand.NONE;
			else
				hand = (WebVRControllerHand) Enum.Parse(typeof(WebVRControllerHand), h.ToUpper(), true);

			Debug.Log("Adding controller, Index: " + index + " Hand: " + hand);
			controller = new WebVRController(index, hand, position, rotation, sitStand);
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
