using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace WebXR.Interactions.Samples.XRInteractionToolkit.Editor
{
    /// <summary>
    /// Unity Editor class which registers Project Validation rules for the WebXR + XR Interaction Toolkit sample,
    /// checking that other required samples and packages are installed.
    /// </summary>
    static class WebXRInteractionToolkitSampleProjectValidation
    {
        const string k_SampleDisplayName = "WebXR + XR Interaction Toolkit sample";
        const string k_Category = "WebXR";
        const string k_HandsInteractionDemoSampleName = "Hands Interaction Demo";
        const string k_ProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";
        const string k_HandsPackageName = "com.unity.xr.hands";
        const string k_XRIPackageName = "com.unity.xr.interaction.toolkit";
        static readonly PackageVersion s_MinimumHandsPackageVersion = new PackageVersion("1.2.1");
        static readonly PackageVersion s_RecommendedHandsPackageVersion = new PackageVersion("1.3.0");
        static readonly PackageVersion s_MinimumXRIPackageVersion = new PackageVersion("2.5.2");

        static readonly BuildTargetGroup[] s_BuildTargetGroups =
            ((BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup))).Distinct().ToArray();

        static readonly List<BuildValidationRule> s_BuildValidationRules = new List<BuildValidationRule>
        {
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_HandsPackageAddRequest == null || s_HandsPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] XR Hands ({k_HandsPackageName}) package must be installed or updated to use this sample.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.GetPackageVersion(k_HandsPackageName) >= s_MinimumHandsPackageVersion,
                FixIt = () =>
                {
                    if (s_HandsPackageAddRequest == null || s_HandsPackageAddRequest.IsCompleted)
                        InstallOrUpdateHands();
                },
                FixItAutomatic = true,
                Error = true,
            },
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_HandsPackageAddRequest == null || s_HandsPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] XR Hands ({k_HandsPackageName}) package must be at version {s_RecommendedHandsPackageVersion} or higher to use the latest sample features.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.GetPackageVersion(k_HandsPackageName) >= s_RecommendedHandsPackageVersion,
                FixIt = () =>
                {
                    if (s_HandsPackageAddRequest == null || s_HandsPackageAddRequest.IsCompleted)
                        InstallOrUpdateHands();
                },
                FixItAutomatic = true,
                Error = false,
            },
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_XRIPackageAddRequest == null || s_XRIPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] XR Interaction Toolkit ({k_XRIPackageName}) package must be installed or updated to use this sample.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.GetPackageVersion(k_XRIPackageName) >= s_MinimumXRIPackageVersion,
                FixIt = () =>
                {
                    if (s_XRIPackageAddRequest == null || s_XRIPackageAddRequest.IsCompleted)
                        InstallOrUpdateXRI();
                },
                FixItAutomatic = true,
                Error = true,
            },
            new BuildValidationRule
            {
                Message = $"[{k_SampleDisplayName}] {k_HandsInteractionDemoSampleName} sample from XR Interaction Toolkit ({k_XRIPackageName}) package must be imported or updated to use this sample.",
                Category = k_Category,
                CheckPredicate = () => TryFindSample(k_XRIPackageName, string.Empty, k_HandsInteractionDemoSampleName, out var sample) && sample.isImported,
                FixIt = () =>
                {
                    if (TryFindSample(k_XRIPackageName, string.Empty, k_HandsInteractionDemoSampleName, out var sample))
                    {
                        sample.Import(Sample.ImportOptions.OverridePreviousImports);
                    }
                },
                FixItAutomatic = true,
                Error = true,
            },
        };

        static AddRequest s_HandsPackageAddRequest;
        static AddRequest s_XRIPackageAddRequest;

        [InitializeOnLoadMethod]
        static void RegisterProjectValidationRules()
        {
            foreach (var buildTargetGroup in s_BuildTargetGroups)
            {
                BuildValidator.AddRules(buildTargetGroup, s_BuildValidationRules);
            }

            // Delay evaluating conditions for issues to give time for Package Manager and UPM cache to fully initialize.
            EditorApplication.delayCall += ShowWindowIfIssuesExist;
        }

        static void ShowWindowIfIssuesExist()
        {
            foreach (var validation in s_BuildValidationRules)
            {
                if (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke())
                {
                    ShowWindow();
                    return;
                }
            }
        }

        internal static void ShowWindow()
        {
            // Delay opening the window since sometimes other settings in the player settings provider redirect to the
            // project validation window causing serialized objects to be nullified.
            EditorApplication.delayCall += () =>
            {
                SettingsService.OpenProjectSettings(k_ProjectValidationSettingsPath);
            };
        }

        static bool TryFindSample(string packageName, string packageVersion, string sampleDisplayName, out Sample sample)
        {
            sample = default;

            if (!PackageVersionUtility.IsPackageInstalled(packageName))
                return false;

            IEnumerable<Sample> packageSamples;
            try
            {
                packageSamples = Sample.FindByPackage(packageName, packageVersion);
            }
            catch (Exception e)
            {
                Debug.LogError($"Couldn't find samples of the {ToString(packageName, packageVersion)} package; aborting project validation rule. Exception: {e}");
                return false;
            }

            if (packageSamples == null)
            {
                Debug.LogWarning($"Couldn't find samples of the {ToString(packageName, packageVersion)} package; aborting project validation rule.");
                return false;
            }

            foreach (var packageSample in packageSamples)
            {
                if (packageSample.displayName == sampleDisplayName)
                {
                    sample = packageSample;
                    return true;
                }
            }

            Debug.LogWarning($"Couldn't find {sampleDisplayName} sample in the {ToString(packageName, packageVersion)} package; aborting project validation rule.");
            return false;
        }

        static string ToString(string packageName, string packageVersion)
        {
            return string.IsNullOrEmpty(packageVersion) ? packageName : $"{packageName}@{packageVersion}";
        }

        static void InstallOrUpdateHands()
        {
            // Set a 3-second timeout for request to avoid editor lockup
            var currentTime = DateTime.Now;
            var endTime = currentTime + TimeSpan.FromSeconds(3);

            var request = Client.Search(k_HandsPackageName);
            if (request.Status == StatusCode.InProgress)
            {
                Debug.Log($"Searching for ({k_HandsPackageName}) in Unity Package Registry.");
                while (request.Status == StatusCode.InProgress && currentTime < endTime)
                    currentTime = DateTime.Now;
            }

            var addRequest = k_HandsPackageName;
            if (request.Status == StatusCode.Success && request.Result.Length > 0)
            {
                var versions = request.Result[0].versions;
                var verifiedVersion = new PackageVersion(versions.recommended);
                var latestCompatible = new PackageVersion(versions.latestCompatible);
                if (verifiedVersion < s_RecommendedHandsPackageVersion && s_RecommendedHandsPackageVersion <= latestCompatible)
                    addRequest = $"{k_HandsPackageName}@{s_RecommendedHandsPackageVersion}";
            }

            s_HandsPackageAddRequest = Client.Add(addRequest);
            if (s_HandsPackageAddRequest.Error != null)
            {
                Debug.LogError($"Package installation error: {s_HandsPackageAddRequest.Error}: {s_HandsPackageAddRequest.Error.message}");
            }
        }

        static void InstallOrUpdateXRI()
        {
            // Set a 3-second timeout for request to avoid editor lockup
            var currentTime = DateTime.Now;
            var endTime = currentTime + TimeSpan.FromSeconds(3);

            var request = Client.Search(k_XRIPackageName);
            if (request.Status == StatusCode.InProgress)
            {
                Debug.Log($"Searching for ({k_XRIPackageName}) in Unity Package Registry.");
                while (request.Status == StatusCode.InProgress && currentTime < endTime)
                    currentTime = DateTime.Now;
            }

            var addRequest = k_XRIPackageName;
            if (request.Status == StatusCode.Success && request.Result.Length > 0)
            {
                var versions = request.Result[0].versions;
                var verifiedVersion = new PackageVersion(versions.recommended);
                var latestCompatible = new PackageVersion(versions.latestCompatible);
                if (verifiedVersion < s_MinimumXRIPackageVersion && s_MinimumXRIPackageVersion <= latestCompatible)
                    addRequest = $"{k_XRIPackageName}@{s_MinimumXRIPackageVersion}";
            }

            s_XRIPackageAddRequest = Client.Add(addRequest);
            if (s_XRIPackageAddRequest.Error != null)
            {
                Debug.LogError($"Package installation error: {s_XRIPackageAddRequest.Error}: {s_XRIPackageAddRequest.Error.message}");
            }
        }
    }
}
