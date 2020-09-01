using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.XR.Management.Metadata;

namespace WebXR.Editor
{
    internal class XRPackage : IXRPackage
    {
        private class XRPluginSampleLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        private class XRPluginSampleMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; } 
        }

        static readonly IXRPackageMetadata s_Metadata = new XRPluginSampleMetadata()
        {
            packageName = "WebXR Export",
            packageId = "com.de-panther.webxr",
            settingsType = typeof(WebXRPluginLoader).FullName,
            loaderMetadata = new List<IXRLoaderMetadata>() 
            {
                new XRPluginSampleLoaderMetadata() 
                {
                    loaderName = "Web XR Plugin",
                    loaderType = typeof(WebXRPluginLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup>() 
                    {
                        BuildTargetGroup.WebGL
                    }
                },
            }
        };

        public IXRPackageMetadata metadata => s_Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            return true;
        }
    }
}