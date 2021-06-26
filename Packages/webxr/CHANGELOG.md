# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.12.1] - 2021-06-26
### Changed
- How WebGL context clear alpha works.

## [0.12.0] - 2021-06-20
### Added
- Support for WebXR Polyfill config. Need to set window.WebXRPolyfillConfig.

### Changed
- How the interoperability between JavaScript and C# works.

## [0.11.0] - 2021-06-06
### Added
- Support for WebXR XRVisibilityState.

### Changed
- Use view transform matrix instead of orientation/position for pose.

## [0.10.1] - 2021-05-08
### Fixed
- Hand joints don't get radius.
- Hand joints instantiate at wrong position.

## [0.10.0] - 2021-05-06
### Added
- WebXRHandJoint enum instead of const values.

### Changed
- Scripts execution order.
- How XR session switch is handled.
- OnXRChange is called from the Update loop.

### Fixed
- User scaling issue in FullView WebGLTemplates.
- Issue with disabling hand when not tracking.

### Removed
- Const hand joints index values from WebXRHandData.

## [0.9.0] - 2021-04-16
### Added
- CameraFollower to WebXRCamera, for AudioListener to follow active cameras poses.

### Changed
- Optimization for JavaScript to C# communication.

### Fixed
- Errors when building for other platforms using IL2CPP.

### Removed
- Custom JavaScript dispatch events from the WebGLTemplates.

## [0.8.1] - 2021-03-06
### Fixed
- Workaround for Chromium depth bug (Chromium issue 1167450).

## [0.8.0] - 2021-02-26
### Added
- ToggleAR, ToggleVR, isSupportedAR and isSupportedVR to WebXRManager.

### Changed
- WebXRDisplayCapabilities class to struct.

### Removed
- hasExternalDisplay from WebXRDisplayCapabilities.

## [0.7.0] - 2021-02-13
### Added
- Support for `targetRaySpace` for controllers poses.

### Changed
- Use `targetRaySpace` as the default controller pose instead of `gripSpace`.
- Use generic Input Profile in editor for all controllers types.

### Fixed
- Hack for Oculus on Chrome Desktop wrong `targetRaySpace` bug.
- WebXRCamera now removes event listeners OnDisable.

## [0.6.0] - 2021-01-23
### Added
- GetLocalRotation and GetLocalPosition to WebXRCamera.
- Support for the new/updated WebXR Hands API.
- isControllerActive and isHandActive to WebXRController.

### Changed
- Using `frame.fillPoses` and `frame.fillJointRadii` instead of `frame.getJointPose` for XRHand.
- Camera matrices handling.
- WebGLTemplates webxr.js merged into Plugins webxr.jspre.

### Fixed
- Bug related to pose view eye can be `none`.

### Removed
- Matrix utilities files.

## [0.5.2] - 2020-01-16
### Added
- Multi touch support for Handheld AR.

### Fixed
- OnDisable in WebXRController.
- Depth and Stencil clear issue in Handheld AR.
- Ugly hack to fix WebXR Viewer viewports on iOS.

## [0.5.1] - 2020-12-26
### Fixed
- WebXRController Button Up/Down state.
- Protect compilation on other platforms.

## [0.5.0] - 2020-12-19
### Added
- Docs page.
- XRInputSource profiles support.
- Added OnControllerActive, OnHandActive and OnHandUpdate actions to WebXRController.

### Changed
- WebXRController no longer instantiate hand joints.
- Better handling for controllers buttons in the editor.
- WebXRController TryUpdateButtons is now private.

### Fixed
- Disable controllers on XR session end.

## [0.4.1] - 2020-11-28
### Added
- Support for Unity 2020.1.
- WebGLTemplates for Unity 2020.1.
- Debug project for Unity 2020.1.

### Changed
- Fixes for JavaScript and Unity communication to support both Unity 2019.1 and Unity 2020.1.
- Added info about OpenUPM package in README.md.
- Invert Y axis for touchpad and thumbstick, so they'll work the same as other XR Providers in Unity.

## [0.4.0] - 2020-11-08
### Added
- Added missing OnXRCapabilitiesUpdate to WebXRManager.
- Added missing controller inputs.

### Changed
- Changed controller action names to enums.

## [0.3.1] - 2020-11-07
### Added
- emscripten browser object pause/resume when switching sessions.

### Changed
- Replaced Inline XR Session with window.rAF.

## [0.3.0] - 2020-11-01
### Added
- Support for XR providers in the editor.
- Settings asset required and optional features in WebXR Session.

### Changed
- Fixed compile and runtime errors, to make sure that WebGL build works again.

## [0.2.0] - 2020-10-30
### Added
- Some XR SDK features, like the subsystems. Still not fully support the XR SDK.
- Info about WebXR Interactions in the README file.
