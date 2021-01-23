# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.6.0] - 2020-01-23
### Added
- Check if hand joints radius changed on hand update in ControllerInteraction.
- Check if isControllerActive and isHandActive on enable in ControllerInteraction.

## [0.5.2] - 2020-01-16
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
