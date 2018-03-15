# Unity WebVR Assets

Assets for creating [WebVR](https://webvr.rocks/)-enabled [Unity3D](https://unity3d.com/) projects.

**[Check out the demo now!](https://mozilla.github.io/unity-webvr-export/)**

[![Preview of Unity WebVR-exported project in the browser](https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/preview.gif)](https://mozilla.github.io/unity-webvr-export/)

**[Free to download](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152)** and available now on the [Unity Asset Store](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152).

<a href="https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152" title="Download the WebVR Assets package for free on the Unity Asset Store">
<img src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/asset-store.png" width="640" alt="Download the WebVR Assets package for free on the Unity Asset Store">
</a>

<hr>

## Unity Compatibility

These assets work with Unity versions `2017.3.0` and above.

### Browser Compatibility

| Platform | Browser | Compatible headsets | |
| --- | --- | --- | --- |
| Desktop | Firefox | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets (using Steam VR) | [Setup instructions](https://webvr.rocks/firefox) |
| Desktop | Microsoft Edge | Windows Mixed Reality headsets | [Setup instructions](https://webvr.rocks/microsoft_edge) |
| Desktop | Chrome Canary | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets | Browser flags required. [Setup instructions](https://webvr.rocks/chrome) |

### Polyfilled WebVR

If the user does not have supported headset, browser or device, the content will still work through the use of the [WebVR Polyfill](https://github.com/immersive-web/webvr-polyfill).

### Mobile support

This asset works by utilizing Unity's WebGL platform support and therefore shares the same limitations. Because of this, mobile support is limited and may not work. See [Unity's WebGL browser compatibility](https://docs.unity3d.com/2018.1/Documentation/Manual/webgl-browsercompatibility.html).


## Getting started

* [Setting up a Unity project for WebVR](./docs/project-setup.md)
* [Publishing](./docs/publishing.md)
* [Troubleshooting and FAQ](./docs/troubleshooting-faq.md)
* [Controllers and input system](./docs/controllers.md)

## Customize the template

* [Adding Google Analytics to your game](./docs/customization/adding-ga.md)

## Need help?

* [Join the WebVR Slack](https://webvr.rocks/slack) (join the [#unity channel](https://webvr.slack.com/messages/unity))


## Contributing

Contributions from the developer community are very important to us. You're encouraged to [open an issue](https://github.com/mozilla/unity-webvr-export/issues/new), report a problem, contribute with code, open a feature request, share your work or ask a question.

Take a look at the contributor guides too.

* [Building the Unity Assets Package](./docs/build.md)


## We want to hear from you!

We’d love to hear about what you come up with using the _WebVR Assets_. **Share your work with us** and use the [#unitywebvr](https://twitter.com/search?f=tweets&q=%23unitywebvr) Twitter hashtag.

The Unity WebVR Assets is an open-source project (licensed under Apache 2) [available on GitHub](https://github.com/mozilla/unity-webvr-export).

* [View known issues](https://github.com/mozilla/unity-webvr-export/issues)
* [File an issue or feature request](https://github.com/mozilla/unity-webvr-export/issues/new)
* [Contribute code or documentation to the project](https://github.com/mozilla/unity-webvr-export#contributing)

Reach out to us with any questions you may have or help you may need, and participate in the discussions on the [WebVR Slack](https://webvr.rocks/slack) in the [#unity channel](https://webvr.slack.com/messages/unity).

* [Join the WebVR Slack](https://webvr.rocks/slack) (join the [#unity channel](https://webvr.slack.com/messages/unity))


## Developer Privacy Notice for Data Collection

_Last updated: March 2018_

To help improve the [WebVR API](https://immersive-web.github.io/webvr/spec/1.1/) and the [Unity WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) package, each time a web page built using the [WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) is loaded, Mozilla automatically receives general-usage statistics and uncaught JavaScript errors encountered by end-users, using [Google Analytics](https://analytics.google.com/analytics/web/) and [Sentry](https://sentry.io), respectively. [The *complete list of collected data*](./TELEMETRY.MD#list-of-collected-data) includes metrics for counting the number of unique web-page sessions; time for web pages to load and time open; JavaScript error exceptions occurred on the page; the number of times a VR device is mounted and worn; number of times VR mode is enabled and time spent; and a random identifier.

You as a developer can turn off this data collection by [modifying the configuration snippet that comes with the VR template](https://github.com/mozilla/unity-webvr-export/blob/master/docs/customization/disabling-telemetry.md). It is your obligation to inform your end-users of this data collection and to inform them that it can be turned off by [enabling “Do-Not-Track”](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/DNT) in their browsers.


## Credits

This project was heavily influenced by early explorations in using Unity to build for WebVR by [@gtk2k](https://github.com/gtk2k).

Also, thanks to [@arturitu](https://github.com/arturitu) for creating the [3D-hand models](https://github.com/aframevr/assets/tree/gh-pages/controllers/hands) used for controllers in these examples.

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
