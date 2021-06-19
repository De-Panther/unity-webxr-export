# WebXR Interactions

This package adds Interaction Components and Samples for the WebXR Export package

You can [check the live demo here](https://de-panther.github.io/unity-webxr-export)

## Downloads

There are two options to import the package to a Unity project.

* Use OpenUPM [WebXR Interactions ![openupm](https://img.shields.io/npm/v/com.de-panther.webxr-interactions?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.de-panther.webxr-interactions/). It's the best option, as it is much more easier to update the package later.

* Use Git. It can let you use versions that are yet uploaded to OpenUPM - Mostly happens between releases.

### Using OpenUPM

One way is to set a new `Scoped Registry` in `Project Settings > Package Manager` for OpenUPM.

```
Name: OpenUPM
URL: https://package.openupm.com
Scope(s): com.de-panther
```

Then in `Window > Package Manager` selecting `Packages: My Registries` and the WebXR Interactions package would be available for install.

The WebXR Interactions package supports the [WebXR Input Profiles Loader](https://github.com/De-Panther/webxr-input-profiles-loader) package and depends on [glTFast](https://github.com/atteneder/glTFast) for that.

Add `com.atteneder` to the scopes list of the OpenUPM registry for Unity to locate the glTFast package.

### Using Git

To add the package to your Unity project using Git, open the Package Manager window, click on the + icon, "Add package from git URL..." and add the path URL

`https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr-interactions`

To update the package, you'll have to manually remove the corresponding package section from the `packages-lock.json` file. For more info read about [Git dependencies](https://docs.unity3d.com/Manual/upm-git.html) in Unity's manual.

## Sample Scene

You can import the `Sample Scene` from `Window > Package Manager > WebXR Interactions > Sample Scene > Import into Project`.