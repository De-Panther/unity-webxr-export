using System.Collections.Generic;
using System.Runtime.InteropServices;
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

#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void RegisterWebXRPlugin();
#endif

        public override bool Initialize()
        {
            Debug.Log("Initialize " + nameof(WebXRLoader));
            #if !UNITY_EDITOR && UNITY_WEBGL
            RegisterWebXRPlugin();
            Debug.Log("Called RegisterWebXRPlugin");
            #endif
            CreateSubsystem<WebXRSubsystemDescriptor, WebXRSubsystem>(sampleSubsystemDescriptors, typeof(WebXRSubsystem).FullName);
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(displaySubsystemDescriptors, "WebXR VR Display");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(inputSubsystemDescriptors, "WebXR Inputs");
            return WebXRSubsystem != null;
        }
        

        public override bool Start()
        {
            WebXRSubsystem.Start();
            Debug.Log("Start XRDisplaySubsystem");
            XRDisplaySubsystem.Start();
            Debug.Log("Did it work?!? XRDisplaySubsystem");
            XRInputSubsystem.Start();
            return true;
        }

        public override bool Stop()
        {
            WebXRSubsystem.Stop();
            XRDisplaySubsystem.Stop();
            XRInputSubsystem.Stop();
            return base.Stop();
        }

        public override bool Deinitialize()
        {
            WebXRSubsystem.Destroy();
            XRDisplaySubsystem.Destroy();
            XRInputSubsystem.Destroy();
            return base.Deinitialize();
        }
    }
}