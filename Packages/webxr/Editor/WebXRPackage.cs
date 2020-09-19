using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.XR.Management.Metadata;

namespace WebXR.Editor
{
    internal class WebXRPackage : IXRPackage
    {
        private class WebXRLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        private class WebXRPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; } 
        }

        static readonly IXRPackageMetadata s_Metadata = new WebXRPackageMetadata()
        {
            packageName = "WebXR Export",
            packageId = "com.de-panther.webxr",
            settingsType = typeof(WebXRSettings).FullName,
            loaderMetadata = new List<IXRLoaderMetadata>() 
            {
                new WebXRLoaderMetadata() 
                {
                    loaderName = "WebXR Export",
                    loaderType = typeof(WebXRLoader).FullName,
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
            var settings = obj as WebXRSettings;
            if (settings != null)
            {
                return true;
            }
            return false;
        }
    }
}