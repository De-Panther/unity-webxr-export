# Unity WebVR Assets

WebVR assets for creating [WebVR](https://webvr.rocks/)-enabled [Unity3D](https://unity3d.com/) projects.

![Preview of Unity WebVR-exported project in the browser](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/preview.gif)

**[Try it out!](https://caseyyee.github.io/unity-webvr-export/)**

Works with [HTC VIVE](https://webvr.rocks/htc_vive) and [Oculus Rift](https://webvr.rocks/oculus_rift) using a [WebVR-enabled browser](https://webvr.rocks/#browsers).


## Setup instructions

With an existing Unity project:

1. [**Download this ZIP file**](https://github.com/caseyyee/unity-webvr-export/archive/master.zip) containing the contents of this repository. Or you can clone the Git repository locally to your machine:
    ```sh
    git clone https://github.com/caseyyee/unity-webvr-export.git && cd unity-webvr-export
    ```
2. Launch **Unity**.
3. Open your existing Unity project: `File > Open Project`.
3. Open `File > Build Settings`, and select the `WebGL` platform.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/build-settings.png" width="550">

4. From `Player Settings` (`Edit > Project Settings > Player`), select the **`WebGL settings`** tab (HTML5 icon), toggle the **`Resolution and Presentation`** view, and select **`WebVR`** for the `WebGL Template`.

    <img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/webgl-template.png" width="250">

5. Select **`File > Build & Run`**.
    > Or select `File > Build Settings …`, and choose a location for your build.
6. Once the build has completed, your web browser will automatically open the project. Make sure you are running the game in a [*WebVR-capable browser*](https://webvr.rocks/#browsers).
    > Alternatively, if you ran `Build`, instead of `Build & Run`, you can run a [local static-file web server](https://aframe.io/docs/0.7.0/introduction/installation.html#use-a-local-server) and load the `index.html` document from your WebVR-capable browser.


## License

All code and content within this source-code repository is, unless otherwise specified, licensed under the [**Creative Commons Zero v1.0 Universal** license (CC0 1.0 Universal; Public Domain Dedication)](LICENSE.md).

You can copy, modify, distribute and perform this work, even for commercial purposes, all without asking permission.

For more information, refer to these following links:

* a copy of the [license](LICENSE.md) in [this source-code repository](https://github.com/caseyyee/unity-webvr-export)
* the [human-readable summary](https://creativecommons.org/publicdomain/zero/1.0/) of the [full text of the legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode)
* the [full text of the legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode)
