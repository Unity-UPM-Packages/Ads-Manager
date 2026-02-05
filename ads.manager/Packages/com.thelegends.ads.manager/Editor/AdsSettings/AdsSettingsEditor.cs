using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor.PackageManager;
using UnityEngine.Networking;

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

            Instance.FlagMediations = (AdsMediation)EditorGUILayout.EnumFlagsField("Use Mediation", Instance.FlagMediations);

            var selectedMediations = new List<AdsMediation>();
            foreach (AdsMediation mediation in Enum.GetValues(typeof(AdsMediation)))
            {
                if (mediation == AdsMediation.None) continue;
                if ((Instance.FlagMediations & mediation) == mediation)
                {
                    selectedMediations.Add(mediation);
                }
            }

            if (selectedMediations.Count > 0)
            {
                if (!selectedMediations.Contains(Instance.primaryMediation))
                {
                    Instance.primaryMediation = selectedMediations[0];
                }

                var mediationNames = selectedMediations.Select(n => n.ToString()).ToArray();
                var mediationValues = selectedMediations.Select(n => (int)n).ToArray();

                var selectedIndex = Array.IndexOf(mediationValues, (int)Instance.primaryMediation);
                
                var newSelectedIndex = EditorGUILayout.Popup("Primary Mediation", selectedIndex, mediationNames);

                if (newSelectedIndex != selectedIndex)
                {
                    Instance.primaryMediation = (AdsMediation)mediationValues[newSelectedIndex];
                }
            }
            else
            {
                Instance.primaryMediation = AdsMediation.None;
            }


            if ((Instance.FlagMediations & AdsMediation.Iron) != 0 &&
                !PackagesManagerIntergration.IsSymbolEnabled("USE_IRON"))
            {
                EditorGUILayout.HelpBox("Add Symbols to Player Settings \"USE_IRON\"", MessageType.Warning);
            }

            if ((Instance.FlagMediations & AdsMediation.Max) != 0 &&
                !PackagesManagerIntergration.IsSymbolEnabled("USE_MAX"))
            {
                EditorGUILayout.HelpBox("Add Symbols to Player Settings \"USE_MAX\"", MessageType.Warning);
            }

            if ((Instance.FlagMediations & AdsMediation.Admob) != 0 &&
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
                
                bool shouldEnableNativeUnity = Instance.showADMOB && Instance.isUseNativeUnity;
                PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB_NATIVE_UNITY", shouldEnableNativeUnity);

                bool shouldEnableHideWhenFullscreen = Instance.showADMOB && Instance.isHideWhenFullscreenShowed;
                PackagesManagerIntergration.SetSymbolEnabled("HIDE_WHEN_FULLSCREEN_SHOWED", shouldEnableHideWhenFullscreen);
                
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

            DrawGoogleSheetSync();

            #region MAX

#if USE_MAX
            if ((Instance.FlagMediations & AdsMediation.Max) != 0)
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
            if ((Instance.FlagMediations & AdsMediation.Admob) != 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Admob", titleStyle);
                Instance.isShowAdmobNativeValidator = EditorGUILayout.Toggle("Show Admob Validator", Instance.isShowAdmobNativeValidator);
                Instance.isUseNativeUnity = EditorGUILayout.Toggle("Is Use Native Unity", Instance.isUseNativeUnity);
                Instance.isHideWhenFullscreenShowed = EditorGUILayout.Toggle("Hide When Fullscreen Showed", Instance.isHideWhenFullscreenShowed);
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

        private void DrawGoogleSheetSync()
        {
            EditorGUILayout.BeginVertical(GUI.skin.window);
            EditorGUILayout.LabelField("Google Sheet Sync", new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.yellow } });
            EditorGUILayout.Space(5);

            // Admob
            Instance.admobSheetUrl = EditorGUILayout.TextField("Admob CSV URL", Instance.admobSheetUrl);
            if (GUILayout.Button("Fetch Admob Data"))
            {
                if (!string.IsNullOrEmpty(Instance.admobSheetUrl))
                {
                    FetchDataFromGoogleSheet(Instance.admobSheetUrl, "ADMOB");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Admob Sheet URL is empty.", "OK");
                }
            }

            EditorGUILayout.Space(5);

            // MAX
            Instance.maxSheetUrl = EditorGUILayout.TextField("MAX CSV URL", Instance.maxSheetUrl);
            if (GUILayout.Button("Fetch MAX Data"))
            {
                if (!string.IsNullOrEmpty(Instance.maxSheetUrl))
                {
                    FetchDataFromGoogleSheet(Instance.maxSheetUrl, "MAX");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "MAX Sheet URL is empty.", "OK");
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void FetchDataFromGoogleSheet(string url, string mediationPrefix)
        {
            Debug.Log("Fetching data from Google Sheet...");
            UnityWebRequest www = UnityWebRequest.Get(url);
            var operation = www.SendWebRequest();

            EditorApplication.update += () =>
            {
                if (!operation.isDone)
                    return;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Fetch successful!");
                    ParseAndApplyData(www.downloadHandler.text, mediationPrefix);
                }
                else
                {
                    Debug.LogError("Fetch failed: " + www.error);
                    EditorUtility.DisplayDialog("Error", "Failed to fetch data from URL. Check console for details.", "OK");
                }
                
                EditorApplication.update = null;
            };
        }

        private void ParseAndApplyData(string csvData, string mediationPrefix)
        {
            if (string.IsNullOrEmpty(csvData)) return;

            var lines = csvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1)
            {
                Debug.LogWarning("CSV data is empty.");
                return;
            }

            var header = lines[0].Split(',');
            var dataRows = new List<string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                dataRows.Add(lines[i].Split(','));
            }

            var newData = new Dictionary<string, List<string>>();
            for (int j = 0; j < header.Length; j++)
            {
                var columnName = header[j].Trim();
                if (string.IsNullOrEmpty(columnName)) continue;

                var idsInColumn = new List<string>();
                for (int i = 0; i < dataRows.Count; i++)
                {
                    if (j < dataRows[i].Length)
                    {
                        string id = dataRows[i][j].Trim();
                        if (!string.IsNullOrEmpty(id))
                        {
                            idsInColumn.Add(id);
                        }
                    }
                }
                newData[columnName] = idsInColumn;
            }

            Action<object, string> applyDataToUnitId = (unitId, platformPrefix) =>
            {
                if (unitId == null) return;
                var fields = unitId.GetType().GetFields().Where(f => f.FieldType == typeof(List<Placement>));

                foreach (var field in fields)
                {
                    string adType = field.Name.Replace("Ids", "");
                    string columnName = $"{platformPrefix}_{adType}";

                    var idList = (List<Placement>)field.GetValue(unitId);
                    idList.Clear();

                    if (newData.ContainsKey(columnName))
                    {
                        foreach (var id in newData[columnName])
                        {
                            idList.Add(new Placement { stringIDs = new List<string> { id } });
                        }
                    }
                }
            };

            if (mediationPrefix.Equals("ADMOB", StringComparison.OrdinalIgnoreCase))
            {
                applyDataToUnitId(Instance.ADMOB_Android, "Android");
                applyDataToUnitId(Instance.ADMOB_IOS, "iOS");
            }
            else if (mediationPrefix.Equals("MAX", StringComparison.OrdinalIgnoreCase))
            {
                applyDataToUnitId(Instance.MAX_Android, "Android");
                applyDataToUnitId(Instance.MAX_iOS, "iOS");
            }

            Debug.Log($"Successfully applied data for {mediationPrefix} from Google Sheet!");
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            serializedObject.Update();
        }
    }
}
