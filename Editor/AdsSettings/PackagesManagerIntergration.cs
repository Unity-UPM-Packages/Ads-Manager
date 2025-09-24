using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace TheLegends.Base.Ads
{
    [ExecuteInEditMode]
    public class PackagesManagerIntergration : MonoBehaviour
    {
        protected static BuildTargetGroup[] targetGroups = new BuildTargetGroup[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };

        public static List<string> GetDefinesList(BuildTargetGroup group)
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }

        public static bool IsSymbolEnabled(string defineName)
        {
            bool isAndroidEnabled = false;
            bool isIOSEnabled = false;

            foreach (var group in targetGroups)
            {
                var defines = GetDefinesList(group);
                if (defines.Contains(defineName))
                {
                    switch (group)
                    {
                        case BuildTargetGroup.Android:
                            isAndroidEnabled = true;
                            break;
                        case BuildTargetGroup.iOS:
                            isIOSEnabled = true;
                            break;
                    }
                }
            }

            return isAndroidEnabled && isIOSEnabled;
        }

        public static void SetSymbolEnabled(string defineName, bool enable)
        {
            // Special handling for USE_ADMOB - when disabled, also disable USE_ADMOB_NATIVE_UNITY
            if (defineName == "USE_ADMOB" && !enable)
            {
                // Disable USE_ADMOB_NATIVE_UNITY when USE_ADMOB is disabled
                SetSymbolEnabledInternal("USE_ADMOB_NATIVE_UNITY", false);
                
                // Reset isUseNativeUnity flag in AdsSettings
                var adsSettings = AdsSettings.Instance;
                if (adsSettings != null)
                {
                    adsSettings.isUseNativeUnity = false;
                    EditorUtility.SetDirty(adsSettings);
                }
            }

            SetSymbolEnabledInternal(defineName, enable);
        }

        private static void SetSymbolEnabledInternal(string defineName, bool enable)
        {
            bool updated = false;

            foreach (var group in targetGroups)
            {
                var defines = GetDefinesList(group);
                if (enable)
                {
                    if (!defines.Contains(defineName))
                    {
                        defines.Add(defineName);
                        updated = true;
                    }
                }
                else
                {
                    if (defines.Contains(defineName))
                    {
                        while (defines.Contains(defineName))
                        {
                            defines.Remove(defineName);
                        }

                        updated = true;
                    }
                }

                if (updated)
                {
                    string definesString = string.Join(";", defines.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
                }
            }
        }

        /// <summary>
        /// Gets the project root path that works both when this package is in development
        /// and when it's imported into other projects via UPM
        /// </summary>
        private static string GetProjectRootPath()
        {
            // Method 1: Try using Application.dataPath (works in most cases)
            string dataPath = Application.dataPath;
            string projectRoot = Path.GetDirectoryName(dataPath); // Go up from Assets/ to project root
            
            // Verify this is actually the project root by checking for Packages/manifest.json
            string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                return projectRoot;
            }
            
            // Method 2: If that doesn't work, try using the current directory
            // This can happen in some edge cases or when running tests
            string currentDir = Directory.GetCurrentDirectory();
            manifestPath = Path.Combine(currentDir, "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                return currentDir;
            }
            
            // Method 3: Last resort - search upward from Application.dataPath
            DirectoryInfo dir = new DirectoryInfo(dataPath);
            while (dir != null && dir.Parent != null)
            {
                manifestPath = Path.Combine(dir.FullName, "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            
            // If all methods fail, return the original method as fallback
            Debug.LogWarning("Could not locate project root with manifest.json, using fallback method");
            return Path.GetDirectoryName(Application.dataPath);
        }

        /// <summary>
        /// Safely removes a package from manifest while handling commas correctly
        /// </summary>
        private static string RemovePackageFromManifest(string manifestContent, string packageName)
        {
            // Escape dots in package name for regex
            string escapedPackageName = packageName.Replace(".", "\\.");
            
            // Try different patterns to handle various comma scenarios
            
            // Pattern 1: Package with trailing comma (most common case)
            // "package": "version",
            string pattern1 = $"\\s*\"{escapedPackageName}\"\\s*:\\s*\"[^\"]*\"\\s*,";
            if (Regex.IsMatch(manifestContent, pattern1))
            {
                return Regex.Replace(manifestContent, pattern1, "");
            }
            
            // Pattern 2: Package without trailing comma but with leading comma from previous line
            // ,\n    "package": "version"
            string pattern2 = $",\\s*\"{escapedPackageName}\"\\s*:\\s*\"[^\"]*\"";
            if (Regex.IsMatch(manifestContent, pattern2))
            {
                return Regex.Replace(manifestContent, pattern2, "");
            }
            
            // Pattern 3: Package is the only one (no commas)
            // "package": "version"
            string pattern3 = $"\\s*\"{escapedPackageName}\"\\s*:\\s*\"[^\"]*\"\\s*";
            return Regex.Replace(manifestContent, pattern3, "");
        }

        /// <summary>
        /// Cleans up JSON comma issues after package removal
        /// </summary>
        private static string CleanupJsonCommas(string manifestContent)
        {
            // Remove double commas
            manifestContent = Regex.Replace(manifestContent, ",\\s*,", ",");
            
            // Remove trailing comma before closing brace (handle multiple whitespace/newlines)
            manifestContent = Regex.Replace(manifestContent, ",\\s*\\}", "\n  }");
            
            // Handle comma after opening brace (in case of empty or leading package removal)
            manifestContent = Regex.Replace(manifestContent, "\\{\\s*,", "{\n");
            
            // Handle standalone commas on their own lines
            manifestContent = Regex.Replace(manifestContent, "\\n\\s*,\\s*\\n", "\n");
            
            return manifestContent;
        }

        public static void UpdateManifest(bool enableAdmob, bool enableMax)
        {
            // Get the project root directory (where manifest.json should be)
            string projectRoot = GetProjectRootPath();
            string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");
            
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("Manifest.json not found at: " + manifestPath);
                return;
            }

            string originalManifestContent = File.ReadAllText(manifestPath);
            bool hasChanges = false;
            
            // Lock assembly reloading to prevent partial updates
            EditorApplication.LockReloadAssemblies();
            
            try
            {
                string manifestContent = originalManifestContent;
                
                // Default versions - only used if package is not already present
                const string defaultAdmobVersion = "9.6.0";
                const string defaultMaxVersion = "8.1.0";
                
                // Handle ADMOB dependency
                if (enableAdmob)
                {
                    if (manifestContent.Contains("\"com.google.ads.mobile\""))
                    {
                        // Package already exists, don't change the version
                        Debug.Log("com.google.ads.mobile already exists in manifest, keeping current version");
                    }
                    else
                    {
                        // Add new ADMOB dependency with default version
                        manifestContent = Regex.Replace(manifestContent,
                            "(\"dependencies\"\\s*:\\s*\\{)",
                            $"$1\n    \"com.google.ads.mobile\": \"{defaultAdmobVersion}\",");
                        Debug.Log($"Added com.google.ads.mobile: {defaultAdmobVersion} to manifest");
                        hasChanges = true;
                    }
                }
                else
                {
                    // Remove ADMOB dependency with improved comma handling
                    if (manifestContent.Contains("\"com.google.ads.mobile\""))
                    {
                        string newManifestContent = RemovePackageFromManifest(manifestContent, "com.google.ads.mobile");
                        if (newManifestContent != manifestContent)
                        {
                            manifestContent = newManifestContent;
                            Debug.Log("Removed com.google.ads.mobile from manifest");
                            hasChanges = true;
                        }
                    }
                }

                // Handle MAX dependency
                if (enableMax)
                {
                    if (manifestContent.Contains("\"com.applovin.mediation.ads\""))
                    {
                        // Package already exists, don't change the version
                        Debug.Log("com.applovin.mediation.ads already exists in manifest, keeping current version");
                    }
                    else
                    {
                        // Add new MAX dependency with default version
                        manifestContent = Regex.Replace(manifestContent,
                            "(\"dependencies\"\\s*:\\s*\\{)",
                            $"$1\n    \"com.applovin.mediation.ads\": \"{defaultMaxVersion}\",");
                        Debug.Log($"Added com.applovin.mediation.ads: {defaultMaxVersion} to manifest");
                        hasChanges = true;
                    }
                }
                else
                {
                    // Remove MAX dependency with improved comma handling
                    if (manifestContent.Contains("\"com.applovin.mediation.ads\""))
                    {
                        string newManifestContent = RemovePackageFromManifest(manifestContent, "com.applovin.mediation.ads");
                        if (newManifestContent != manifestContent)
                        {
                            manifestContent = newManifestContent;
                            Debug.Log("Removed com.applovin.mediation.ads from manifest");
                            hasChanges = true;
                        }
                    }
                }

                // Final cleanup of any remaining comma issues
                manifestContent = CleanupJsonCommas(manifestContent);

                // Only write and refresh if there were actual changes
                if (hasChanges || manifestContent != originalManifestContent)
                {
                    // Write back to file
                    File.WriteAllText(manifestPath, manifestContent);
                    Debug.Log("Manifest.json updated successfully");
                }
                else
                {
                    Debug.Log("No changes needed in manifest.json - packages already match the desired state");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to update manifest.json: " + e.Message);
                hasChanges = false; // Don't refresh if there was an error
            }
            finally
            {
                // Always unlock assembly reloading
                EditorApplication.UnlockReloadAssemblies();
                
                // Only force refresh if there were actual changes
                if (hasChanges)
                {
                    ForcePackageManagerRefresh();
                }
            }
        }

        /// <summary>
        /// Forces Unity to refresh Package Manager and reload packages
        /// </summary>
        private static void ForcePackageManagerRefresh()
        {
            try
            {
                Debug.Log("Starting Package Manager refresh due to manifest changes...");
                
                // Method 1: Refresh Asset Database (basic refresh)
                AssetDatabase.Refresh();
                
                // Method 2: Use Package Manager Client to force refresh
                Client.Resolve();
                
                // Method 3: Force reimport of packages
                AssetDatabase.ImportAsset("Packages/manifest.json");
                
                Debug.Log("Immediate Package Manager refresh completed");
                
                // Method 4: Single delayed refresh to ensure packages are loaded
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                        Client.Resolve();
                        
                        Debug.Log("Delayed Package Manager refresh completed");
                        
                        // Final delayed refresh for editor windows
                        EditorApplication.delayCall += () =>
                        {
                            try
                            {
                                EditorApplication.RepaintProjectWindow();
                                Debug.Log("Final editor refresh completed - packages should be fully loaded");
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning("Final editor refresh failed (non-critical): " + ex.Message);
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Delayed refresh failed (non-critical): " + ex.Message);
                    }
                };
            }
            catch (Exception e)
            {
                Debug.LogWarning("Package Manager refresh failed: " + e.Message);
            }
        }
    }
}

