using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WVRController
{
    public int index;
    public Enum hand;
    public Vector3 position;
    public Quaternion rotation;
    public Matrix4x4 sitStand;
    public WVRControllerButton[] buttons = null;
    public GameObject gameObject;

    private Dictionary<InputAction, bool[]> buttonStates = new Dictionary<InputAction, bool[]>();
    
    public void UpdateButtons(WVRControllerButton[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            WVRControllerButton button = buttons[i];
            foreach(InputAction action in Enum.GetValues(typeof(InputAction)))
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

    public bool GetButton(InputAction action)
    {
        if (!buttonStates.ContainsKey(action))
            return false;
        return buttonStates[action][0];
    }

    public bool GetButtonDown(InputAction action)
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

    public bool GetButtonUp(InputAction action)
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

    public WVRController(int index, Enum hand, Vector3 position, Quaternion rotation, Matrix4x4 sitStand)
    {
        this.index = index;
        this.hand = hand;
        this.position = position;
        this.rotation = rotation;
        this.sitStand = sitStand;
        this.gameObject = null;
    }
}
