using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace Ads.Manager.Editor
{
    /// <summary>
    /// Post-build processor để configure WKWebView compatibility trong Unity iOS builds.
    /// Giải quyết vấn đề video ads bị flash/disappear khi render trong Unity environment.
    /// </summary>
    public static class ConfigureUnityWebViewiOS
    {
        [PostProcessBuildAttribute(996)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            UnityEngine.Debug.Log("🔧 ConfigureUnityWebViewiOS: Configuring Xcode project for Unity WebView compatibility...");

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

#if UNITY_2019_3_OR_NEWER
            string mainTargetGuid = pbxProject.GetUnityMainTargetGuid();
#else
            string mainTargetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif

            // 1. CRITICAL: Thêm linker flag để cho phép WKWebView chạy trong Unity
            // Đây là cờ quan trọng nhất - cho phép WebView được link đúng cách
            pbxProject.AddBuildProperty(mainTargetGuid, "OTHER_LDFLAGS", "-Wl,-U,_WKWebView");
            UnityEngine.Debug.Log("  ✓ Added linker flag: -Wl,-U,_WKWebView");

            // 2. Thêm các framework cần thiết cho WebView và Media
            pbxProject.AddFrameworkToProject(mainTargetGuid, "WebKit.framework", false);
            pbxProject.AddFrameworkToProject(mainTargetGuid, "CoreTelephony.framework", false);
            pbxProject.AddFrameworkToProject(mainTargetGuid, "SystemConfiguration.framework", false);
            UnityEngine.Debug.Log("  ✓ Added frameworks: WebKit, CoreTelephony, SystemConfiguration");

            // Write changes
            pbxProject.WriteToFile(projectPath);

            // 3. Configure Info.plist
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            
            if (!File.Exists(plistPath))
            {
                UnityEngine.Debug.LogError($"❌ ConfigureUnityWebViewiOS: Info.plist not found at {plistPath}");
                return;
            }

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;

            // Disable Metal API validation (reduces rendering overhead)
            if (!rootDict.values.ContainsKey("MetalAPIValidation"))
            {
                rootDict.SetBoolean("MetalAPIValidation", false);
                UnityEngine.Debug.Log("  ✓ Set MetalAPIValidation = NO");
            }

            plist.WriteToFile(plistPath);
            UnityEngine.Debug.Log("✅ ConfigureUnityWebViewiOS: Configuration completed successfully");
        }
    }
}
