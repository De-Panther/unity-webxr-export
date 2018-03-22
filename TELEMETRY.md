# Developer Privacy Notice for Data Collection

_Last updated: March 2018_

To help improve the [WebVR API](https://immersive-web.github.io/webvr/spec/1.1/) and the [Unity WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) package, each time a web page built using the [WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) is loaded, Mozilla automatically receives general-usage statistics and uncaught JavaScript errors encountered by end-users, using [Google Analytics](https://analytics.google.com/analytics/web/) and [Sentry](https://sentry.io), respectively. [The *complete list of collected data*](#list-of-collected-data) includes metrics for counting the number of unique web-page sessions; time for web pages to load and time open; JavaScript error exceptions occurred on the page; the number of times a VR device is mounted and worn; number of times VR mode is enabled and time spent; and a random identifier.

You as a developer can turn off this data collection by [modifying the configuration snippet that comes with the VR template](https://github.com/mozilla/unity-webvr-export/blob/master/docs/customization/disabling-telemetry.md). It is your obligation to inform your end-users of this data collection and to inform them that it can be turned off by [enabling “Do-Not-Track”](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/DNT) in their browsers.


## List of Collected Data

- Unique UUID-v4 random identifier (generated according to [IETF RFC4122](http://www.ietf.org/rfc/rfc4122.txt)), persisted in the browser's [`LocalStorage`](https://developer.mozilla.org/en-US/docs/Web/API/Storage/LocalStorage)
- JavaScript error (exception) messages (without file paths)
- Console messages (i.e., error, warning, log, info)
- Respects [Do-Not-Track](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/DNT) (i.e., `DNT: 1` HTTP header and `navigator.doNotTrack === '1'` JavaScript value)
- Dimensions (width × height) of the browser's `screen` and `window`, ratio of the resolution (i.e., `navigator.devicePixelRatio`)
- Release version of the [Unity WebVR Assets](https://github.com/mozilla/unity-webvr-export) package being used (e.g., [`v1.0.1`](https://github.com/mozilla/unity-webvr-export/releases/tag/v1.0.1))
- JavaScript Heap memory used (measured in megabytes)
- [WebXR API](https://immersive-web.github.io/webxr/spec/latest/) support (i.e., `navigator.xr`)
- [WebVR v1.1 API](https://immersive-web.github.io/webvr/spec/1.1/) support (i.e., `navigator.getVRDisplays`)
- [WebVR v1.1 events](https://immersive-web.github.io/webvr/spec/1.1/#interface-window) emitted by the browser (i.e., user-initiated actions and headset events) during a unique page load:
    - Number of times and time until VR mode is entered (e.g., user keypress, user click, automatically presented)
    - Number of times and time until VR mode is exiting (e.g., user keypress, user click, automatically exited, browser's Back button, browser's navigation to another page, etc.)
    - Number of times and time until a VR device is worn/mounted
    - Number of times and time until a VR device is taken off/unmounted
    - Number of times and time until a VR device has been connected (or detected on page load)
    - Number of times and time until a VR device has been disconnected/unplugged
    - Number of times and time until a mouse cursor is temporarily disabled for input while "pointerlocked" in VR mode (e.g., for Windows Mixed Reality's desktop flat-pane views)
    - Number of times and time until a mouse cursor is temporarily disabled for input while "pointerlocked" in VR mode (e.g., for Windows Mixed Reality's desktop flat-pane views)
- Amount of time the active page took to load and to reach:
    - Loading screen
    - Splash screen
    - Unity game
- Amount of time the active page was open for ("session length")
- Browser's `User-Agent` string (i.e., `navigator.userAgent`)
- [WebGL 1.0 API](https://www.khronos.org/registry/webgl/specs/latest/1.0/) support
- [WebGL 2.0 API](https://www.khronos.org/registry/webgl/specs/latest/2.0/) support
- [Gamepad API](https://w3c.github.io/gamepad/)
    - Support of API (i.e., `navigator.getGamepads`)
    - Names of connected gamepads (i.e., `Gamepad#id`)
- [Web Audio API](https://webaudio.github.io/web-audio-api/) support (i.e., `AudioContext`)
- [WebAssembly (WASM) API](http://webassembly.org) support (i.e., `WebAssembly`)
- [Web Worker](https://developer.mozilla.org/en-US/docs/Web/API/Web_Workers_API) support (i.e., `Worker`)
- [Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API) (i.e., `navigator.serviceWorker`)
- [`requestIdleCallback` API](https://developer.mozilla.org/en-US/docs/Web/API/Window/requestIdleCallback) (i.e., `window.requestIdleCallback`)
