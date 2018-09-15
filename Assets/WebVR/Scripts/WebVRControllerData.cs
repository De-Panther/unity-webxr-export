using UnityEngine;

[System.Serializable]
class WebVRControllerData
{
	public string id = null;
	public int index = 0;
	public string hand = null;
	public bool hasOrientation = false;
	public bool hasPosition = false;
	public float[] orientation = null;
	public float[] position = null;
	public float[] linearAcceleration = null;
	public float[] linearVelocity = null;
	public float[] axes = null;
	public WebVRControllerButton[] buttons = new WebVRControllerButton[0];
}