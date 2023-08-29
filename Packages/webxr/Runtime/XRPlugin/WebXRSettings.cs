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
