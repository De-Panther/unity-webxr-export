using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace WebXR
{
    public class WebXRLoader : XRLoaderHelper
    {
        static readonly List<WebXRSubsystemDescriptor> sampleSubsystemDescriptors = new List<WebXRSubsystemDescriptor>();
        public WebXRSubsystem WebXRSubsystem => GetLoadedSubsystem<WebXRSubsystem>();
        
        public override bool Initialize()
        {
            Debug.Log("Initialize " + nameof(WebXRLoader));
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

        private void OnDestroy()
        {
            WebXRSubsystem.Destroy();
        }
    }
}