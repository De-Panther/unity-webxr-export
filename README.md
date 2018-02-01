# Unity WebVR Assets

![Preview of Unity WebVR-exported project in the browser](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/preview.gif)

## **[Try it out!](https://caseyyee.github.io/unity-webvr-export/)**

Assets for creating [WebVR](https://webvr.rocks/)-enabled [Unity3D](https://unity3d.com/) projects.

## Compatibility

These assets work with Unity versions `2017.2.0f3` and later.

### Browser Compatibility

| Platform | Browser | Compatible headsets | |
| --- | --- | --- | --- |
| Desktop | Firefox | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets (using Steam VR) | [Setup instructions](https://webvr.rocks/firefox) |
| Desktop | Microsoft Edge | Windows Mixed Reality headsets | [Setup instructions](https://webvr.rocks/microsoft_edge) |
| Desktop | Chrome Canary | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets | [Setup instructions](https://webvr.rocks/chrome) |

### Polyfilled WebVR
If the user does not have supported headset, browser or device, the content will still work through the use of the [WebVR Polyfill](https://github.com/immersive-web/webvr-polyfill).

### Mobile support

This asset works by utlizing Unity's WebGL platform support and therefore shares the same limitations.  Because of this, mobile support is limited and may not work.  See Unity [Unity WebGL browser compatibility](https://docs.unity3d.com/560/Documentation/Manual/webgl-browsercompatibility.html).

## Setup instructions

To export an existing Unity project to WebVR:

1. [**Download this ZIP file**](https://github.com/caseyyee/unity-webvr-export/archive/master.zip) containing the contents of this repository. Or you can clone the Git repository locally to your machine:
    ```sh
    git clone https://github.com/caseyyee/unity-webvr-export.git && cd unity-webvr-export
    ```
2. Launch **Unity**. ([Download and install Unity](https://store.unity.com/download?ref=personal), or upgrade to the latest version, if you have not already.)
3. Open your existing Unity project: `File > Open Project`.
4. Open **`File > Build Settings`**, and from the `Platform` list, select the **`WebGL`** platform.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/build-settings.png" width="550">

5. From **`Player Settings…`** (`Edit > Project Settings > Player`), select the **`WebGL settings`** tab (HTML5 icon), toggle to the **`Resolution and Presentation`** view, and select **`WebVR`** for the `WebGL Template`.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/webgl-template.png" width="250">

6. Select **`File > Build & Run`**.
    > Or select `File > Build Settings …`, and choose a location for your build.
7. Once the build has completed, your web browser will automatically open the project. Make sure you are running the game in a [*WebVR-capable browser*](https://webvr.rocks/#browsers).
    > Alternatively, if you ran `Build`, instead of `Build & Run`, you can run a [local static-file web server](https://aframe.io/docs/0.7.0/introduction/installation.html#use-a-local-server) and load the `index.html` document from your WebVR-capable browser.


## Building the Unity Asset Package

If you are a maintainer of this project, you will want to update the [`Build/` directory](https://github.com/caseyyee/unity-webvr-export/tree/master/Build/) (hosted [online here](https://caseyyee.github.io/unity-webvr-export/Build/)) and the [`WebVR-Assets.unitypackage` file](https://github.com/caseyyee/unity-webvr-export/blob/master/WebVR-Assets.unitypackage).

1. Launch `Edit > Build Settings > Project Settings`. From `Player Settings…` (`Edit > Project Settings > Player`), select the **`WebGL settings`** tab (HTML5 icon), toggle the **`Resolution and Presentation`** view, and select **`WebVR`** for the `WebGL Template`.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/webgl-template.png" width="250">

2. Launch `Edit > Build Settings > Project Settings`. Then, press the **`Build and Run`** button, and **`Save`** to the directory named **`Build`**.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/build-webgl.png" width="250">

3. Open **`Assets > Export Package…`**, and press the **`Export…`** button. Set **`WebVR-Assets`** as the filename of the destination Unity Asset Package, and press the **`Save`** button.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/export-asset-package.png" width="250">

4. A window titled `Exporting package` will appear. Click the **`Export…`** button to proceed.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/exporting-asset-package.png" width="250">


## License

All code and content within this source-code repository is, unless otherwise specified, licensed under the [**Creative Commons Zero v1.0 Universal** license (CC0 1.0 Universal; Public Domain Dedication)](LICENSE.md).

You can copy, modify, distribute and perform this work, even for commercial purposes, all without asking permission.

For more information, refer to these following links:

* a copy of the [license](LICENSE.md) in [this source-code repository](https://github.com/caseyyee/unity-webvr-export)
* the [human-readable summary](https://creativecommons.org/publicdomain/zero/1.0/) of the [full text of the legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode)
* the [full text of the legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode)
