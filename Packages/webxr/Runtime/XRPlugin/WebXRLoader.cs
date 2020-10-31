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

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void SetWebXRSettings(string settingsJson);
#endif

    WebXRSettings GetSettings()
    {
      WebXRSettings settings = null;
      // When running in the Unity Editor, we have to load user's customization of configuration data directly from
      // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
      UnityEditor.EditorBuildSettings.TryGetConfigObject<WebXRSettings>("WebXR.Settings", out settings);
#elif UNITY_WEBGL
      settings = WebXRSettings.Instance;
#endif
      return settings;
    }

    public override bool Initialize()
    {
      WebXRSettings settings = GetSettings();
      if (settings != null)
      {
        Debug.Log($"Got WebXRSettings");
#if UNITY_WEBGL && !UNITY_EDITOR
        SetWebXRSettings(settings.ToJson());
#endif
        Debug.Log($"Sent WebXRSettings");
      }

      CreateSubsystem<WebXRSubsystemDescriptor, WebXRSubsystem>(sampleSubsystemDescriptors, typeof(WebXRSubsystem).FullName);
      return WebXRSubsystem != null;
    }


    public override bool Start()
    {
      WebXRSubsystem.Start();
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
      return base.Deinitialize();
    }
  }
}