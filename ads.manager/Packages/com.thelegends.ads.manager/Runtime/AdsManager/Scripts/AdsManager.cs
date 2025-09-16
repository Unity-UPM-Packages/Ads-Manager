using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using GoogleMobileAds.Api;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using TheLegends.Base.UI;
using TheLegends.Base.UnitySingleton;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdsManager : PersistentMonoSingleton<AdsManager>
    {
        [SerializeField]
        private Camera adsCamera;

        public Camera AdsCamera
        {
            get { return adsCamera; }
        }
        
        private List<AdsNetworkBase> adsNetworks = new List<AdsNetworkBase>();

        protected AdsSettings settingsAds = null;

        public AdsConfigs adsConfigs;

        public AdsSettings SettingsAds
        {
            get
            {
                if (settingsAds == null)
                {
                    settingsAds = Resources.Load<AdsSettings>(AdsSettings.FileName);
                }

                if (settingsAds == null)
                {
                    throw new Exception("[AdsManager]" + " AdsSettings NULL --> Please creat from Ads Manager/Ads Settings");
                }
                else
                {
                    return settingsAds;
                }
            }
            private set => settingsAds = value;
        }

        protected AdsNetworks DefaultMediation
        {
            get { return SettingsAds.AdsNetworks.FirstOrDefault(); }
        }

        private DateTime lastTimeShowAd = DateTime.Now.AddSeconds(-600);


        public bool IsTimeToShowAd
        {
            get
            {
                float totalTimePlay = (float)(DateTime.Now - lastTimeShowAd).TotalSeconds;
                bool canShowAds = Mathf.FloorToInt(totalTimePlay) >= adsConfigs.timePlayToShowAds;

                LogWarning(
                    $"Total Time play: {totalTimePlay} - Time to show ads: {adsConfigs.timePlayToShowAds} - Can show ads: {canShowAds}");

                return canShowAds;
            }
        }


        public Action<bool> OnCanShowAdsChanged;
        [SerializeField]
        private bool isCanShowAds = true;
        public bool IsCanShowAds
        {
            get {
                return isCanShowAds = PlayerPrefs.GetInt("IsCanShowAds", 1) == 1;
            }
            set
            {
                isCanShowAds = value;
                if (isCanShowAds)
                {
                    PlayerPrefs.SetInt("IsCanShowAds", 1);
                }
                else
                {
                    PlayerPrefs.SetInt("IsCanShowAds", 0);

                    var netWork = (AdmobNetworkController)GetNetwork(DefaultMediation);

                    if (netWork != null)
                    {
                        netWork.RemoveAds();
                    }
                }

                OnCanShowAdsChanged?.Invoke(isCanShowAds);
            }
        }

        private InitiationStatus status = InitiationStatus.NotInitialized;
        


        public void Init()
        {
            StartCoroutine(DoInit());
        }

        public IEnumerator DoInit()
        {
            if (SettingsAds.AdsNetworks == null || SettingsAds.AdsNetworks.Count == 0)
            {
                LogError("AdsNetworks NULL or Empty --> return");
                status = InitiationStatus.Failed;
                yield break;
            }

            if(status == InitiationStatus.Initialized)
            {
                LogError("AdsManager already initialized");
                yield break;
            }

            status = InitiationStatus.Initializing;

            adsNetworks = GetComponentsInChildren<AdsNetworkBase>().ToList();

            foreach (var network in adsNetworks)
            {
                yield return network.DoInit();
                yield return new WaitForSeconds(0.25f);
            }

            status = InitiationStatus.Initialized;
        }

        private AdsNetworkBase GetNetwork(AdsNetworks network)
        {
            return adsNetworks.FirstOrDefault(x => x.GetNetworkType() == network);
        }

        public void LoadInterstitial(AdsType interType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadInterstitial(interType, order);
            }
        }

        public void ShowInterstitial(AdsType interType, PlacementOrder order, string position, Action OnClose = null)
        {
            if (!IsInitialized())
            {
                return;
            }

            if (!IsTimeToShowAd)
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowInterstitial(interType, order, position, OnClose);
            }
        }

        public void LoadRewarded(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadRewarded(order);
            }
        }

        public void ShowRewarded(PlacementOrder order, string position, Action OnRewarded = null)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowRewarded(order, position, OnRewarded);

                if (GetAdsStatus(AdsType.Rewarded, order) != AdsEvents.LoadAvailable)
                {
                    UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);
                }
            }
        }

        public void LoadAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadAppOpen(order);
            }
        }

        public void ShowAppOpen(PlacementOrder order, string position)
        {
            if (!IsInitialized() || GetAdsStatus(AdsType.AppOpen, order) != AdsEvents.LoadAvailable)
            {
                return;
            }

            if (!IsTimeToShowAd)
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideAllBanner();
                netWork.HideAllMrec();
                netWork.ShowAppOpen(order, position);
            }
        }

        public void LoadBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadBanner(order);
            }
        }

        public void ShowBanner(PlacementOrder order, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowBanner(order, position);
            }
        }

        public void HideBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideBanner(order);
            }
        }

        public void LoadMrec(AdsType mrecType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadMrec(mrecType, order);
            }
        }

        public void ShowMrec(AdsType mrecType, PlacementOrder order, AdsPos mrecPosition, Vector2Int offset, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowMrec(mrecType, order, mrecPosition, offset, position);
            }
        }

        public void HideMrec(AdsType mrecType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideMrec(mrecType, order);
            }
        }

        public void LoadNativeOverlay(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeOverlay(order);
            }
        }

        public void ShowNativeOverlay(PlacementOrder order, NativeTemplateStyle style, AdsPos nativeOverlayposition, Vector2Int size, Vector2Int offset, string position, Action OnShow = null, Action OnClose = null)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.ShowNativeOverlay(order, style, nativeOverlayposition, size, offset, position, OnShow, OnClose);
            }
        }

        public void HideNativeOverlay(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeOverlay(order);
            }
        }

        public void LoadNativeBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeBanner(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeBanner(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeBanner(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeBanner(order);
            }
        }

        public void LoadNativeInter(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeInter(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeInter(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeInter(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeInter(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeInter(order);
            }
        }

        public void LoadNativeReward(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeReward(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeReward(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeReward(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeReward(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeReward(order);
            }
        }

        public void LoadNativeMrec(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeMrec(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeMrec(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeMrec(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeMrec(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeMrec(order);
            }
        }

        public void LoadNativeAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeAppOpen(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeAppOpen(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeAppOpen(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeAppOpen(order);
            }
        }

        public void LoadNativeInterOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeInterOpen(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeInterOpen(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeInterOpen(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeInterOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeInterOpen(order);
            }
        }

        public void LoadNativeMrecOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeMrecOpen(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeMrecOpen(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeMrecOpen(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeMrecOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeMrecOpen(order);
            }
        }

        public void LoadNativeVideo(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeVideo(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeVideo(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                return netWork.ShowNativeVideo(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeVideo(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeVideo(order);
            }
        }

        public AdsEvents GetAdsStatus(AdsType adsType, PlacementOrder order)
        {
            AdsEvents status = AdsEvents.None;

            if (!IsInitialized())
            {
                status = AdsEvents.None;
            }

            var netWork = (AdmobNetworkController)GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                status = netWork.GetAdsStatus(adsType, order);
            }

            return status;
        }

        public IEnumerator WaitAdLoaded(AdsType type, PlacementOrder order)
        {
            if (GetAdsStatus(type, order) == AdsEvents.None)
            {
                yield break;
            }

            while (GetAdsStatus(type, order) != AdsEvents.LoadAvailable && GetAdsStatus(type, order) != AdsEvents.LoadNotAvailable)
            {
                yield return null;
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            Debug.Log("OnApplicationPause " + isPaused);

            if (status == InitiationStatus.Initialized && isPaused == false)
            {
                StartCoroutine(IEShowAppOpen());
            }
        }

        private IEnumerator IEShowAppOpen()
        {
            yield return new WaitForEndOfFrame();
            ShowAppOpen(PlacementOrder.One ,"Pause");
        }


        #region Common

        public void SetStatus(AdsNetworks AdsNetworks, AdsType adsType, string adsUnitID, string position, AdsEvents adEvent, AdsNetworks networks)
        {
            string eventName = $"{AdsNetworks}_{adsType} | {adEvent.ToString()} | {adsUnitID} | {position}";
            string eventFirebaseName =  $"{adsType}_{adEvent.ToString()}";
            Log(eventName);

            FirebaseManager.Instance.LogEvent(eventFirebaseName, new Dictionary<string, object>()
            {
                { "network", networks.ToString() },
                { "type", adsType.ToString() },
                { "position", position },
                { "adUnitID", adsUnitID }
            });

            if ((adsType == AdsType.Interstitial || adsType == AdsType.AppOpen || adsType == AdsType.Rewarded) &&
                (adEvent == AdsEvents.ShowSuccess))
            {
                lastTimeShowAd = DateTime.Now;
            }
        }

        public void LogImpressionData(AdsNetworks network, AdsType adsType, string adsUnitID, object value)
        {
            string monetizationNetwork = "";
            double revenue = 0;
            string ad_unit_name = "";
            string ad_format = "";
            string country = "";
            string currency = "USD";
            MediationNetwork mediation = MediationNetwork.Custom;

            if (value == null)
            {
                LogWarning("LogImpressionData: " + "data NULL");
            }

#if USE_ADMOB
            if (value is GoogleMobileAds.Api.AdValue)
            {
                var impressionData = value as GoogleMobileAds.Api.AdValue;

                if (impressionData != null)
                {
                    mediation = MediationNetwork.GoogleAdMob;
                    monetizationNetwork = "googleadmob";
                    ad_format = adsType.ToString();
                    ad_unit_name = adsUnitID;
                    country = "";
                    //The ad's value in micro-units, where 1,000,000 micro-units equal one unit of the currency.
                    revenue = (double)impressionData.Value / 1000000f;
                    currency = impressionData.CurrencyCode;


                }

                Log("GoogleMobileAds AdValue: " + impressionData.Value + " Revenue: " + revenue + " CurrencyCode: " + currency + " Precision: " + impressionData.Precision);

            }
#endif

#if USE_APPSFLYER
            AppsFlyerManager.Instance.LogImpression(new Dictionary<string, string>()
            {
                { "mediation", mediation.ToString() },
                { "monetizationNetwork", monetizationNetwork },
                { "ad_format", ad_format },
                { "ad_unit_name", ad_unit_name },
                { "country", country },
                { "revenue", revenue.ToString() },
                { "currency", currency },
            });

            AppsFlyerManager.Instance.LogRevenue(monetizationNetwork, mediation, currency, revenue, new Dictionary<string, string>()
            {
                { AdRevenueScheme.AD_UNIT, ad_unit_name },
                { AdRevenueScheme.AD_TYPE, ad_format },
                { AdRevenueScheme.COUNTRY, country },
            });
#endif
        }

        public void Log(string message)
        {
            Debug.Log("AdsManager------: " + message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning("AdsManager------: " + message);
        }

        public void LogError(string message)
        {
            Debug.LogError("AdsManager------: " + message);
        }

        public void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public new bool IsInitialized()
        {
            bool isInitialized = false;
            string message = "";

            switch (status)
            {
                case InitiationStatus.Initialized:
                    isInitialized = true;
                    break;
                case InitiationStatus.NotInitialized:
                    message = "AdsManager is not initialized";
                    break;
                case InitiationStatus.Initializing:
                    message = "AdsManager initializing";
                    break;
                case InitiationStatus.Failed:
                    message = "AdsManager initial failed";
                    break;
            }

            if (!isInitialized)
            {
                LogError(message);
            }

            return isInitialized;

        }

        #endregion
    }

    public enum TagLog
    {
        UMP,
        ADMOB,
    }

    [System.Serializable]
    public class AdsConfigs
    {
        public bool adInterOnComplete = true;
        public bool adInterOnStart = true;
        public float timePlayToShowAds = 20f;
        public bool isUseAdNative = true;
        public float adNativeBannerHeight = 140;
        public float adNativeTimeReload = 15f;
        public float adLoadTimeOut = 5f;
        public float nativeVideoCountdownTimerDuration = 5f;
        public float nativeVideoDelayBeforeCountdown = 5f;
        public float nativeVideoCloseClickableDelay = 2f;
        public float nativeBannerTimeReload = 15f;
    }

}
