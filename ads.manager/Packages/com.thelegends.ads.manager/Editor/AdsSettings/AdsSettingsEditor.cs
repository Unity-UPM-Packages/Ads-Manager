using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

            Instance.FlagNetWorks = (AdsNetworks)EditorGUILayout.EnumFlagsField("Use Mediation", Instance.FlagNetWorks);
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

            // if (GUILayout.Button("INSTALL DEFINE SYMBOLS"))
            // {
            //     PackagesManagerIntergration.SetSymbolEnabled("USE_IRON", Instance.showIRON);
            //     PackagesManagerIntergration.SetSymbolEnabled("USE_MAX", Instance.showMAX);
            //     PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB", Instance.showADMOB);
            // }

            if (GUILayout.Button("SAVE"))
            {
                Save(Instance);
                PackagesManagerIntergration.SetSymbolEnabled("USE_IRON", Instance.showIRON);
                PackagesManagerIntergration.SetSymbolEnabled("USE_MAX", Instance.showMAX);
                PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB", Instance.showADMOB);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            Instance.bannerPosition = (BannerPos)EditorGUILayout.EnumPopup(new GUIContent("Banner Position"), Instance.bannerPosition);
            Instance.fixBannerSmallSize = EditorGUILayout.Toggle("Fix Banner Small Size 320x50", Instance.fixBannerSmallSize);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            Instance.autoReLoadMax = EditorGUILayout.IntField("Max auto reload if no ads", Instance.autoReLoadMax);

            #region IronSource

#if USE_IRON
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
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("IronSource", EditorStyles.boldLabel);
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
#endif
            #endregion

            #region MAX

#if USE_MAX
            if (AssetDatabase.IsValidFolder("Assets/MaxSDK/AppLovin"))
            {
                AppLovinSettings appLovinSettings = Resources.Load<AppLovinSettings>("AppLovinSettings");
                if (appLovinSettings == null)
                {
                    AppLovinSettings asset = CreateInstance<AppLovinSettings>();
                    Directory.CreateDirectory("Assets/MaxSDK/Resources");
                    AssetDatabase.CreateAsset(asset, "Assets/MaxSDK/Resources/AppLovinSettings.asset");
                    appLovinSettings = asset;
                }
                appLovinSettings.SdkKey = Instance.maxSdkKey;
                appLovinSettings.SetAttributionReportEndpoint = true;

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("MAX AppLovin", EditorStyles.boldLabel);
                Instance.maxSdkKey = EditorGUILayout.TextField("MAX SDK Key", Instance.maxSdkKey);
                Instance.isMaxTest = EditorGUILayout.Toggle("Is Testing", Instance.isMaxTest);

                Instance.maxEnableAdmob = EditorGUILayout.Toggle("Enable Admob", Instance.maxEnableAdmob);
                if (Instance.maxEnableAdmob)
                {
                    Instance.maxAdmobAndroidAppID = EditorGUILayout.TextField("MAX Admob Android AppID", Instance.maxAdmobAndroidAppID);
                    Instance.maxAdmobIOSAppID = EditorGUILayout.TextField("MAX Admob IOS AppID", Instance.maxAdmobIOSAppID);
                    appLovinSettings.AdMobAndroidAppId = Instance.maxAdmobAndroidAppID;
                    appLovinSettings.AdMobIosAppId = Instance.maxAdmobIOSAppID;
                }
                else
                {
                    Instance.maxAdmobAndroidAppID = string.Empty;
                    Instance.maxAdmobIOSAppID = string.Empty;
                    appLovinSettings.AdMobAndroidAppId = string.Empty;
                    appLovinSettings.AdMobIosAppId = string.Empty;
                }

                AssetDatabase.SaveAssetIfDirty(appLovinSettings);

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("ANDROID AD_UNIT_ID");
                var MAX_Android = serializedObject.FindProperty("MAX_Android");
                EditorGUILayout.PropertyField(MAX_Android.FindPropertyRelative("bannerIds"), true);
                EditorGUILayout.PropertyField(MAX_Android.FindPropertyRelative("interIds"), true);
                EditorGUILayout.PropertyField(MAX_Android.FindPropertyRelative("rewardIds"), true);
                EditorGUILayout.PropertyField(MAX_Android.FindPropertyRelative("mrecIds"), true);
                EditorGUILayout.PropertyField(MAX_Android.FindPropertyRelative("appOpenIds"), true);

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("IOS AD_UNIT_ID");
                var MAX_IOS = serializedObject.FindProperty("MAX_iOS");
                EditorGUILayout.PropertyField(MAX_IOS.FindPropertyRelative("bannerIds"), true);
                EditorGUILayout.PropertyField(MAX_IOS.FindPropertyRelative("interIds"), true);
                EditorGUILayout.PropertyField(MAX_IOS.FindPropertyRelative("rewardIds"), true);
                EditorGUILayout.PropertyField(MAX_IOS.FindPropertyRelative("mrecIds"), true);
                EditorGUILayout.PropertyField(MAX_IOS.FindPropertyRelative("appOpenIds"), true);

            }
#endif

            #endregion

            #region ADMOB

#if USE_ADMOB

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Admob", EditorStyles.boldLabel);
            Instance.isAdmobTest = EditorGUILayout.Toggle("Is Testing", Instance.isAdmobTest);
            Instance.isShowAdmobNativeValidator = EditorGUILayout.Toggle("Show Admob Validator", Instance.isShowAdmobNativeValidator);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("ANDROID AD_UNIT_ID");
            var ADMOB_Android = serializedObject.FindProperty("ADMOB_Android");
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("bannerIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("interIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("rewardIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("mrecIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("interOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("mrecOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("appOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeOverlayIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativePlatformIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeBannerIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeInterIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeRewardIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeMrecIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeAppOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeInterOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeMrecOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_Android.FindPropertyRelative("nativeVideoIds"), true);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("IOS AD_UNIT_ID");
            var ADMOB_IOS = serializedObject.FindProperty("ADMOB_IOS");
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("bannerIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("interIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("rewardIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("mrecIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("interOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("mrecOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("appOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeOverlayIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativePlatformIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeBannerIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeInterIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeRewardIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeMrecIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeAppOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeInterOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeMrecOpenIds"), true);
            EditorGUILayout.PropertyField(ADMOB_IOS.FindPropertyRelative("nativeVideoIds"), true);
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
