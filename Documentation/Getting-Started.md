# Getting Started

In this page, we have a step by step guide on how to build a project from the Sample Scene.

## Let's build some WebXR stuff

Create a new Unity Project (2019.4.7f1 and up). Switch platform to WebGL.

Import WebXR Export and WebXR Interactions packages from OpenUPM.
- [WebXR Export ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr/)
- [WebXR Interactions ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr-interactions?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr-interactions/)

Once packages are imported, Go to `Window > WebXR > Copy WebGLTemplates`.

![Copy WebGLTemplates](unity-webxr-export-copy-webgltemplates.png)

After `WebGLTemplates` are in the `Assets` folder, Open the `XR Plug-in Management` tab in the `Project Settings` window and select the `WebXR Export` plug-in provider.

![XR Plug-in Management](unity-webxr-export-xr-plug-in-management.png)

Now you can import the `Sample Scene` from `Window > Package Manager > WebXR Interactions > Sample Scene > Import into Project`.

![Import Sample Scene](unity-webxr-export-import-sample-scene.png)

In `Project Settings > Player > Resolution and Presentation`, select `WebXR` as the `WebGL Template`. (If you are using Unity 2020.x and up you should use the 2020 templates)

![Resolution and Presentation](unity-webxr-export-resolution-and-presentation.png)

Now you can build the project.

![Build](unity-webxr-export-build.png)

WebXR requires a secure context (HTTPS server or localhost URL). Make sure to build the project from `Build Settings > Build`. Unity's `Build And Run` server use HTTP. Run the build on your own HTTPS server.

![Result](unity-webxr-export-result.png)

That's it.
