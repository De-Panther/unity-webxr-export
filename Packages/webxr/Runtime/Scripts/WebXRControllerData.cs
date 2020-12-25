using UnityEngine;

namespace WebXR
{
  [System.Serializable]
  public class WebXRControllerData
  {
    public int frame;
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
    public string[] profiles;
  }

  [System.Serializable]
  public class WebXRControllersProfiles
  {
    public string[] conrtoller1;
    public string[] conrtoller2;
  }

  [System.Serializable]
  public class WebXRHandData
  {
    public int frame;
    public bool enabled;
    public int hand;
    public float trigger;
    public float squeeze;
    public WebXRJointData[] joints = new WebXRJointData[25];
    public const int WRIST = 0;
    public const int THUMB_METACARPAL = 1;
    public const int THUMB_PHALANX_PROXIMAL = 2;
    public const int THUMB_PHALANX_DISTAL = 3;
    public const int THUMB_PHALANX_TIP = 4;
    public const int INDEX_METACARPAL = 5;
    public const int INDEX_PHALANX_PROXIMAL = 6;
    public const int INDEX_PHALANX_INTERMEDIATE = 7;
    public const int INDEX_PHALANX_DISTAL = 8;
    public const int INDEX_PHALANX_TIP = 9;
    public const int MIDDLE_METACARPAL = 10;
    public const int MIDDLE_PHALANX_PROXIMAL = 11;
    public const int MIDDLE_PHALANX_INTERMEDIATE = 12;
    public const int MIDDLE_PHALANX_DISTAL = 13;
    public const int MIDDLE_PHALANX_TIP = 14;
    public const int RING_METACARPAL = 15;
    public const int RING_PHALANX_PROXIMAL = 16;
    public const int RING_PHALANX_INTERMEDIATE = 17;
    public const int RING_PHALANX_DISTAL = 18;
    public const int RING_PHALANX_TIP = 19;
    public const int LITTLE_METACARPAL = 20;
    public const int LITTLE_PHALANX_PROXIMAL = 21;
    public const int LITTLE_PHALANX_INTERMEDIATE = 22;
    public const int LITTLE_PHALANX_DISTAL = 23;
    public const int LITTLE_PHALANX_TIP = 24;
  }

  [System.Serializable]
  public struct WebXRJointData
  {
    public bool enabled;
    public Vector3 position;
    public Quaternion rotation;
    public float radius;
  }

  [System.Serializable]
  public class WebXRHitPoseData
  {
    public int frame;
    public bool available;
    public Vector3 position;
    public Quaternion rotation;
  }

  public enum WebXRControllerHand
  {
    NONE = 0,
    LEFT = 1,
    RIGHT = 2
  }

  [System.Serializable]
  public class WebXRControllerButton
  {
    public bool pressed;
    public bool down;
    public bool up;
    public float value;

    public WebXRControllerButton(bool isPressed, float buttonValue)
    {
      down = false;
      up = false;
      pressed = isPressed;
      value = buttonValue;
    }

    public void UpdateState(bool isPressed, float buttonValue)
    {
      if (isPressed && pressed) // nothing
      {
        down = false;
        up = false;
      }
      else if (isPressed && !pressed) // up
      {
        down = true;
        up = false;
      }
      else if (!isPressed && !pressed) // nothing
      {
        down = false;
        up = false;
      }
      else if (!isPressed && pressed) // down
      {
        down = false;
        up = true;
      }
      pressed = isPressed;
      value = buttonValue;
    }
  }
}
