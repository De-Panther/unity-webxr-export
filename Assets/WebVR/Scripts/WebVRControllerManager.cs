using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Hand { NONE, LEFT, RIGHT };

public class Controller
{
	public int index;
	public Enum hand;
	public Vector3 position;
	public Quaternion rotation;
	public Matrix4x4 sitStand;
	public GameObject gameObject;
	
	public Controller(int index, Enum hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand)
	{
		this.index = index;
		this.hand = hand;
		this.position = position;
		this.rotation = rotation;
		this.sitStand = sitStand;
		this.gameObject = null;
	}
}

public class WebVRControllerManager : MonoBehaviour
{
	public static WebVRControllerManager instance;

	public List<Controller> controllers = new List<Controller>();

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
		onControllerUpdate(
			0,
			"left",
			UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand),
			UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand),
			Matrix4x4.identity);

		onControllerUpdate(
			1,
			"right",
			UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand),
			UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand),
			Matrix4x4.identity);
		#endif
	}

	// registers GameObject to controller returning controller.
	public Controller GetController(GameObject gameObject, Enum h)
	{
		Controller controller = controllers.Where(x => x.gameObject == gameObject).FirstOrDefault();

		if (controller != null)
			return controller;

		Controller unbound;
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
		int index, string hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand)
	{
		// add or update controller values.
		Controller controller = controllers.Where(x => x.index == index).FirstOrDefault();
		
		if (controller == null)
		{
			// convert string to enum
			Enum h = String.IsNullOrEmpty(hand) ? Hand.NONE : (Hand) Enum.Parse(typeof(Hand), hand.ToUpper(), true);
			Debug.Log("Adding controller, Index:" + index + " Hand: " + h);
			controllers.Add(new Controller(index, h, position, rotation, sitStand));
		}
		else
		{
			controller.position = position;
			controller.rotation = rotation;
			controller.sitStand = sitStand;
		}	
	}
}