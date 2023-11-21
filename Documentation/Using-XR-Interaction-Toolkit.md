# Using XR Interaction Toolkit

A quick guide on using XR Interaction Toolkit with WebXR Export.
For a guide on how to build and set a project, basic settings and WebGLTemplates, check the Getting Started guide.

## Sample Scene

As part of the WebXR Interactions package, there's the "XR Interaction Toolkit Sample", import it using the Package Manager window.
The sample will notify about missing packages and samples from those packages, using the Project Validation settings window.
Once all needed packages and samples are imported, the project will have "WebXRInteractionToolkit" scene, and prefabs for XR Rig and XR Setup.
The main difference between XR Interaction Toolkit basic XR Rig to the WebXR XR Rig, is the use of more than one Camera object.

## Project Settings

Make sure that in player settings, `Active Input Handling` is set to `Both` or `Input System Package (New)`.
In the `Input System Package` settings, `Background Behavior` should be set to `Ignore Focus`.

## Notes

The support for XR Interaction Toolkit was added using Unity 2022.3.10f1 and tested mainly on this version.
The support was built in a way that it'll be possible to use the OpenXR package in editor mode. For that the OpenXR package should be set up in the XR Plug-in Management window.
Notice that the `CameraMain` is using the old `Tracked Pose Driver` instead of the one of the Input System package, due to a bug. It will create issues when using "XR Interaction Toolkit - XR Device Simulator". On those cases, you can add the new component and disable the old one when in editor.
Notice that the XR Device Simulator won't work if OpenXR is enabled and the Interaction Profiles list in the OpenXR settings is not empty.
