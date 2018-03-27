using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Controller
{
	public int index;
	public string hand;
	public Vector3 position;
	public Quaternion rotation;
	public Matrix4x4 sitStand;
	public GameObject gameObject;
	
	public Controller(int index, string hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand)
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

	// registers GameObject to controller returning controller.
	public Controller registerController(GameObject gameObject)
	{
		Controller controller = controllers.Where(x => x.gameObject == gameObject).SingleOrDefault();
		if (controller == null)
		{
			Controller unbound = controllers.Where(x => x.gameObject == null).SingleOrDefault();
			if (unbound != null)
			{
				Debug.Log("Bounding to controller! " + unbound.index);
				unbound.gameObject = gameObject;
				return unbound;
			}
			else
				return null;
		}
		else
			return controller;
	}

	void Start()
	{
		WebVRManager.OnControllerUpdate += handleControllerUpdate;
	}

	void Awake()
	{
		instance = this;
	}

	private void handleControllerUpdate(
		int index, string hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand)
	{
		// add or update controller values.
		Controller controller = controllers.Where(x => x.index == index).SingleOrDefault();
		
		if (controller == null)
			controllers.Add(new Controller(index, hand, position, rotation, sitStand));
		else
		{
			controller.position = position;
			controller.rotation = rotation;
			controller.sitStand = sitStand;
		}	
	}
}