# Controllers and input system

Tracked controllers are by default included in `WebVRCameraSet` prefab when installed into your project scene. See [Project Setup](./project-setup.md#4-add-the-webvr-prefab-to-your-scene) on how to do this.

Out of the box, the prefab includes:

* Hand models
* Animations for hands when using grip and trigger buttons.
* Controller position and orientation tracking.
* Works with _PC, Mac and Linux Standalone_ and _WebVR_ platforms.
* Works with in-editor playback.

![In-editor play](images/editor-play.gif)

# WebVRController Script

Provides configuration for setting up GameObjects as controllers. The script also applies position and orientation tracking from VR controllers.

![Controller component](images/controller-script.png)

| Option | Description |
| --- | --- |
| Hand | GameObject tracked as Left or Right hand |
| Input Map | WebVRControllerInputMap asset used to configure Inputs from controllers |

# Cross platform support using WebVRControllerInputMap

Because we are working between two platforms, Unity (Editor and Standalone PC, Mac, Linux) and Web Browsers, we need a way to mediate between the two input systems, the [Unity Input Manager](https://docs.unity3d.com/Manual/xr_input.html) and [Browser Gamepad API](https://developer.mozilla.org/en-US/docs/Web/API/Gamepad_API/Using_the_Gamepad_API) respectively. To do this, we create `WebVRControllerInputMap` assets to configure _Actions_ that map to the respective inputs for each platform.

In the asset package, we include two pre-made assets, one for each hand: `LeftControllerMap.asset` and `RightControllerMap.asset`.  To use, _drag and drop_ the assets into `WebVRController` script `Input Map` option for the corresponding left or right hand.

![Assign Input Map](images/assign-inputmap.gif)

# Configuring a WebVRControllerInputMap asset

![Input Map configuration](images/inputmap.png)

| Option | Description |
| --- | --- |
| Action Name | Name describing gesture performed on controller |
| Gamepad Id | The Corresponding Gamepad API button or axis ID |
| Gamepad Is Button | Whether gesture derives its value from a Gamepad API `GamepadButton.pressed` property |
| Unity Input Name | Input name defined in Unity Input Manager |
| Unity Input Is Button | Whether gesture derives its value from Unity using `Input.GetButton` function |

As a note, we have two `WebVRControllerInputMap` assets, one for each hand since there is overlap between input ID's for browser Gamepad button and axis for each hand.

## Creating a new InputMap Asset

To create a new `WebVRControllerInputMap`, use `Asset > Create > WebVRControllerInputMap`

# Configure Unity Input Manager

To get up and running using pre-configured Input Manager settings, copy `Project Settings/InputManager.asset` from this repo into your own project.

You can also choose to manually configure the Input Manager by using 
`Edit > Project Settings > Input`

Below is an example of a Unity Input Manager entries that map to `WebVRControllerInputMap` configuration as shown above.

![Unity Input Manager configuration](images/unity-input-manager.png)

## Unity VR Input

See Unity VR Input specification for controller axis and button definitions for each VR system.   

* [Input for Oculus](https://docs.unity3d.com/Manual/OculusControllers.html)
* [Input for Open VR](https://docs.unity3d.com/Manual/OpenVRControllers.html)
* [Input for Windows Mixed Reality](https://docs.unity3d.com/Manual/Windows-Mixed-Reality-Input.html)

## Gamepad API Input

Identify controller axis and button usage using the [HTML5 Gamepad Tester](http://html5gamepad.com/).

You will need to have WebVR active and rendering into the headset before controllers are visible. To do this, open [WebVR content](https://webvr.info/samples/XX-vr-controllers.html) and _Enter VR_, open another tab and navigate to the [HTML5 Gamepad Tester](http://html5gamepad.com/).

![html5gamepad.com](images/html5gamepad.png)

## How the WebVRController decides which Input platform to use.

Unity Input Manager (for button and axis) and XR Inputs (for position and orientation) is used when using in-editor playback or when building to PC, Mac & Linux compatible standalone.
* [In-editor VR playback for rapid testing](./xr-testing.md).

When your project is built to WebVR, the browser Gamepad API is used for input from VR controllers.
* [Building your project to WebVR](./project-setup#6-build-your-project-to-webvr).

## Using controller Inputs

The `WebVRController` Inputs are similar in use to the standard Unity Input system.

```c#
using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
    void Update() {
        // Retrieve the WebVRController commponent.
        WebVRController controller = gameObject.GetComponent<WebVRController>();

        // Controller hand being used.
        WebVRControllerHand hand = controller.hand;
        
        // GetButtonDown and GetButtonUp:
        if (controller.GetButtonDown("Trigger"))
            print(hand + " controller Trigger is down!");

        if (controller.GetButtonUp("Trigger"))
            print(hand + " controller Trigger is up!");
        
        // GetAxis:
        if (controller.GetAxis("Grip") > 0)
            print(hand + " controller Grip value: " + controller.GetAxis("Grip"));
    }
}
```
