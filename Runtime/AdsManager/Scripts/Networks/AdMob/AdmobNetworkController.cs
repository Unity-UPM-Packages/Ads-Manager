using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNetworkController : AdsNetworkBase
    {
        private bool isChecking = false;
        private InitiationStatus status = InitiationStatus.NotInitialized;

        [Header("DEBUG")]
        [SerializeField]
        private List<string> testDevicesIDAds = new List<string>();

        [Space(5)]

        [SerializeField]
        private DebugGeography debugGeography = DebugGeography.Disabled;

        [SerializeField]
        private List<string> testDeivesIDConsent = new List<string>();

        private List<AdmobInterstitialController> interList = new List<AdmobInterstitialController>();
        private List<AdmobRewardedController> rewardList = new List<AdmobRewardedController>();
        private List<AdmobAppOpenController> appOpenList = new List<AdmobAppOpenController>();
        private List<AdmobBannerController> bannerList = new List<AdmobBannerController>();
        private List<AdmobMrecController> mrecList = new List<AdmobMrecController>();
        private List<AdmobMrecOpenController> mrecOpenList = new List<AdmobMrecOpenController>();
        private List<AdmobInterstitialOpenController> interOpenList = new List<AdmobInterstitialOpenController>();
        private List<AdmobNativeOverlayController> nativeOverlayList = new List<AdmobNativeOverlayController>();
        private List<AdmobNativePlatformController> nativePlatformList = new List<AdmobNativePlatformController>();

        // Danh sách các trường ID cần loại trừ khỏi quá trình tự động tạo controller
        private readonly List<string> excludedIdFields = new List<string>
        {
            "nativeIds" // Không tạo controller cho nativeIds
            // Thêm các ID khác cần loại trừ ở đây
        };

        public override IEnumerator DoInit()
        {
            status = InitiationStatus.Initializing;


#if (UNITY_ANDROID || UNITY_IOS) && USE_ADMOB
            var platform = Application.platform;
            var isIOS = platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.OSXPlayer;

            var isAdmobTest = AdsManager.Instance.SettingsAds.isAdmobTest;

            // Lấy tất cả các trường trong AdmobUnitID
            var unitIdFields = typeof(AdmobUnitID).GetFields();
            
            // Lấy tất cả các trường danh sách controller trong AdmobNetworkController
            var controllerListFields = this.GetType()
                .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(f => f.FieldType.IsGenericType && 
                           f.FieldType.GetGenericTypeDefinition() == typeof(List<>) && 
                           typeof(AdsPlacementBase).IsAssignableFrom(f.FieldType.GetGenericArguments()[0]))
                .ToList();

            // Tạo một ánh xạ từ tên trường trong AdmobUnitID đến trường danh sách controller
            foreach (var unitIdField in unitIdFields)
            {
                // Chỉ xử lý các trường kiểu List<Placement>
                if (unitIdField.FieldType == typeof(List<Placement>))
                {
                    string fieldName = unitIdField.Name;
                    
                    // Kiểm tra xem trường này có nằm trong danh sách loại trừ không
                    if (excludedIdFields.Contains(fieldName))
                    {
                        // Bỏ qua trường này
                        continue;
                    }
                    
                    // Tìm trường danh sách controller tương ứng dựa trên quy ước đặt tên
                    var controllerField = controllerListFields.FirstOrDefault(f => 
                        fieldName.Replace("Ids", "List").Equals(f.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (controllerField != null)
                    {
                        // Lấy danh sách Placement từ các AdmobUnitID
                        var iosIds = AdsManager.Instance.SettingsAds.ADMOB_IOS;
                        var androidIds = AdsManager.Instance.SettingsAds.ADMOB_Android;
                        var iosTestIds = AdsManager.Instance.SettingsAds.ADMOB_IOS_Test;
                        var androidTestIds = AdsManager.Instance.SettingsAds.ADMOB_Android_Test;
                        
                        var placements = GetAdUnitIds(
                            isIOS,
                            isAdmobTest,
                            (List<Placement>)unitIdField.GetValue(iosIds),
                            (List<Placement>)unitIdField.GetValue(androidIds),
                            (List<Placement>)unitIdField.GetValue(iosTestIds),
                            (List<Placement>)unitIdField.GetValue(androidTestIds)
                        );
                        
                        // Lấy danh sách controller
                        var controllerList = controllerField.GetValue(this);
                        
                        // Gọi phương thức CreateAdController với generic type phù hợp
                        var controllerType = controllerField.FieldType.GetGenericArguments()[0];
                        var methodInfo = typeof(AdmobNetworkController).GetMethod("CreateAdController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var genericMethod = methodInfo.MakeGenericMethod(controllerType);
                        genericMethod.Invoke(this, new object[] { placements, controllerList });
                    }
                    else
                    {
                        AdsManager.Instance.LogError($"Cannot find controller list for {fieldName} - skipping");
                    }
                }
            }
#endif

            yield return RequestUMP();

            if (ConsentInformation.CanRequestAds())
            {
                MobileAds.SetRequestConfiguration( new RequestConfiguration
                {
                    TestDeviceIds = testDevicesIDAds
                });

                MobileAds.RaiseAdEventsOnUnityMainThread = true;

                MobileAds.SetiOSAppPauseOnBackground(true);

                MobileAds.Initialize(initStatus =>
                {
                    PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (initStatus == null)
                        {
                            status = InitiationStatus.Failed;

                            AdsManager.Instance.LogError("Google Mobile Ads initialization failed.");
                            return;
                        }

                        if (initStatus != null)
                        {
                            status = InitiationStatus.Initialized;

                            AdsManager.Instance.Log($"{TagLog.ADMOB} " + "Mediations checking status...");
                            // If you use mediation, you can check the status of each adapter.
                            var adapterStatusMap = initStatus.getAdapterStatusMap();
                            if (adapterStatusMap != null)
                            {
                                foreach (var item in adapterStatusMap)
                                {
                                    AdsManager.Instance.Log($"{TagLog.ADMOB} " + string.Format(" Google Adapter {0} is {1}",
                                        item.Key,
                                        item.Value.InitializationState));
                                }
                            }
                            AdsManager.Instance.Log($"{TagLog.ADMOB} " + "Mediations checking done.");

                        }

                        AdsManager.Instance.Log($"{TagLog.ADMOB} " + "Initialize: " + initStatus.ToString());

                    });

                });
            }
            else
            {
                AdsManager.Instance.Log($"{TagLog.UMP} " + "UMP ConsentStatus --> " + ConsentInformation.ConsentStatus.ToString() + " CanRequestAds: " + ConsentInformation.CanRequestAds().ToString().ToUpper() + " --> NOT INIT");
                status = InitiationStatus.Failed;
            }



            while (status == InitiationStatus.Initializing)
            {
                yield return null;
            }
        }



        private IEnumerator RequestUMP()
        {
            if (isChecking)
            {
                AdsManager.Instance.LogError($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " CHECKING");
                yield break;
            }

            // Create a ConsentRequestParameters object.
            ConsentRequestParameters request = new ConsentRequestParameters
            {
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    TestDeviceHashedIds = testDeivesIDConsent,
                    DebugGeography = debugGeography
                }
            };

            // Check the current consent information status.
            ConsentInformation.Update(request, (updateError =>
            {
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (updateError != null)
                    {
                        // Handle the error.
                        AdsManager.Instance.LogError($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + updateError.Message);
                        isChecking = false;
                        return;
                    }

                    if (ConsentInformation.CanRequestAds()) // Determine the consent-related action to take based on the ConsentStatus.
                    {
                        // Consent has already been gathered or not required.
                        // Return control back to the user.
                        AdsManager.Instance.Log($"{TagLog.UMP} " + "Update " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " -- Consent has already been gathered or not required");
                        return;
                    }

                    AdsManager.Instance.Log(ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW ConsentForm If Required");

                    // If the error is null, the consent information state was updated.
                    // You are now ready to check if a form is available.
                    ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                    {
                        if (formError != null)
                        {
                            // Consent gathering failed.
                            AdsManager.Instance.LogError($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + formError.Message);
                            return;
                        }
                        else
                        {
                            // Form showing succeeded.
                            AdsManager.Instance.Log($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                        }
                        
                        isChecking = false;
                    });
                });

            }));

            while (isChecking && (ConsentInformation.ConsentStatus == ConsentStatus.Required || ConsentInformation.ConsentStatus == ConsentStatus.Unknown))
            {
                yield return null;
            }
        }

        private List<Placement> GetAdUnitIds(bool isIOS, bool isAdmobTest, List<Placement> iosIds, List<Placement> androidIds, List<Placement> iosTestIds, List<Placement> androidTestIds)
        {
            return isAdmobTest ? (isIOS ? iosTestIds : androidTestIds) : (isIOS ? iosIds : androidIds);
        }

        private void CreateAdController<T>(List<Placement> placements, List<T> adList) where T : AdsPlacementBase
        {
            if (placements.Count <= 0)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {typeof(T).Name} IDs NULL or Empty --> return");
                return;
            }

            foreach (var adId in placements)
            {
                var adController = new GameObject().AddComponent<T>();
                adController.name = typeof(T).Name;
                adController.transform.parent = this.transform;
                ((AdsPlacementBase)adController).Init(adId);
                adList.Add(adController);
            }
        }

        public void OpenAdInspector()
        {
            AdsManager.Instance.Log("Opening ad Inspector.");
            MobileAds.OpenAdInspector((AdInspectorError error) =>
            {
                // If the operation failed, an error is returned.
                if (error != null)
                {
                    AdsManager.Instance.Log("Ad Inspector failed to open with error: " + error);
                    return;
                }

                AdsManager.Instance.Log("Ad Inspector opened successfully.");
            });
        }

        public override AdsEvents GetAdsStatus(AdsType type, PlacementOrder order)
        {
            var listPlacement = new List<AdsPlacementBase>();

            switch (type)
            {
                case AdsType.Banner:
                    listPlacement = bannerList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Interstitial:
                    listPlacement = interList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Rewarded:
                    listPlacement = rewardList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Mrec:
                    listPlacement = mrecList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.AppOpen:
                    listPlacement = appOpenList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.MrecOpen:
                    listPlacement = mrecOpenList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.InterOpen:
                    listPlacement = interOpenList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.NativeOverlay:
                    listPlacement = nativeOverlayList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Native:
                    listPlacement = nativeOverlayList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.NativePlatform:
                    listPlacement = nativePlatformList.Cast<AdsPlacementBase>().ToList();
                    break;
            }

            if (!IsListExist(listPlacement))
            {
                return AdsEvents.None;
            }

            var index = GetPlacementIndex((int)order, listPlacement.Count);

            if (index == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {type} {order} is not exist");
                return AdsEvents.None;
            }

            return listPlacement[index].Status;
        }

        private int GetPlacementIndex(int order, int listCount)
        {
            if (listCount <= 0)
            {
                return -1;
            }

            if (order > listCount)
            {
                return -1;
            }

            return Mathf.Clamp(order - 1, 0, listCount - 1);
        }

        public override void LoadInterstitial(AdsType interType, PlacementOrder order)
        {
            var list = interType == AdsType.InterOpen ? (new List<AdmobInterstitialController>(interOpenList)) : interList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {interType} {order} is not exist");
                return;
            }

            list[placementIndex].LoadAds();
        }

        public override void ShowInterstitial(AdsType interType, PlacementOrder order, string position, Action OnClose = null)
        {
            var list = interType == AdsType.InterOpen ? (new List<AdmobInterstitialController>(interOpenList)) : interList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {interType} {order} is not exist");
                return;
            }

            list[placementIndex].ShowAds(position, OnClose);
        }

        public override void LoadRewarded(PlacementOrder order)
        {
            if (!IsListExist(rewardList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, rewardList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Rewarded"} {order} is not exist");
                return;
            }

            rewardList[placementIndex].LoadAds();
        }

        public override void ShowRewarded(PlacementOrder order, string position, Action OnRewarded = null)
        {
            if (!IsListExist(rewardList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, rewardList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Rewarded"} {order} is not exist");
                return;
            }

            rewardList[placementIndex].ShowAds(position, OnRewarded);
        }

        public override void LoadAppOpen(PlacementOrder order)
        {
            if (!IsListExist(appOpenList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, appOpenList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"AppOpen"} {order} is not exist");
                return;
            }

            appOpenList[placementIndex].LoadAds();
        }

        public override void ShowAppOpen(PlacementOrder order, string position)
        {
            if (!IsListExist(appOpenList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, appOpenList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"AppOpen"} {order} is not exist");
                return;
            }

            appOpenList[placementIndex].ShowAds(position);
        }

        public override void LoadBanner(PlacementOrder order)
        {
            if (!IsListExist(bannerList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, bannerList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Banner"} {order} is not exist");
                return;
            }

            bannerList[placementIndex].LoadAds();
        }

        public override void HideBanner(PlacementOrder order)
        {
            if (!IsListExist(bannerList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, bannerList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Banner"} {order} is not exist");
                return;
            }

            bannerList[placementIndex].HideAds();
        }

        public override void ShowBanner(PlacementOrder order, string position)
        {
            if (!IsListExist(bannerList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, bannerList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Banner"} {order} is not exist");
                return;
            }

            bannerList[placementIndex].ShowAds(position);
        }

        public override void LoadMrec(AdsType mrecType, PlacementOrder order)
        {
            var list = mrecType == AdsType.MrecOpen ?(new List<AdmobMrecController>(mrecOpenList)) : mrecList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Mrec"} {order} is not exist");
                return;
            }

            list[placementIndex].LoadAds();
        }

        public override void ShowMrec(AdsType mrecType, PlacementOrder order, AdsPos mrecPosition, Vector2Int offset, string position)
        {
            var list = mrecType == AdsType.MrecOpen ?(new List<AdmobMrecController>(mrecOpenList)) : mrecList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Mrec"} {order} is not exist");
                return;
            }

            list[placementIndex].ShowAds(mrecPosition, offset, position);
        }

        public override void HideMrec(AdsType mrecType, PlacementOrder order)
        {
            var list = mrecType == AdsType.MrecOpen ?(new List<AdmobMrecController>(mrecOpenList)) : mrecList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"Mrec"} {order} is not exist");
                return;
            }

            list[placementIndex].HideAds();
        }

        public void LoadNativeOverlay(PlacementOrder order)
        {
            if (!IsListExist(nativeOverlayList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, nativeOverlayList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"NativeOverlay"} {order} is not exist");
                return;
            }

            nativeOverlayList[placementIndex].LoadAds();
        }

        public void ShowNativeOverlay(PlacementOrder order, NativeTemplateStyle style, AdsPos nativeOverlayposition, Vector2Int size, Vector2Int offset, string position, Action OnShow = null, Action OnClose = null)
        {
            if (!IsListExist(nativeOverlayList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, nativeOverlayList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"NativeOverlay"} {order} is not exist");
                return;
            }

            nativeOverlayList[placementIndex].ShowAds(style, nativeOverlayposition, size, offset, position, OnShow, OnClose);
        }

        public void HideNativeOverlay(PlacementOrder order)
        {
            if (!IsListExist(nativeOverlayList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, nativeOverlayList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"NativeOverlay"} {order} is not exist");
                return;
            }

            nativeOverlayList[placementIndex].HideAds();
        }

        public void LoadNativePlatform(PlacementOrder order)
        {
            if (!IsListExist(nativePlatformList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, nativePlatformList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"NativePlatform"} {order} is not exist");
                return;
            }

            nativePlatformList[placementIndex].LoadAds();
        }

        public NativePlatformShowBuilder ShowNativePlatform(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsListExist(nativePlatformList))
            {
                return null;
            }

            var placementIndex = GetPlacementIndex((int)order, nativePlatformList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"NativePlatform"} {order} is not exist");
                return null;
            }

            var controller = nativePlatformList[placementIndex];

            return new NativePlatformShowBuilder(controller, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
        }

        public void HideNativePlatform(PlacementOrder order)
        {
            if (!IsListExist(nativePlatformList))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, nativePlatformList.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {"NativePlatform"} {order} is not exist");
                return;
            }

            nativePlatformList[placementIndex].HideAds();
        }

        private bool IsListExist<T>(List<T> list) where T : AdsPlacementBase
        {
            bool isExist = false;
            isExist = list.Count > 0;

            if (!isExist)
            {
                AdsManager.Instance.LogError($"{typeof(T).Name} is empty");
            }

            return isExist;
        }

        public override void HideAllBanner()
        {
            foreach (var banner in bannerList)
            {
                banner.HideAds();
            }
        }

        public override void HideAllMrec()
        {
            foreach (var mrec in mrecList)
            {
                mrec.HideAds();
            }
        }


        public override AdsNetworks GetNetworkType()
        {
            return AdsNetworks.Admob;
        }
    }
}

