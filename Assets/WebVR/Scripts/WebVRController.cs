using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WebVRControllerButton
{
    public bool pressed;
    public bool touched;
    public float value;
}

public class WebVRController
{
    public int index;
    public Enum hand;
    public Vector3 position;
    public Quaternion rotation;
    public Matrix4x4 sitStand;
    public WebVRControllerButton[] buttons = null;
    public GameObject gameObject;

    private Dictionary<WebVRInputAction, bool[]> buttonStates = new Dictionary<WebVRInputAction, bool[]>();
    
    public void UpdateButtons(WebVRControllerButton[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            WebVRControllerButton button = buttons[i];
            foreach(WebVRInputAction action in Enum.GetValues(typeof(WebVRInputAction)))
            {
                if (i == (int)action) {
                    if (buttonStates.ContainsKey(action))
                        buttonStates[action][0] = button.pressed;
                    else
                        buttonStates.Add (action, new bool[]{ button.pressed, false });
                }
            }
        }
    }

    public bool GetButton(WebVRInputAction action)
    {
        if (!buttonStates.ContainsKey(action))
            return false;
        return buttonStates[action][0];
    }

    public bool GetButtonDown(WebVRInputAction action)
    {
        if (!buttonStates.ContainsKey(action))
            return false;

        bool isDown = false;
        bool buttonPressed = buttonStates[action][0];
        bool prevButtonState = buttonStates[action][1];

        if (buttonPressed && prevButtonState != buttonPressed)
        {
            buttonStates[action][1] = true;
            isDown = true;
        }
        return isDown;
    }

    public bool GetButtonUp(WebVRInputAction action)
    {
        if (!buttonStates.ContainsKey(action))
            return false;
        
        bool isUp = false;
        bool buttonPressed = buttonStates[action][0];
        bool prevButtonState = buttonStates[action][1];

        if (!buttonPressed && prevButtonState) {
            buttonStates[action][1] = false;
            isUp = true;
        }
        return isUp;
    }

    public WebVRController(int index, Enum hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand)
    {
        this.index = index;
        this.hand = hand;
        this.position = position;
        this.rotation = rotation;
        this.sitStand = sitStand;
        this.gameObject = null;
    }
}
