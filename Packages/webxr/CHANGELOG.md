# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
