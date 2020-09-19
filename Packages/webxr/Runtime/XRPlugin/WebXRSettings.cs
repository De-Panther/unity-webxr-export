using UnityEngine;
using UnityEngine.XR.Management;

namespace WebXR
{
  [System.Serializable]
  [XRConfigurationData("WebXR", "WebXR.Settings")]
  public class WebXRSettings : ScriptableObject
  {
    // Here we'll ask which extra features to try to use
    // Hand, Hit-Test, Planes, etc...
  }
}
