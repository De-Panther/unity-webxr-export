# Troubleshooting & FAQ

## What is WebVR?

The WebVR assets use the WebVR browser API's, a standard-based implementation that removes the need for any platform specific SDK’s and also provides ability to be responsive to different VR configurations.

## How does it work?

The package works by using a [custom WebGL template](https://docs.unity3d.com/Manual/webgl-templates.html) and Unity's [message passing](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html) to send the necessary headset and controller values from the WebVR API into Unity, the values are then applied to cameras and controllers before being then rendered.  The rendered frames are then submitted to the headset using WebVR API.

## Why wouldn't I just create a native app?

The web makes it easy to deliver content to users. Just navigate or link to a experience.  It works just like any other web content and can be used without requiring downloads, installs or approvals.  Meet your users where they are.

## Will it work on mobile?

The package targets desktop VR browsers.   Mobile device support is limited and not recommended. Please see [compatibility section](../README.md) for more information.

We have gotten it to run on Android devices, but performance is an issue and were still investigating ways to make it more useable.

## Can I use these components in production?

The components are definitely a work in progress and are in a experimental stage of development, but we're committed to making it possible to deploy real applications with it.

## I'm getting out-of-memory errors. (also Maximum call-stack size exceeded)

You can increase the memory that is available to your application from the player setting in Unity from `Edit > Project Settings > Player setting > WebGL Player Settings`

<img alt="Unity WebGL player memory settings" src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/docs/images/webgl-memory.png" width="250">

## Do I always have to re-build before I can test my experience?

You will not be able to directly use WebVR from within the Unity editor environment.  Because of this, you will need to re-build the project before testing within the browser.

There is the option of enabling XR support in Unity player settings to preview your content before building to WebVR, but the results can be quite different and controller integration is not yet supported.

We are investigating ways that this could be made easier.
