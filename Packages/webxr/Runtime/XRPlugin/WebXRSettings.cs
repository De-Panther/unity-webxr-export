using System;
using UnityEngine;
using UnityEngine.XR.Management;

namespace WebXR
{
  [System.Serializable]
  [XRConfigurationData("WebXR", "WebXR.Settings")]
  public class WebXRSettings : ScriptableObject
  {
    public enum ReferenceSpaceTypes
    {
      local = 1,
      local_floor = 2,
      bounded_floor = 4,
      unbounded = 8,
      viewer = 16,
    }

    [Flags]
    public enum ExtraFeatureTypes
    {
      hit_test = 1,
      hand_tracking = 2
    }

    [Header("VR Settings")]
    public ReferenceSpaceTypes VRRequiredReferenceSpace = ReferenceSpaceTypes.local_floor;
    public ExtraFeatureTypes VROptionalFeatures = ExtraFeatureTypes.hand_tracking;

    [Header("AR Settings")]
    public ReferenceSpaceTypes ARRequiredReferenceSpace = ReferenceSpaceTypes.local_floor;
    public ExtraFeatureTypes AROptionalFeatures = (ExtraFeatureTypes)(-1);

    [Header("More Settings")]
    [Tooltip(@"Should manually set FramebufferScaleFactor?
The scale factor in which the scene is rendered in.
Default is the recommended resolution. Can be different than native resolution.")]
    public bool UseFramebufferScaleFactor = false;
    [Tooltip(@"If ""Use Framebuffer Scale Factor"" is true, should use native resolution?")]
    public bool UseNativeResolution = false;
    [Tooltip(@"If ""Use Framebuffer Scale Factor"" is true, and not using native resolution, what should be the scale factor?
Default is 1.0, the recommended resolution.")]
    [Range(0.2f,2.0f)]
    public float FramebufferScaleFactor = 1.0f;
    [Tooltip(@"Should WebXRManager be created on start or manually by the developer.")]
    public bool AutoLoadWebXRManager = true;
    [Tooltip(@"Should WebXRInputSystem be created on start or manually by the developer.
WebXRInputSystem is needed when using Unity Input System and XR Interaction Toolkit.
WebXRInputSystem is part of WebXR Interactions package.")]
    public bool AutoLoadWebXRInputSystem = true;
    [Tooltip(@"Should XRDisplaySubsystem be used?
By default it is in use and require URP.
Disabling it can allow the use of BiRP,
but it's less convenient as it means using a list of Cameras instead of 1.
If XRDisplaySubsystem is disabled use the WebXRCamera component.")]
    public bool DisableXRDisplaySubsystem = false;

    string EnumToString<T>(T value) where T : Enum
    {
      return value.ToString().Replace('_','-');
    }

    string FlagsToString<T>(T value) where T : Enum
    {
      if (value.ToString() == "0")
      {
        return "[]";
      }
      var flags = Enum.GetValues(typeof(T));
      string result = "[";
      foreach (var flag in flags)
      {
        if (value.HasFlag((Enum)flag))
        {
          result += "\"" + flag + "\",";
        }
      }
      result = result.Remove(result.Length - 1).Replace('_','-');
      result += "]";
      return result;
    }

    // TODO: Replace with a better way to send the settings object to native
    [ContextMenu("ToJson")]
    public string ToJson()
    {
      string result = $@"{{
        ""VRRequiredReferenceSpace"": [""{EnumToString(VRRequiredReferenceSpace)}""],
        ""VROptionalFeatures"": {FlagsToString(VROptionalFeatures)},
        ""ARRequiredReferenceSpace"": [""{EnumToString(ARRequiredReferenceSpace)}""],
        ""AROptionalFeatures"": {FlagsToString(AROptionalFeatures)},
        ""UseFramebufferScaleFactor"": {(UseFramebufferScaleFactor ? "true" : "false")},
        ""UseNativeResolution"": {(UseNativeResolution ? "true" : "false")},
        ""FramebufferScaleFactor"": {FramebufferScaleFactor}
}}";
      return result;
    }

    public static WebXRSettings GetSettings()
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

#if !UNITY_EDITOR
    private static WebXRSettings instance = null;
    public static WebXRSettings Instance
    {
      get { return instance; }
    }

    void Awake()
    {
      instance = this;
    }
#endif
  }
}
