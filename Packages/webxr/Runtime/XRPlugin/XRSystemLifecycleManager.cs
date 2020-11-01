using System;
using UnityEngine;
using UnityEngine.XR.Management;

namespace WebXR
{
    public class SubsystemLifecycleManager<TSubsystem, TSubsystemDescriptor> : MonoBehaviour
        where TSubsystem : Subsystem<TSubsystemDescriptor>
        where TSubsystemDescriptor : SubsystemDescriptor<TSubsystem>
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