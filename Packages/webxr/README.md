# WebXR Export

You can [check the live demo here](https://de-panther.github.io/unity-webxr-export)

## Downloads

There are two options to import the package to a Unity project.

* Use OpenUPM [WebXR Export ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr/). It's the best option, as it is much more easier to update the package later.

* Use Git. It can let you use versions that are yet uploaded to OpenUPM - Mostly happens between releases.

### Using OpenUPM

One way is to set a new `Scoped Registry` in `Project Settings > Package Manager` for OpenUPM.

```
Name: OpenUPM
URL: https://package.openupm.com
Scope(s): com.de-panther
```

Then in `Window > Package Manager` selecting `Packages: My Registries` and the WebXR Export package would be available for install.

### Using Git

To add the package to your Unity project using Git, open the Package Manager window, click on the + icon, "Add package from git URL..." and add the path URL

`https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr`

To update the package, you'll have to manually remove the corresponding package section from the `packages-lock.json` file. For more info read about [Git dependencies](https://docs.unity3d.com/Manual/upm-git.html) in Unity's manual.

## After importing the package

Make sure to check `WebXR Export` in `Project Settings > XR Plug-in Management > WebGL > Plug-in Providers`.

And to copy the WebGLTemplates by `Window > WebXR > Copy WebGLTemplates`.

## WebXR Interactions

You might also want to add the WebXR Interactions package, as it has some Interaction Components and Samples.
You can get the WebXR Interactions package [here](../webxr-interactions/README.md) or at OpenUPM [WebXR Interactions ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr-interactions?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr-interactions/).
