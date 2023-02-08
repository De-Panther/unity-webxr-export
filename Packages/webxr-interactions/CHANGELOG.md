# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.16.3] - 2023-02-08
### Fixed
- SceneHitTest right controller issue.
- Webcam performance.

## [0.16.2] - 2023-02-04
### Fixed
- Webcam errors in different versions of Unity.

## [0.16.1] - 2023-02-03
### Fixed
- No reference to controllers in SceneHitTest.
- Errors when trying to play webcam stream.

## [0.16.0] - 2023-02-02
### Added
- Spectator Camera.
- Mixed Reality Capture.

### Changed
- Requires WebXR Export 0.16.0.

### Fixed
- WebXR input profiles loading on SteamVR.

## [0.15.0] - 2022-06-04
### Changed
- Requires WebXR Export 0.15.0.

## [0.14.1] - 2022-02-05
### Fixed
- Null reference errors when destroying contacted interactables.

## [0.14.0] - 2021-12-26
### Added
- Support for AR headsets in SceneHitTest.
- Velocity for interactables when dropped.

### Changed
- Minimum WebXR Input Profiles Loader version 0.4.0.
- Minimum Unity Editor version 2019.4.33.

## [0.13.0] - 2021-10-18
### Changed
- Requires WebXR Export 0.13.0.

## [0.12.0] - 2021-06-20
### Changed
- Minimum WebXR Input Profiles Loader version 0.3.7.
- Sample scene assets settings optimizations.

## [0.11.0] - 2021-06-06
### Added
- An option to disable/enable useCollidersForHandJoints in ControllerInteraction.

## [0.10.0] - 2021-05-06
### Added
- Support for Generic Hands models from the WebXR Input Profiles.

### Changed
- Minimum WebXR Input Profiles Loader version 0.3.6.

## [0.9.0] - 2021-04-16
### Changed
- AudioListener in the WebXRCameraSet prefabs is now attached to CameraFollower.

## [0.8.0] - 2021-02-26
### Fixed
- Smearing issues in AR.

### Changed
- AR cameras Clear Flags in the WebXRCameraSet prefabs.

## [0.7.0] - 2021-02-13
### Added
- Check for alwaysUseGrip in ControllerInteraction.

### Changed
- Minimum WebXR Input Profiles Loader version 0.3.5.

## [0.6.0] - 2021-01-23
### Added
- Check if hand joints radius changed on hand update in ControllerInteraction.
- Check if isControllerActive and isHandActive on enable in ControllerInteraction.

## [0.5.2] - 2021-01-16
### Fixed
- OnDisable in ControllerInteraction.
- Missing material in default WebXRCameraSet.

## [0.5.1] - 2020-12-26
### Fixed
- Material references in prefabs and sample scene.

## [0.5.0] - 2020-12-19
### Added
- Docs page.
- WebXR Input Profiles Loader support.
- UI to toggle WebXR Input Profiles Loader in the sample scene.

### Changed
- ControllerInteraction handles hand joints.
- Disabled the transparent cube in the sample scene. (It's still there for tests)

### Fixed
- Working default WebXRCameraSet prefab.

## [0.4.2] - 2020-11-28
### Changed
- WebXR Export dependency update.
- Added info about OpenUPM package in README.md.

## [0.4.1] - 2020-11-17
### Changed
- WebXR Export dependency fix.

## [0.4.0] - 2020-11-08
### Added
- Support for the missing controller inputs.

### Changed
- Sample scene - Changed Rigidbody Interpolate for the Interactables.
- ControllerInteraction - Changed what values to check for pick/drop Interactables.

## [0.3.0] - 2020-11-01
### Added
- Support for XR providers in the sample WebXRCameraSet prefab.

### Changed
- Fixed compile and runtime errors, to make sure that WebGL build works again.

## [0.2.0] - 2020-10-30
### Added
- This package.
- Sample scene from the main project
