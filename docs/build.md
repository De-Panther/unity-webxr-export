# Building the Unity Assets Package

If you are a maintainer of this project, and you modified the demo scene distributed with the package, you will want to update the [`Build/` directory](https://github.com/mozilla/unity-webvr-export/tree/master/Build/) (hosted [online here](https://caseyyee.github.io/unity-webvr-export/Build/)).

1. Launch `Edit > Build Settings > Project Settings`. From `Player Settings…` (`Edit > Project Settings > Player`), select the **`WebGL settings`** tab (HTML5 icon), toggle the **`Resolution and Presentation`** view, and select **`WebVR`** for the `WebGL Template`.

    <img alt="WebGL template selector" src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/webgl-template.png" width="250">

2. Launch `Edit > Build Settings > Project Settings`. Then, press the **`Build and Run`** button, and **`Save`** to the directory named **`Build`**.

    <img alt="Selecting the Build folder" src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/build-webgl.png" width="250">

If you are contributing to the assets and want to and want to update the [`WebVR-Assets.unitypackage` file](../WebVR-Assets.unitypackage)...

1. Open **`Assets > Export Package…`**, and press the **`Export…`** button. Set **`WebVR-Assets`** as the filename of the destination Unity Asset Package, and press the **`Save`** button.

    <img alt="" src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/export-asset-package.png" width="250">

2. A window titled `Exporting package` will appear. Click the **`Export…`** button to proceed.

    <img src="https://raw.githubusercontent.com/mozilla/unity-webvr-export/master/img/exporting-asset-package.png" width="250">