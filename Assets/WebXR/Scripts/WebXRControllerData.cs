using UnityEngine;

namespace WebXR
{
  [System.Serializable]
  class WebXRControllerData
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
    public WebXRControllerButton[] buttons = new WebXRControllerButton[0];
  }

  [System.Serializable]
  public struct WebXRControllerData2
  {
    public bool enabled;
    public int hand;
    public Vector3 position;
    public Quaternion rotation;
    public float trigger;
    public float squeeze;
    public float thumbstick;
    public float thumbstickX;
    public float thumbstickY;
    public float touchpad;
    public float touchpadX;
    public float touchpadY;
    public float buttonA;
    public float buttonB;
  }

  public enum WebXRControllerHand { NONE, LEFT, RIGHT };

  [System.Serializable]
  public class WebXRControllerButton
  {
    public bool pressed;
    public bool prevPressedState;
    public bool touched;
    public float value;

    public WebXRControllerButton(bool isPressed, float buttonValue)
    {
      pressed = isPressed;
      prevPressedState = false;
      value = buttonValue;
    }
  }
}
