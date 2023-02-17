# [WebXR Export](https://github.com/De-Panther/unity-webxr-export/)

You can [check the live demo here](https://de-panther.github.io/unity-webxr-export/Build).

Develop and export WebXR experiences using the [Unity Engine](https://unity.com/).

WebXR Export supports both Augmented Reality and Virtual Reality [WebXR API](https://www.w3.org/TR/webxr/) immersive sessions.

It integrates the WebXR JavaScript API to [Unity WebGL](https://docs.unity3d.com/Manual/webgl.html) and lets you develop experiences in the familiar editor using C#.

[⭐Star to show interest⭐](https://github.com/De-Panther/unity-webxr-export/stargazers)

![Demo](/Documentation/webxr-input-profiles.gif)

<hr>

## 💾 Downloads

Better option: Import WebXR Export and WebXR Interactions packages from OpenUPM.
- [WebXR Export ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr/)
- [WebXR Interactions ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr-interactions?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr-interactions/)

Or import the packages from git, more info at [WebXR Export](Packages/webxr/README.md) and [WebXR Interactions](Packages/webxr-interactions/README.md).

## 📄 Documentation

For more info on getting started and how to use WebXR Export, you can check the [Documentation](Documentation) folder.

## 🧩 Compatibility

**Important notice** regarding this Git repository - This repository use Symlinks. make sure that Symlinks are enabled when you clone.

`git config core.symlinks true`

Or just use this as a Unity Package (UPM), more info [here](Packages/webxr/README.md)

### 📝 Unity Editor version

* `2019.4.33` and up.
* `2020.1` and up.
* `2021.1` and up.

### 🗺️ Browser Compatibility

This is a list of browsers and platforms that the WebXR Export demo was tested on.

Google Chrome on Windows.

Microsoft Edge on Windows.

Mozilla Firefox on Windows.

Oculus Browser on Oculus Quest (Supports WebXR Hand Input).

Firefox Reality on Oculus Quest.

Firefox Reality on Hololens 2 (Supports WebXR Hand Input).

Microsoft Edge on Hololens 2.

Google Chrome on Android (Supports both AR and VR sessions).

Samsung Internet Browser on Android (Supports both AR and VR sessions).

Firefox Reality on Vive Focus Plus.

#### 🐞 Has bugs/issues

Mozilla WebXR Viewer on iOS (Has a bug related to touch input).

Helio on Magic Leap One (Has a bug related to eye views).

### ⚙️ WebXR APIs support

[WebXR Device API](https://immersive-web.github.io/webxr/)

[WebXR Gamepads Module](https://immersive-web.github.io/webxr-gamepads-module/) (Including [hapticActuators](https://w3c.github.io/gamepad/extensions.html) on supported devices)

[WebXR Augmented Reality Module](https://immersive-web.github.io/webxr-ar-module/)

[WebXR Hit Test Module](https://immersive-web.github.io/hit-test/) (Currently limited to viewer space Hit Test source)

[WebXR Hand Input](https://github.com/immersive-web/webxr-hand-input)

[WebXR Input Profiles](https://github.com/immersive-web/webxr-input-profiles) using [WebXR Input Profiles Loader](https://github.com/De-Panther/webxr-input-profiles-loader)

![Hands](/Documentation/webxr-hands-api.gif)

#### More features

Spectator Camera

[WebXR Mixed Reality Capture](Documentation/WebXR-Mixed-Reality-Capture.md)

### 📊 Polyfilled WebXR / WebVR

If the user does not have supported headset, browser or device, the content will still work through the use of the [WebXR Polyfill](https://github.com/immersive-web/webxr-polyfill).

### 📱 Mobile support

This asset works by utilizing Unity's WebGL platform support and therefore shares the same limitations. Because of this, mobile support is limited and may not work. We did made sure to make it light, so mobile devices would be able to handle it, and it is able to run on high end mobile devices with some optimizations. See [Unity's WebGL browser compatibility](https://docs.unity3d.com/2019.3/Documentation/Manual/webgl-browsercompatibility.html).

### 📜 Version History and Notes

You can check the changelogs of [WebXR Export](Packages/webxr/CHANGELOG.md) and [WebXR Interactions](Packages/webxr-interactions/CHANGELOG.md) for version history.

## 🤝 Contributing

You're encouraged to [open an issue](https://github.com/De-Panther/unity-webxr-export/issues/new), report a problem, contribute with code, open a feature request, share your work or ask a question. But remember that it's an experimental project.

## 🔗 Useful links and info

Much of the WebXR upgrades and API usage made possible thanks to these resources.

[WebVR to WebXR Migration Guide](https://github.com/immersive-web/webxr/blob/master/webvr-migration.md)

[WebXR Device API Explained](https://github.com/immersive-web/webxr/blob/master/explainer.md)

[WebXR Input Profile Viewer](https://immersive-web.github.io/webxr-input-profiles/packages/viewer/dist/index.html)

[WebXR Samples](https://immersive-web.github.io/webxr-samples/)

[WebXR Polyfill](https://github.com/immersive-web/webxr-polyfill)

[The Immersive Web Working Group/Community Group](https://immersive-web.github.io/)

[SimpleWebXR by Rufus31415](https://github.com/Rufus31415/Simple-WebXR-Unity)

[WebXR Discord](https://discord.gg/Jt5tfaM)

## 🎞️ Credits

Special thanks to [needle.tools](https://needle.tools/) for the help in making this a proper package.

Thanks to [Brandon Jones (@toji)](https://github.com/toji) who wrote [WebVR to WebXR Migration Guide](https://github.com/immersive-web/webxr/blob/master/webvr-migration.md) and lots of samples that helped in converting the code from WebVR to WebXR.

Mozilla's Unity WebVR Export credits:

This project was heavily influenced by early explorations in using Unity to build for WebVR by [@gtk2k](https://github.com/gtk2k), [Chris Miller (@chrmi)](https://github.com/chrmi) and [Anthony Palma](https://twitter.com/anthonyrpalma).

Also, thanks to [Arturo Paracuellos (@arturitu)](https://github.com/arturitu) for creating the [3D-hand models](https://github.com/aframevr/assets/tree/gh-pages/controllers/hands) used for controllers in these examples.

## ⚖️ License

As the base project used the Apache License, Version 2.0, we will continue with it.

WebXR Export License:

Copyright 2020 Oren Weizman

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

<hr>

This is a project based on Mozilla's [Unity WebXR Exporter](https://github.com/MozillaReality/unity-webxr-export) (from when it was WebVR exporter).

Unity WebVR Exporter License:

Copyright 2017 - 2018 Mozilla Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
