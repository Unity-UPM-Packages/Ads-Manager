using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Manages AdMob native plugin (.aar file) based on USE_ADMOB scripting symbol
    /// Automatically enables/disables admob_native_unity-release.aar when USE_ADMOB symbol changes
    /// </summary>
    public class AdmobPluginManager
    {
        private const string ADMOB_AAR_FILE = "admob_native_unity-release.aar";

        /// <summary>
        /// Updates AdMob plugin state based on USE_ADMOB symbol
        /// Call this after changing USE_ADMOB symbol
        /// </summary>
        public static void UpdateAdmobPlugin()
        {
            bool useAdmob = PackagesManagerIntergration.IsSymbolEnabled("USE_ADMOB");
            
            Debug.Log($"[AdmobPluginManager] Updating AdMob plugin - USE_ADMOB enabled: {useAdmob}");
            
            try
            {
                // Get project root path
                string dataPath = Application.dataPath;
                string projectRoot = Path.GetDirectoryName(dataPath);
                
                // Construct package path
                string packagePath = Path.Combine(projectRoot, "Packages", "com.thelegends.ads.manager", "Runtime", "Plugins", "Android");
                string aarPath = Path.Combine(packagePath, ADMOB_AAR_FILE);
                
                // Check if AAR file exists
                if (!File.Exists(aarPath))
                {
                    Debug.LogWarning($"[AdmobPluginManager] AdMob AAR file not found at: {aarPath}");
                    return;
                }
                
                // Convert to Unity asset path for AssetImporter
                string assetPath = aarPath.Substring(projectRoot.Length + 1).Replace('\\', '/');
                
                // Get plugin importer
                PluginImporter importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[AdmobPluginManager] Could not get PluginImporter for: {assetPath}");
                    return;
                }
                
                // Check if plugin state needs to be updated
                bool currentState = importer.GetCompatibleWithPlatform(BuildTarget.Android);
                if (currentState != useAdmob)
                {
                    // Update plugin state
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, useAdmob);
                    
                    // Force reimport to apply changes
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    
                    Debug.Log($"[AdmobPluginManager] {(useAdmob ? "Enabled" : "Disabled")} AdMob plugin: {assetPath}");
                }
                else
                {
                    Debug.Log($"[AdmobPluginManager] AdMob plugin already {(useAdmob ? "enabled" : "disabled")}: {assetPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AdmobPluginManager] Failed to update AdMob plugin: {e.Message}");
            }
        }

        /// <summary>
        /// Manual menu item to force update AdMob plugin state
        /// </summary>
        [MenuItem("Ads Manager/Update AdMob Plugin")]
        public static void ForceUpdateAdmobPlugin()
        {
            UpdateAdmobPlugin();
            Debug.Log("[AdmobPluginManager] Manually triggered AdMob plugin update");
        }

    }
}