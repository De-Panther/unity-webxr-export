# WebXR Export

You can [check the live demo here](https://de-panther.github.io/unity-webxr-export)

To add the package to your Unity project, open the Package Manager window, click on the + icon, "Add package from git URL..." and add the path URL

`https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr`

Then make sure to check `WebXR Export` in `Project Settings > XR Plug-in Management > WebGL > Plug-in Providers`.

To update the package, you'll have to manually remove the corresponding package section from the `packages-lock.json` file. For more info read about [Git dependencies](https://docs.unity3d.com/Manual/upm-git.html) in Unity's manual.


You might also want to add the WebXR Interactions package, as it has some Interaction Components and Samples.
You can get the WebXR Interactions package [here](../webxr-interactions/README.md)
