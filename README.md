# Unity WebVR Assets

### WebVR assets for creating WebVR-enabled Unity projects.

![Preview](https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/preview.gif)

**Supports**
* Room-scale environments with 6dof controller support.
* Work with both Oculus Rift using Touch, and HTC VIVE.
* For now, desktop WebVR only.

## How to use

### Open and Build project

1. Clone or Download the contents of this repository.

2. Open the project from `File > Open Project` in Unity.

3. Go to `File > Build Settings`, select the _WebGL_ platform.

<img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/build-settings.png" width="550">

4. Select _Player Settings_ and from _Settings for WebGL_ select the `WebVR` template.

<img src="https://raw.githubusercontent.com/caseyyee/unity-webvr-export/master/img/webgl-template.png" width="250">

5. Press _Build_ and select a location for your build.

6. Once the build is completed, browse to the build open `index.html` from a WebVR enabled browser.

You may need to serve the files from a local web server if your browser does not support running content from `file://` url's.
