# Controllers and input system

## Setting up tracked controllers

Set up `GameObject`s for left/hand controllers by attaching them to the `WebVRCamera` component's `Left Hand Obj` and `Right Hand Obj` fields.

![Controller setup](https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/Docs/images/attach-controllers.gif)

## Input API

Controller inputs are handled through the [Unity WebGL input](https://docs.unity3d.com/Manual/webgl-input.html) system.

Listen for inputs using the `Input.GetKey`.


```c#
using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
    void Update() {
        if (Input.GetKey("joystick button 1"))
            print("Controller trigger is pressed!");
    }
}
```

## Controller mappings

When targeting the WebGL and therefore, the browser, the way the different inputs are labelled changes in comparison with the names they are assigned while working in Unity. We are trying to provide a more consistent way to deal with this issue; in the meantime, here are the mappings for the different controller models.

Also notice that now, where the controllers are positioned in 3D space can be detected, but which controller initiated an interaction cannot be detected.

If you're looking for a depiction of the VIVE and Oculus Rift controllers, take a look at the excellent [schemas provided by Unity documentation](https://docs.unity3d.com/Manual/OpenVRControllers.html).

### Gear VR Controller

Button | Interaction Type | WebVR Unity | Range  
-------|------------------|-------------|------
Trackpad | Press | `"joystick button 0"` |
Trackpad | Touch | `X axis` | -1.0 to 1.0
Trackpad | Touch | `Y axis` | -1.0 to 1.0

### Oculus Touch

Button | Interaction Type | WebVR Unity | Range  
-------|------------------|-------------|------
X/A | Press | `"joystick button 3"` |
Y/B | Press | `"joystick button 4"` |
Thumbrest | Touch | `"joystick button 5"` |
Thumbstick | Press | `"joystick button 0"` |
Thumbstick | Touch | `X axis` | -1.0 to 1.0
Thumbstick | Touch | `Y axis` | -1.0 to 1.0
Start | not mapped ||
IndexTrigger | Squeeze | `"joystick button 1"` | 0.0 to 1.0
HandTrigger | Squeeze | `"joystick button 2"` | 0.0 to 1.0 

### VIVE Controllers

Button | Interaction Type | WebVR Unity | Range  
-------|------------------|-------------|------
Menu | Press | `"joystick button 3"` |
Trackpad | Press | `"joystick button 0"` |
Trackpad | Touch | `X axis` | -1.0 to 1.0
Trackpad | Touch | `Y axis` | -1.0 to 1.0
System | not mapped ||
Trigger | Squeeze | `"joystick button 1"` | 0.0 to 1.0
Grip | Squeeze | `"joystick button 2"` |

### Windows MR

Button | Interaction Type | WebVR Unity | Range  
-------|------------------|-------------|------
Menu | Press | `"joystick button 3"` |
Thumbstick | Press | `"joystick button 0"` |
Thumbstick | Touch | `X axis` | -1.0 to 1.0
Thumbstick | Touch | `Y axis` | -1.0 to 1.0
Trackpad | Press | `"joystick button 4"` |
Trackpad | Touch | `3rd axis` | -1.0 to 1.0
Trackpad | Touch | `4th axis` | -1.0 to 1.0
Windows | not mapped ||
Trigger | Squeeze | `"joystick button 1"` | 0.0 to 1.0
Grip | Squeeze | `"joystick button 2"` |
