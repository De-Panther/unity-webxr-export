# Unity WebXR Export

You can [check live demo here](https://de-panther.github.io/unity-webxr-export)

This is a project based on Mozilla's [Unity WebXR Export](https://github.com/MozillaReality/unity-webxr-export) (from when it was WebVR export).

WebVR and WebXR, while having lots in common, are different in the way they calling a frame, using controllers, and the fact the WebXR have the ground for support AR and not just VR.

That, and the fact that I want to use more updated version of Unity Editor and tools/practices, made me to create this fork.

The current docs are still in the repository as reference, but I modified this README file, as some of the links and info are no longer relevant or won't be relevant soon.

<hr>

## Compatibility

### Unity editor version

* `2019.3` and above.

### Browser Compatibility

Tested with Firefox on Windows, and Oculus Browser on Oculus Quest

### Polyfilled WebXR

If the user does not have supported headset, browser or device, the content will still work through the use of the [WebXR Polyfill](https://github.com/immersive-web/webxr-polyfill).

### Mobile support

This asset works by utilizing Unity's WebGL platform support and therefore shares the same limitations. Because of this, mobile support is limited and may not work. See [Unity's WebGL browser compatibility](https://docs.unity3d.com/2019.3/Documentation/Manual/webgl-browsercompatibility.html).

### Version History and Notes

## Contributing

Contributions from the developer community are very important to us. You're encouraged to [open an issue](https://github.com/De-Panther/unity-webxr-export/issues/new), report a problem, contribute with code, open a feature request, share your work or ask a question.

## Developer Privacy Notice for Data Collection

_Last updated: March 2018_

To help improve the [WebVR API](https://immersive-web.github.io/webvr/spec/1.1/) and the [Unity WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) package, each time a web page built using the [WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) is loaded, Mozilla automatically receives general-usage statistics and uncaught JavaScript errors encountered by end-users, using [Google Analytics](https://analytics.google.com/analytics/web/) and [Sentry](https://sentry.io), respectively. [The *complete list of collected data*](https://github.com/mozilla/unity-webvr-export/blob/master/TELEMETRY.md#list-of-collected-data) includes metrics for counting the number of unique web-page sessions; time for web pages to load and time open; JavaScript error exceptions occurred on the page; the number of times a VR device is mounted and worn; number of times VR mode is enabled and time spent; and a random identifier.

You as a developer can turn off this data collection by [modifying the configuration snippet that comes with the VR template](https://github.com/mozilla/unity-webvr-export/blob/master/docs/customization/disabling-telemetry.md). It is your obligation to inform your end-users of this data collection and to inform them that it can be turned off by [enabling “Do-Not-Track”](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/DNT) in their browsers.


## Credits

Thanks to [Brandon Jones (@toji)](https://github.com/toji) who wrote [WebVR to WebXR Migration Guide](https://github.com/immersive-web/webxr/blob/master/webvr-migration.md) and lots of samples that helped in converting the code from WebVR to WebXR.

Mozilla's Unity WebVR Export credits:

This project was heavily influenced by early explorations in using Unity to build for WebVR by [@gtk2k](https://github.com/gtk2k), [Chris Miller (@chrmi)](https://github.com/chrmi) and [Anthony Palma](https://twitter.com/anthonyrpalma).

Also, thanks to [Arturo Paracuellos (@arturitu)](https://github.com/arturitu) for creating the [3D-hand models](https://github.com/aframevr/assets/tree/gh-pages/controllers/hands) used for controllers in these examples.

## License

As the base project used the Apache License, Version 2.0, we will continue with it.

Unity WebVR Export License:

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
