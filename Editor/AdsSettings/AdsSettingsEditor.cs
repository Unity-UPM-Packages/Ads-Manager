using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if USE_ADMOB
using GoogleMobileAds.Editor;
#endif

namespace TheLegends.Base.Ads
{
    [CustomEditor(typeof(AdsSettings))]
    public class AdsSettingsEditor : Editor
    {
        private static AdsSettings instance = null;

        public static AdsSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AdsSettings>(AdsSettings.FileName);
                }

                if (instance != null)
                {
                    Selection.activeObject = instance;
                }
                else
                {
                    Directory.CreateDirectory(AdsSettings.ResDir);

                    instance = CreateInstance<AdsSettings>();

                    string assetPath = Path.Combine(AdsSettings.ResDir, AdsSettings.FileName);
                    string assetPathWithExtension = Path.ChangeExtension(assetPath, AdsSettings.FileExtension);
                    AssetDatabase.CreateAsset(instance, assetPathWithExtension);
                    AssetDatabase.SaveAssets();
                }

                return instance;
            }
        }

        [MenuItem("TripSoft/Ads Settings")]
        public static void OpenInspector()
        {
            if (Instance == null)
            {
                Debug.Log("Creat new Ads Settings");
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.cyan }
            };

            Instance.FlagNetWorks = (AdsNetworks)EditorGUILayout.EnumFlagsField("Use Mediation", Instance.FlagNetWorks);

            // Custom GUI for Primary Network
            var selectedNetworks = new List<AdsNetworks>();
            foreach (AdsNetworks network in Enum.GetValues(typeof(AdsNetworks)))
            {
                if (network == AdsNetworks.None) continue;
                if ((Instance.FlagNetWorks & network) == network)
                {
                    selectedNetworks.Add(network);
                }
            }

            if (selectedNetworks.Count > 0)
            {
                // Ensure the current primary network is valid
                if (!selectedNetworks.Contains(Instance.primaryNetwork))
                {
                    Instance.primaryNetwork = selectedNetworks[0];
                }

                var networkNames = selectedNetworks.Select(n => n.ToString()).ToArray();
                var networkValues = selectedNetworks.Select(n => (int)n).ToArray();

                var selectedIndex = Array.IndexOf(networkValues, (int)Instance.primaryNetwork);
                
                var newSelectedIndex = EditorGUILayout.Popup("Primary Network", selectedIndex, networkNames);

                if (newSelectedIndex != selectedIndex)
                {
                    Instance.primaryNetwork = (AdsNetworks)networkValues[newSelectedIndex];
                }
            }
            else
            {
                Instance.primaryNetwork = AdsNetworks.None;
            }


            if ((Instance.FlagNetWorks & AdsNetworks.Iron) != 0 &&
                !PackagesManagerIntergration.IsSymbolEnabled("USE_IRON"))
            {
                EditorGUILayout.HelpBox("Add Symbols to Player Settings \"USE_IRON\"", MessageType.Warning);
            }

            if ((Instance.FlagNetWorks & AdsNetworks.Max) != 0 &&
                !PackagesManagerIntergration.IsSymbolEnabled("USE_MAX"))
            {
                EditorGUILayout.HelpBox("Add Symbols to Player Settings \"USE_MAX\"", MessageType.Warning);
            }

            if ((Instance.FlagNetWorks & AdsNetworks.Admob) != 0 &&
                !PackagesManagerIntergration.IsSymbolEnabled("USE_ADMOB"))
            {
                EditorGUILayout.HelpBox("Add Symbols to Player Settings \"USE_ADMOB\"\"", MessageType.Warning);
            }

            if (GUILayout.Button("SAVE"))
            {
                Save(Instance);
                PackagesManagerIntergration.SetSymbolEnabled("USE_IRON", Instance.showIRON);
                PackagesManagerIntergration.SetSymbolEnabled("USE_MAX", Instance.showMAX);
                PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB", Instance.showADMOB);
                
                // Also update USE_ADMOB_NATIVE_UNITY based on current state
                bool shouldEnableNativeUnity = Instance.showADMOB && Instance.isUseNativeUnity;
                PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB_NATIVE_UNITY", shouldEnableNativeUnity);
                
                PackagesManagerIntergration.UpdateManifest(Instance.showADMOB, Instance.showMAX);
                
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            Instance.bannerPosition = (BannerPos)EditorGUILayout.EnumPopup(new GUIContent("Banner Position"), Instance.bannerPosition);
            Instance.fixBannerSmallSize = EditorGUILayout.Toggle("Fix Banner Small Size 320x50", Instance.fixBannerSmallSize);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            Instance.autoReLoadMax = EditorGUILayout.IntField("Max auto reload if no ads", Instance.autoReLoadMax);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            Instance.isTest = EditorGUILayout.Toggle("Is Testing", Instance.isTest);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("preloadSettings"), true);
            EditorGUILayout.Separator();

            #region IronSource

#if USE_IRON
            if ((Instance.FlagNetWorks & AdsNetworks.Iron) != 0)
            {
                if (AssetDatabase.IsValidFolder("Assets/LevelPlay/Editor/"))
                {
                    IronSourceMediationSettings ironSourceMediationSettings = Resources.Load<IronSourceMediationSettings>(IronSourceConstants.IRONSOURCE_MEDIATION_SETTING_NAME);
                    if (ironSourceMediationSettings == null)
                    {
                        IronSourceMediationSettings asset = CreateInstance<IronSourceMediationSettings>();
                        Directory.CreateDirectory(IronSourceConstants.IRONSOURCE_RESOURCES_PATH);
                        AssetDatabase.CreateAsset(asset, IronSourceMediationSettings.IRONSOURCE_SETTINGS_ASSET_PATH);
                        ironSourceMediationSettings = asset;
                    }
                    ironSourceMediationSettings.AndroidAppKey = instance.ironAndroidAppKey;
                    ironSourceMediationSettings.IOSAppKey = instance.ironIOSAppKey;
                    ironSourceMediationSettings.AddIronsourceSkadnetworkID = true;
                    ironSourceMediationSettings.DeclareAD_IDPermission = true;
                    ironSourceMediationSettings.EnableIronsourceSDKInitAPI = false;

                    IronSourceMediatedNetworkSettings ironSourceMediatedNetworkSettings = Resources.Load<IronSourceMediatedNetworkSettings>(IronSourceConstants.IRONSOURCE_MEDIATED_NETWORK_SETTING_NAME);
                    if(ironSourceMediatedNetworkSettings == null)
                    {
                        IronSourceMediatedNetworkSettings asset = CreateInstance<IronSourceMediatedNetworkSettings>();
                        Directory.CreateDirectory(IronSourceConstants.IRONSOURCE_RESOURCES_PATH);
                        AssetDatabase.CreateAsset(asset, IronSourceMediatedNetworkSettings.MEDIATION_SETTINGS_ASSET_PATH);
                        ironSourceMediatedNetworkSettings = asset;
                    }


                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("IronSource", titleStyle);
                    Instance.ironAndroidAppKey = EditorGUILayout.TextField("IronSource Android AppKey", Instance.ironAndroidAppKey);
                    Instance.ironIOSAppKey = EditorGUILayout.TextField("IronSource iOS AppKey", Instance.ironIOSAppKey);
                    Instance.isIronTest = EditorGUILayout.Toggle("Is Testing", Instance.isIronTest);

                    Instance.ironEnableAdmob = EditorGUILayout.Toggle("Enable Admob", Instance.ironEnableAdmob);
                    ironSourceMediatedNetworkSettings.EnableAdmob = Instance.ironEnableAdmob;
                    if (Instance.ironEnableAdmob)
                    {
                        Instance.ironAdmobAndroidAppID = EditorGUILayout.TextField("IronSource Admob Android AppID", Instance.ironAdmobAndroidAppID);
                        Instance.ironAdmobIOSAppID = EditorGUILayout.TextField("IronSource IOS AppID", Instance.ironAdmobIOSAppID);
                        ironSourceMediatedNetworkSettings.AdmobAndroidAppId = Instance.ironAdmobAndroidAppID;
                        ironSourceMediatedNetworkSettings.AdmobIOSAppId = Instance.ironAdmobIOSAppID;
                    }
                    else
                    {
                        Instance.ironAdmobAndroidAppID = string.Empty;
                        Instance.ironAdmobIOSAppID = string.Empty;
                        ironSourceMediatedNetworkSettings.AdmobAndroidAppId = string.Empty;
                        ironSourceMediatedNetworkSettings.AdmobIOSAppId = string.Empty;
                    }

                    AssetDatabase.SaveAssetIfDirty(ironSourceMediationSettings);
                }
            }
#endif
            #endregion

            #region MAX

#if USE_MAX
            if ((Instance.FlagNetWorks & AdsNetworks.Max) != 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("MAX AppLovin", titleStyle);
                Instance.isShowMediationDebugger = EditorGUILayout.Toggle("Is Use Mediation Debugger", Instance.isShowMediationDebugger);
                EditorGUILayout.Separator();
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MAX_Android"), new GUIContent("Android Unit IDs"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MAX_iOS"), new GUIContent("iOS Unit IDs"), true);
            }
#endif

            #endregion

            #region ADMOB

#if USE_ADMOB
            if ((Instance.FlagNetWorks & AdsNetworks.Admob) != 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Admob", titleStyle);
                Instance.isShowAdmobNativeValidator = EditorGUILayout.Toggle("Show Admob Validator", Instance.isShowAdmobNativeValidator);
                Instance.isUseNativeUnity = EditorGUILayout.Toggle("Is Use Native Unity", Instance.isUseNativeUnity);
                EditorGUILayout.Separator();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ADMOB_Android"), new GUIContent("Android Unit IDs"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ADMOB_IOS"), new GUIContent("iOS Unit IDs"), true);
            }
#endif

            #endregion

            if (EditorGUI.EndChangeCheck())
            {
                Save((AdsSettings)target);
            }
        }

        private void Save(Object target)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
