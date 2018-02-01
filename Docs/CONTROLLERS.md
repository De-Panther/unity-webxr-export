# Tracked controller setup

Setup GameObjects that you would like to be used for left/hand controllers by attaching 
them to the `WebVRCamera` component `Left Hand Obj` and `Right Hand Obj` fields.

![Controller setup](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/Docs/images/attach-controllers.gif)

# controller inputs

Controller inputs are handled through the [Unity WebGL input](https://docs.unity3d.com/Manual/webgl-input.html) system.

Listen for inputs using the `Input.GetKey`.


```
using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
    void Update() {
        if (Input.GetKey("joystick button 1"))
            print("Controller trigger is pressed!");
    }
}
```

# Controllers Mappings

## VIVE Controllers

Button | Interaction Type | WebVR Unity | Range  
--------|-----------------|---------------|------
Menu | Press | `"joystick button 3"` |
Trackpad| Press | `"joystick button 0"` |
Trackpad | Touch | `X axis` | -1.0 to 1.0
Trackpad | Touch | `Y axis` | -1.0 to 1.0
System | not mapped ||
Trigger | Squeeze | `"joystick button 1"` | 0.0 to 1.0
Grip | Squeeze | `"joystick button 2"` |

**NOTE**: It is not possible to distinguish between the different joysticks right now.