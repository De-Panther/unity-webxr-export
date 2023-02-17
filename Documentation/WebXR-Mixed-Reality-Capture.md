# WebXR Mixed Reality Capture

Using Mixed Reality Capture in WebXR requires different parts to work together. In this page we will go over the different parts and settings to make it work in a project.

## General info

Mixed Reality Capture is used to merge between the real world and the virtual world, by positioning a real camera feed inside the virtual environment and displaying the outcome in a spectator display.
In this WebXR solution, we made sure that WebXR Export can support Spectator mode and had the code of the Mixed Reality Capture feature in the WebXR Interactions package.
A configured prefab can be found in the Sample scene of WebXR Interactions named as `SpectatorCameraHolder.prefab`.

## Expected hierarchy

The current configuration is expected for Mixed Reality Capture to work properly

```
- SpectatorCameraHolder
  - SpectatorCamera (Components: Camera, SpectatorCamera)
    - StackCameras
      - SpectatorBackgroundCamera (Components: Camera)
      - SpectatorWebcamLightingCamera (Components: Camera)
      - SpectatorForegroundCamera (Components: Camera)
      - Background (Components: MeshFilter with Quad, MeshRenderer with UnlitTransparent Material. Layer: Spectator) 
      - Foreground (Components: MeshFilter with Quad, MeshRenderer with UnlitTransparent Material. Layer: Spectator)
  - MixedRealityCaptureController (Components: MixedRealityCaptureController)
    - CameraPoint (Used as Visual reference for moving point)
    - TopPoint (Used as Visual reference for moving point)
    - BottomPoint (Used as Visual reference for moving point)
  - WebcamHolder
    - WebcamQuad (Components: PlayWebcam, MeshFilter with Quad, MeshRenderer with ChromaKeyUnlit Material. Layer: Webcam)
      - WebcamLightingQuad (Components: MeshFilter with Quad, MeshRenderer with White Legacy Diffuse Material. Layer: WebcamLighting)
    - CameraHint (Used as Visual reference for point on webcam)
    - TopHint (Used as Visual reference for point on webcam)
    - BottomHint (Used as Visual reference for point on webcam)
```

## Layers

The `SpectatorCameraHolder.prefab` sample prefab uses pre-defined layers:

- 16 Webcam
- 17 WebcamLighting
- 18 Spectator

## Configuring Mixed Reality Capture in a project

Some general steps to follow when implementing WebXR Mixed Reality Capture in a project:

- Import the `SpectatorCameraHolder.prefab` from the sample. It's better to duplicate it and using the duplicated asset.
- Add the prefab to the scene with the WebXR Camera Rig (or `WebXRCameraSet`).
- If the camera rig is expected to be static, the `SpectatorCameraHolder` can be at the root of the scene, else it should be a child of the camera rig.
- Make sure `MixedRealityCaptureController` is configured correctly.
- `webcamLayer` point to the Webcam layer.
- `mixedRealityOnLayers` are the layers that the Spectator camera would use during the capture mode, should contain the Webcam and Spectator layers.
- `camerasBase` reference to the Cameras Transform in the camera rig hierarchy.
- `cameraFollower` reference the `CameraFollower` Transform in the camera rig hierarchy.
- `xrCameras` reference all the XR cameras Camera components in the camera rig hierarchy.
- `spectatorCamera` reference the Spectator camera Camera in the `SpectatorCameraHolder` hierarchy.
- `stackCameras` reference the `StackCameras` GameObject in the `SpectatorCameraHolder` hierarchy.
- `spectatorBackgroundCamera` reference the `SpectatorBackgroundCamera` Camera in the `SpectatorCameraHolder` hierarchy.
- `spectatorForegroundCamera` reference the `SpectatorForegroundCamera` Camera in the `SpectatorCameraHolder` hierarchy.
- `spectatorWebcamLightingCamera` reference the `SpectatorWebcamLightingCamera` Camera in the `SpectatorCameraHolder` hierarchy.
- `spectatorCameraTransform` reference the Spectator camera Transform in the `SpectatorCameraHolder` hierarchy.
- `spectatorCameraParent` reference the `SpectatorCameraHolder` Transform.
- `backgroundPlaneRenderer` reference the `Background` Renderer in the `SpectatorCameraHolder` hierarchy.
- `foregroundPlaneRenderer` reference the `Foreground` Renderer in the `SpectatorCameraHolder` hierarchy.
- `backgroundPlaneTransform` reference the `Background` Transform in the `SpectatorCameraHolder` hierarchy.
- `foregroundPlaneTransform` reference the `Foreground` Transform in the `SpectatorCameraHolder` hierarchy.
- `defaultPlaneMaterial` default Material used for the background and foreground planes, Unlit Transparent material.
- `webcamParent` reference the `WebcamHolder` Transform in the `SpectatorCameraHolder` hierarchy.
- `webcam` reference the `WebcamQuad` PlayWebcam in the `SpectatorCameraHolder` hierarchy.
- `calibrationPointCamera` reference the `CameraPoint` Transform in the `SpectatorCameraHolder` hierarchy.
- `calibrationPointTop` reference the `TopPoint` Transform in the `SpectatorCameraHolder` hierarchy.
- `calibrationPointBottom` reference the `BottomPoint` Transform in the `SpectatorCameraHolder` hierarchy.
- `calibrationHintCamera` reference the `CameraHint` GameObject in the `SpectatorCameraHolder` hierarchy.
- `calibrationHintTop` reference the `TopHint` GameObject in the `SpectatorCameraHolder` hierarchy.
- `calibrationHintBottom` reference the `BottomHint` GameObject in the `SpectatorCameraHolder` hierarchy.
- `leftController` reference the left WebXRController in the camera rig hierarchy.
- `rightController` reference the right WebXRController in the camera rig hierarchy.
- `webcamFramesDelaySize` set the default number of delayed frames to store - Used for cases of Webcam image slower than VR movement.
- Make sure that the Main Camera and XR Cameras hide or show the correct layers, the Spectator, Webcam and WebcamLighting should be hidden.
- Make sure that the Spectator camera shows the Spectator and Webcam Layers and other layers that the project should display.
- Make sure that the Spectator camera hides the WebcamLighting layer.
- Make sure that the background and foreground cameras hide the Spectator, Webcam and WebcamLighting layers.
- Make sure that the background and foreground cameras show all other layers that the project should display.
- Make sure that the SpectatorWebcamLightingCamera only shows the WebcamLighting layer.

That should handle all the configurations needed for Mixed Reality Captuer to work properly.

## Enable Mixed Reality Captuer on runtime

Once the project runs, for the feature to be enabled, both the `SpectatorCamera` and `MixedRealityCaptureController` should be set to be enable in XR.
Both components contains `EnableInXR(bool)` method to enable their functionality when user is switching to XR mode.

There are more customization and settings that can be handle by scripts like Chroma Key min and max range, check the `MixedRealityCaptureController` and `PlayWebcam` code for those options.
