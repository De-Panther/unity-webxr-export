# Troubleshooting & FAQ

## What is WebVR?

The WebVR Assets package uses the WebVR browser APIs, a standards-based implementation that removes the need for any platform-specific SDKs and handles responsiveness for a variety of VR configurations.

**[See WebVR specification.](https://immersive-web.github.io/webvr/spec/1.1/)**

## What browsers and headsets are supported?

**[See browser and headset compatibility.](https://github.com/mozilla/unity-webvr-export#unity-compatibility)**

## Why Unity?

Unity is a game development platform that has been a very popular choice amongst VR and indie game developers.  There is a huge community out there to support creators, in particular a massive collection of assets in the [Asset Store](https://assetstore.unity.com/), which reduces the time and skills needed to build content.  Unity has a relatively easy-to-use environment and cross-platform integration (including WebGL!) which makes targeting multiple platforms using a single project codebase easy.

## What version of Unity can I experiment with?

[Unity's Personal Edition](https://store.unity.com/products/unity-personal) is completely free to use for beginners, students and hobbyists.  It's a great place to start and only has a few limitations. The Unity splash screen is required to be shown, and you cannot use the realtime developer analytics nor Unity Cloud Build services.

## What are some other ways to create WebVR content?

If you're a web developer, there are several popular JavaScript tools and frameworks you can use to create VR experiences for the Web:

* [A-Frame](https://aframe.io/)
* [three.js](https://threejs.org/)
* [ReactVR](https://facebook.github.io/react-vr/)
* [Babylon.js](https://www.babylonjs.com/)
* [PlayCanvas](https://playcanvas.com/)
* [JanusVR](http://janusvr.com/)
* [Amazon Sumerian](https://aws.amazon.com/sumerian/)

## How does it work?

The package works by using a [custom WebGL template](https://docs.unity3d.com/Manual/webgl-templates.html) and Unity's [message passing](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html) to send the necessary headset and controller values from the WebVR API into Unity, the values are then applied to cameras and controllers before being then rendered.  The rendered frames are then submitted to the headset using WebVR API.

## Why wouldn't I just create a native app?

The web makes it easy to deliver content to users. Just navigate or link to a experience.  It works just like any other web content and can be used without requiring downloads, installs or approvals.  Meet your users where they are.

## Will it work on mobile?

The WebVR Assets package targets desktop VR browsers. Mobile VR browser support is limited and generally not recommended at this time due to the performance and memory limitations of the Unity WebGL Player on mobile browsers.

With that said, we have had luck using the Oculus Go (Oculus browser) and Google Pixel (Chrome with Daydream) devices as well as more powerful Android devices such as the Google Pixel and Samsug Galaxy using Polyfilled Cardboard support. Safari on iOS [fails to load](https://github.com/mozilla/unity-webvr-export/issues/181). We are investigating ways to make the exporter more usable on mobile. Stay tuned!

Please see [compatibility details](../README.md) for more information.

## Can I use these components in production?

The components are definitely a work in progress and are in a experimental stage of development, but we're committed to making it possible to deploy real applications with it.

## I'm getting out-of-memory errors (`Maximum call-stack size exceeded`)

You can increase the memory that is available to your application from the player setting in Unity from `Edit > Project Settings > Player setting > WebGL Player Settings`.

<img alt="Unity WebGL player memory settings" src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/docs/images/webgl-memory.png" width="250">

## Do I always have to re-build before I can test my experience?

You can work within the Unity editor by enabling VR support in Unity before publishing to WebVR.

* See [In-editor VR playback for rapid testing](./xr-testing.md).
