using System.Collections.Generic;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace WebXR
{
  public class WebXRLoader : XRLoaderHelper
  {
    static readonly List<WebXRSubsystemDescriptor> sampleSubsystemDescriptors = new List<WebXRSubsystemDescriptor>();
    static readonly List<XRDisplaySubsystemDescriptor> displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
    static readonly List<XRInputSubsystemDescriptor> inputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();
    public WebXRSubsystem WebXRSubsystem => GetLoadedSubsystem<WebXRSubsystem>();
    public XRDisplaySubsystem XRDisplaySubsystem => GetLoadedSubsystem<XRDisplaySubsystem>();
    public XRInputSubsystem XRInputSubsystem => GetLoadedSubsystem<XRInputSubsystem>();
    private bool useXRDisplaySubsystem = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void SetWebXRSettings(string settingsJson);

    [DllImport("__Internal")]
    private static extern void RegisterWebXRPlugin();
#endif

    public override bool Initialize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
      RegisterWebXRPlugin();
#endif
      WebXRSettings settings = WebXRSettings.GetSettings();
      if (settings != null)
      {
        Debug.Log($"Got WebXRSettings");
#if UNITY_WEBGL && !UNITY_EDITOR
        SetWebXRSettings(settings.ToJson());
#endif
        Debug.Log($"Sent WebXRSettings");
        useXRDisplaySubsystem = !settings.DisableXRDisplaySubsystem;
      }
      XRSettings.useOcclusionMesh = false;
      CreateSubsystem<WebXRSubsystemDescriptor, WebXRSubsystem>(sampleSubsystemDescriptors, typeof(WebXRSubsystem).FullName);
      return WebXRSubsystem != null;
    }

    public void StartEssentialSubsystems()
    {
      if (useXRDisplaySubsystem)
      {
        CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(displaySubsystemDescriptors, "WebXR Display");
        XRDisplaySubsystem.Start();
      }
      CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(inputSubsystemDescriptors, "WebXR Tracked Display");
      XRInputSubsystem.Start();
      // TODO: Enable Single-Pass rendering
      // Debug.LogError(XRDisplaySubsystem.supportedTextureLayouts);
    }

    public void EndEssentialSubsystems()
    {
      if (useXRDisplaySubsystem)
      {
        XRDisplaySubsystem.Stop();
        XRDisplaySubsystem.Destroy();
      }
      XRInputSubsystem.Stop();
      XRInputSubsystem.Destroy();
    }

    public override bool Start()
    {
      WebXRSubsystem.Start();
      WebXRSubsystem.webXRLoader = this;
      return true;
    }

    public override bool Stop()
    {
      WebXRSubsystem.Stop();
      return base.Stop();
    }

    public override bool Deinitialize()
    {
      WebXRSubsystem.Destroy();
      if (useXRDisplaySubsystem)
      {
        XRDisplaySubsystem?.Destroy();
      }
      XRInputSubsystem?.Destroy();
      return base.Deinitialize();
    }
  }
}