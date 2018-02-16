# Unity WebVR Assets

![Preview of Unity WebVR-exported project in the browser](https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/preview.gif)

## **[Try it out!](https://mozilla.github.io/unity-webvr-export/)**

Assets for creating [WebVR](https://webvr.rocks/)-enabled [Unity3D](https://unity3d.com/) projects.

## Supported Unity Versions

These assets work with Unity versions `2017.3.0` or higher.

### Browser Compatibility

| Platform | Browser | Compatible headsets | |
| --- | --- | --- | --- |
| Desktop | Firefox | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets (using Steam VR) | [Setup instructions](https://webvr.rocks/firefox) |
| Desktop | Microsoft Edge | Windows Mixed Reality headsets | [Setup instructions](https://webvr.rocks/microsoft_edge) |
| Desktop | Chrome Canary | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets | Browser flags required. [Setup instructions](https://webvr.rocks/chrome#setup) |

### Polyfilled WebVR

If the user does not have a supported headset, browser, or device, the web page will still work through the use of the [WebVR Polyfill](https://github.com/immersive-web/webvr-polyfill).

### Mobile support

This asset works by utilizing [Unity's WebGL platform support](https://docs.unity3d.com/2018.1/Documentation/Manual/webgl-gettingstarted.html) and therefore shares the same limitations. Because of this, mobile support is limited and may not work. See [Unity's **WebGL Browser Compatibility** table](https://docs.unity3d.com/2018.1/Documentation/Manual/webgl-browsercompatibility.html).

#### Chrome for Android

For WebVR content to work with the regular release version of Chrome for Android, users will need to [manually enable WebVR and Gamepad extensions](https://webvr.rocks/chrome_for_android#setup) from `chrome://flags`.

Alternatively, sites can register for a [WebVR Origin Trial token](https://webvr.rocks/chrome_for_android#what_is_the_webvr_origin_trial) which will allows the API to be available for whitelisted origins.

## Getting started

* [Setting up a Unity project for WebVR](./docs/project-setup.md)
* [Publishing](./docs/publishing.md)
* [Troubleshooting and FAQ](./docs/faq.md)
* [Controllers and input system](./docs/controllers.md)

## Need help?

* [Join the **WebVR Slack** and chat with us **`#unity` channel**](https://webvr.rocks/slack)

## Contributing

Contributions from the developer community are very important to us. You're encouraged to [open an issue](https://github.com/mozilla/unity-webvr-export/issues/new), report a problem, contribute with code, open a feature request, share your work, or ask a question.

Be sure to check out the contributor guides as well:

* [Building the Unity Assets Package](./docs/build.md)

## License

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
