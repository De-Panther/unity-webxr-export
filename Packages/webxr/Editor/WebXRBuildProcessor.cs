using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WebXR
{
  public class WebXRBuildProcessor : AssetPostprocessor, IPreprocessBuildWithReport, IPostprocessBuildWithReport
  {
    /// <summary>Override of <see cref="IPreprocessBuildWithReport"/> and <see cref="IPostprocessBuildWithReport"/></summary>
    public int callbackOrder
    {
      get { return 0; }
    }

    void CleanOldSettings()
    {
      UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
      if (preloadedAssets == null)
        return;

      var oldSettings = from s in preloadedAssets
                        where s != null && s.GetType() == typeof(WebXRSettings)
                        select s;

      if (oldSettings != null && oldSettings.Any())
      {
        var assets = preloadedAssets.ToList();
        foreach (var s in oldSettings)
        {
          assets.Remove(s);
        }

        PlayerSettings.SetPreloadedAssets(assets.ToArray());
      }
    }

    /// <summary>Override of <see cref="IPreprocessBuildWithReport"/></summary>
    /// <param name="report">Build report.</param>
    public void OnPreprocessBuild(BuildReport report)
    {
      // Always remember to cleanup preloaded assets after build to make sure we don't
      // dirty later builds with assets that may not be needed or are out of date.
      CleanOldSettings();

      if (report.summary.platform != BuildTarget.WebGL)
      {
        return;
      }

#if UNITY_2020_3 || UNITY_2021_1
            if (!PlayerSettings.WebGL.emscriptenArgs.Contains("-std="))
            {
              PlayerSettings.WebGL.emscriptenArgs += " -std=c++11";
            }
#endif

      WebXRSettings settings = WebXRSettings.GetSettings();
      if (settings == null)
        return;

      UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();

      if (!preloadedAssets.Contains(settings))
      {
        var assets = preloadedAssets.ToList();
        assets.Add(settings);
        PlayerSettings.SetPreloadedAssets(assets.ToArray());
      }
    }

    /// <summary>Override of <see cref="IPostprocessBuildWithReport"/></summary>
    /// <param name="report">Build report.</param>
    public void OnPostprocessBuild(BuildReport report)
    {
      // Always remember to cleanup preloaded assets after build to make sure we don't
      // dirty later builds with assets that may not be needed or are out of date.
      CleanOldSettings();
    }

#if UNITY_2023_2_OR_NEWER || UNITY_2022_3 || UNITY_2021_3
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
    {
#if !HAS_URP
      bool needsURP = true;
      WebXRSettings settings = WebXRSettings.GetSettings();
      if (settings != null && settings.DisableXRDisplaySubsystem)
      {
        needsURP = false;
      }
      if (needsURP)
      {
        Debug.LogWarning(@"WebXR Export requires Universal Render Pipeline,
using Built-in Render Pipeline might cause issues.");
      }
#endif
      if (PlayerSettings.colorSpace != ColorSpace.Gamma)
      {
        Debug.LogWarning(@"WebXR Export requires Gamma Color Space,
using Linear Color Space might cause issues.");
      }
    }
  }
}
