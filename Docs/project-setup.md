# Setting up your WebVR project

In this tutorial, we go through the steps of a basic project setup using the WebVR assets.

1. Create a new Unity 3D project

1. Ensure that WebGL platform support is installed.

`Unity > Build Settings`

![WebGL Platform](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/Docs/images/webgl-platform.png)

1. Import the WebVR assets to your project.

Download unity package from github: (WebVR-Assets.unitypackage)[https://github.com/caseyyee/unity-webvr-export/raw/master/WebVR-Assets.unitypackage]

Import the package into your project:

`Assets > Import Package > Custom Package`

![WebVR package](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/Docs/images/import-package.png)

1. Remove default Main Camera

Select `Main Camera` from project Hierchy and right-click and select delete.

1. Add `WebVRCamera` prefab to your Project scene from WebVR Assets folder:

![Import prefab and models](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/Docs/images/camera-prefab-models.gif)

1. Add controllers

Setup GameObjects that you would like to be used for left/hand controllers by attaching 
them to the `WebVRCamera` component `Left Hand Obj` and `Right Hand Obj` fields.

![Controller setup](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/Docs/images/attach-controllers.gif)

1. Select the WebVR template from player settings.

`Edit > Project Settings > Player`

![WebVR template](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/Docs/images/webgl-temlate.png)

1. Build and run your project!

`File > Build & Run`

