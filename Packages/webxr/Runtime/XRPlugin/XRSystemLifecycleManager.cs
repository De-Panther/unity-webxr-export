﻿using System;
using UnityEngine;
using UnityEngine.XR.Management;

#if UNITY_XR_MANAGEMENT_4_3_1_OR_NEWER
using UnityEngine.SubsystemsImplementation;
#endif

namespace WebXR
{
#if UNITY_XR_MANAGEMENT_4_3_1_OR_NEWER
    public class SubsystemLifecycleManager<TSubsystem, TSubsystemDescriptor,TProvider> : MonoBehaviour
        where TSubsystem : SubsystemWithProvider<TSubsystem, TSubsystemDescriptor,TProvider>, new()
        where TSubsystemDescriptor : SubsystemDescriptorWithProvider
        where TProvider : SubsystemProvider<TSubsystem>
#else
    public class SubsystemLifecycleManager<TSubsystem, TSubsystemDescriptor> : MonoBehaviour
        where TSubsystem : Subsystem<TSubsystemDescriptor>
        where TSubsystemDescriptor : SubsystemDescriptor<TSubsystem>
#endif
    {
        /// <summary>
        /// Get the <c>TSubsystem</c> whose lifetime this component manages.
        /// </summary>
        public TSubsystem subsystem { get; private set; }

        public bool isSubsystemAvailable
        {
          get
          {
            return subsystem != null;
          }
        }

        protected virtual void Awake()
        {
            EnsureSubsystem();
        }

        protected virtual void OnEnable()
        {
            EnsureSubsystem();
        }

        private void EnsureSubsystem()
        {
            if (subsystem != null) return;
            subsystem = GetActiveSubsystemInstance();
        }

        /// <summary>
        /// Returns the active <c>TSubsystem</c> instance if present, otherwise returns null.
        /// </summary>
        /// <returns>The active subsystem instance, or `null` if there isn't one.</returns>
        protected TSubsystem GetActiveSubsystemInstance()
        {
            TSubsystem activeSubsystem = null;

            // Query the currently active loader for the created subsystem, if one exists.
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                XRLoader loader = XRGeneralSettings.Instance.Manager.activeLoader;
                if (loader != null)
                    activeSubsystem = loader.GetLoadedSubsystem<TSubsystem>();
            }

            if (activeSubsystem == null)
                Debug.LogWarningFormat($"No active {typeof(TSubsystem).FullName} is available. Please ensure that a " +
                                       "valid loader configuration exists in the XR project settings.");

            return activeSubsystem;
        }
    }
}