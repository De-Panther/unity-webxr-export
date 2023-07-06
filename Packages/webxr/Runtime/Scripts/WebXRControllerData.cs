﻿using UnityEngine;

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
    public Vector3 gripPosition;
    public Quaternion gripRotation;
    public float trigger;
    public bool triggerTouched;
    public float squeeze;
    public bool squeezeTouched;
    public float thumbstick;
    public bool thumbstickTouched;
    public float thumbstickX;
    public float thumbstickY;
    public float touchpad;
    public bool touchpadTouched;
    public float touchpadX;
    public float touchpadY;
    public float buttonA;
    public bool buttonATouched;
    public float buttonB;
    public bool buttonBTouched;
    public string[] profiles;
  }

  [System.Serializable]
  public class WebXRControllersProfiles
  {
    public string[] controller1;
    public string[] controller2;
  }

  public enum WebXRHandJoint
  {
    wrist = 0,
    thumb_metacarpal = 1,
    thumb_phalanx_proximal = 2,
    thumb_phalanx_distal = 3,
    thumb_tip = 4,
    index_finger_metacarpal = 5,
    index_finger_phalanx_proximal = 6,
    index_finger_phalanx_intermediate = 7,
    index_finger_phalanx_distal = 8,
    index_finger_tip = 9,
    middle_finger_metacarpal = 10,
    middle_finger_phalanx_proximal = 11,
    middle_finger_phalanx_intermediate = 12,
    middle_finger_phalanx_distal = 13,
    middle_finger_tip = 14,
    ring_finger_metacarpal = 15,
    ring_finger_phalanx_proximal = 16,
    ring_finger_phalanx_intermediate = 17,
    ring_finger_phalanx_distal = 18,
    ring_finger_tip = 19,
    pinky_finger_metacarpal = 20,
    pinky_finger_phalanx_proximal = 21,
    pinky_finger_phalanx_intermediate = 22,
    pinky_finger_phalanx_distal = 23,
    pinky_finger_tip = 24
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
  }

  [System.Serializable]
  public struct WebXRJointData
  {
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
    public bool touched;
    public bool down;
    public bool up;
    public float value;

    public WebXRControllerButton(bool isPressed, bool isTouched, float buttonValue)
    {
      down = false;
      up = false;
      pressed = isPressed;
      touched = isTouched;
      value = buttonValue;
    }

    public void UpdateState(bool isPressed, bool isTouched, float buttonValue)
    {
      down = isPressed && !pressed;
      up = !isPressed && pressed;

      pressed = isPressed;
      touched = isTouched;
      value = buttonValue;
    }
  }
}
