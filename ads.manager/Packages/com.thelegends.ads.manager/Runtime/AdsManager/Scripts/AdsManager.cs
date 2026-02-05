using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
#if USE_ADMOB
using GoogleMobileAds.Api;
#endif
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Databuckets;
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

        private List<AdsMediationBase> adsMediations = new List<AdsMediationBase>();

        private readonly WaitForSeconds _initDelay = new WaitForSeconds(0.25f);
        private readonly WaitForSeconds _showAppOpenDelay = new WaitForSeconds(0.5f);

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

        protected AdsMediation DefaultMediation
        {
            get { return SettingsAds.AdsMediations.FirstOrDefault(); }
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
            get
            {
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

                    foreach (var mediation in adsMediations)
                    {
                        mediation.RemoveAds();
                    }
                }

                OnCanShowAdsChanged?.Invoke(isCanShowAds);
            }
        }

        private List<BannerShowedConfig> bannerShowedConfigs = new List<BannerShowedConfig>();
#if USE_ADMOB
        private List<NativeShowedConfig> nativeBannerShowedConfigs = new List<NativeShowedConfig>();
#endif

        private InitiationStatus status = InitiationStatus.NotInitialized;


        public IEnumerator DoInit()
        {
            if (SettingsAds.AdsMediations == null || SettingsAds.AdsMediations.Count == 0)
            {
                LogError("AdsMediations NULL or Empty --> return");
                status = InitiationStatus.Failed;
                yield break;
            }

            if (status == InitiationStatus.Initialized)
            {
                LogError("AdsManager already initialized");
                yield break;
            }

            status = InitiationStatus.Initializing;

            adsMediations = GetComponentsInChildren<AdsMediationBase>().ToList();

            foreach (var mediation in adsMediations)
            {
                yield return mediation.DoInit();
                yield return _initDelay;
            }

            status = InitiationStatus.Initialized;
        }


        #region Interstitial

        public void LoadInterstitial(AdsType interType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.LoadInterstitial(interType, order);
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

            var mediation = GetMediationToShow(interType, order);

            if (mediation != null)
            {
                mediation.ShowInterstitial(interType, order, position, OnClose);
            }
        }

        #endregion

        #region Rewarded

        public void LoadRewarded(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.LoadRewarded(order);
            }
        }

        public void ShowRewarded(PlacementOrder order, string position, Action OnRewarded = null)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = GetMediationToShow(AdsType.Rewarded, order);

            if (mediation != null)
            {
                mediation.ShowRewarded(order, position, OnRewarded);

                if (GetAdsStatus(AdsType.Rewarded, order) != AdsEvents.LoadAvailable)
                {
                    UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);
                }
            }
        }

        #endregion

        #region AppOpen

        public void LoadAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.LoadAppOpen(order);
            }

        }

        public void ShowAppOpen(PlacementOrder order, string position, Action OnClose = null)
        {
            if (!IsInitialized() || GetAdsStatus(AdsType.AppOpen, order) != AdsEvents.LoadAvailable)
            {
                return;
            }

            if (!IsTimeToShowAd)
            {
                return;
            }

            var mediation = GetMediationToShow(AdsType.AppOpen, order);

            if (mediation != null)
            {
                mediation.ShowAppOpen(order, position, OnClose);
            }
        }

        #endregion

        #region Banner

        public void LoadBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.LoadBanner(order);
            }

        }

        public void ShowBanner(PlacementOrder order, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = GetMediationToShow(AdsType.Banner, order);

            if (mediation != null)
            {
                mediation.ShowBanner(order, position);
            }
        }

        public void HideBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.HideBanner(order);
            }

            var config = bannerShowedConfigs.FirstOrDefault(x => x.order == order);
            UnregisterBannerConfig(config);
        }

        public void HideAllBanner()
        {
            foreach (var mediation in adsMediations)
            {
                mediation.HideAllBanner();
            }
        }

        public void RegisterBannerConfig(BannerShowedConfig config)
        {
            var existedConfig = bannerShowedConfigs.FirstOrDefault(x => x.order == config.order);
            if (existedConfig != null)
            {
                existedConfig = config;
            }
            else
            {
                bannerShowedConfigs.Add(config);
            }
        }

        private void UnregisterBannerConfig(BannerShowedConfig config)
        {
            bannerShowedConfigs.RemoveAll(x => x.order == config.order);
        }

        public void ShowRegisteredBanners()
        {
            foreach (var config in bannerShowedConfigs)
            {
                ShowBanner(config.order, config.position);
            }
        }

        #endregion

        #region Mrec

        public void LoadMrec(AdsType mrecType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.LoadMrec(mrecType, order);
            }

        }

        public void ShowMrec(AdsType mrecType, PlacementOrder order, AdsPos mrecPosition, Vector2Int offset, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = GetMediationToShow(mrecType, order);

            if (mediation != null)
            {
                mediation.ShowMrec(mrecType, order, mrecPosition, offset, position);
            }
        }

        public void HideMrec(AdsType mrecType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            foreach (var mediation in adsMediations)
            {
                mediation.HideMrec(mrecType, order);
            }
        }

        public void HideAllMrec()
        {
            foreach (var mediation in adsMediations)
            {
                mediation.HideAllMrec();
            }
        }

        #endregion



#if USE_ADMOB

        #region NativeOverlay

        public void LoadNativeOverlay(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeOverlay(order);
            }
        }

        public void ShowNativeOverlay(PlacementOrder order, NativeTemplateStyle style, AdsPos nativeOverlayposition, Vector2Int size, Vector2Int offset, string position, Action OnShow = null, Action OnClose = null)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.ShowNativeOverlay(order, style, nativeOverlayposition, size, offset, position, OnShow, OnClose);
            }
        }

        public void HideNativeOverlay(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeOverlay(order);
            }
        }

        #endregion

        #region NativeBanner

        public void LoadNativeBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeBanner(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeBanner(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeBanner(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeBanner(order);
            }

            var config = nativeBannerShowedConfigs.FirstOrDefault(x => x.order == order);
            UnregisterNativeBannerConfig(config);
        }

        public void HideAllNativeBanner()
        {
            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);
            if (mediation != null)
            {
                mediation.HideAllNativeBanner();
            }
        }

        public void RegisterNativeBannerConfig(NativeShowedConfig config)
        {
            var existedConfig = nativeBannerShowedConfigs.FirstOrDefault(x => x.order == config.order);
            if (existedConfig != null)
            {
                existedConfig = config;
            }
            else
            {
                nativeBannerShowedConfigs.Add(config);
            }
        }

        private void UnregisterNativeBannerConfig(NativeShowedConfig config)
        {
            nativeBannerShowedConfigs.RemoveAll(x => x.order == config.order);
        }

        public void ShowRegisteredNativeBanners()
        {
            foreach (var config in nativeBannerShowedConfigs)
            {
                ShowNativeBanner(config.order, config.position, config.layoutName)
                ?.WithAutoReload(config.reloadTime)
                ?.WithShowOnLoaded(config.showOnLoaded)
                ?.Execute();
            }
        }

        #endregion

        #region NativeInter

        public void LoadNativeInter(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeInter(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeInter(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeInter(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeInter(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeInter(order);
            }
        }

        #endregion

        #region NativeReward

        public void LoadNativeReward(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeReward(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeReward(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeReward(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeReward(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeReward(order);
            }
        }

        #endregion

        #region NativeMrec

        public void LoadNativeMrec(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeMrec(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeMrec(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeMrec(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeMrec(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeMrec(order);
            }
        }

        public void HideAllNativeMrec()
        {
            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);
            if (mediation != null)
            {
                mediation.HideAllNativeMrec();
            }
        }


        #endregion

        #region NativeAppOpen

        public void LoadNativeAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeAppOpen(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeAppOpen(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeAppOpen(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeAppOpen(order);
            }
        }

        #endregion

        #region NativeInterOpen

        public void LoadNativeInterOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeInterOpen(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeInterOpen(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeInterOpen(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeInterOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeInterOpen(order);
            }
        }

        #endregion

        #region NativeMrecOpen

        public void LoadNativeMrecOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeMrecOpen(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeMrecOpen(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeMrecOpen(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeMrecOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeMrecOpen(order);
            }
        }

        #endregion

        #region NativeVideo

        public void LoadNativeVideo(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.LoadNativeVideo(order);
            }
        }

        public NativePlatformShowBuilder ShowNativeVideo(PlacementOrder order, string position, string layoutName, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            if (!IsInitialized())
            {
                LogError("AdsManager not initialized");
                return null;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                return mediation.ShowNativeVideo(order, position, layoutName, OnShow, OnClose, OnAdDismissedFullScreenContent);
            }

            return null;
        }

        public void HideNativeVideo(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var mediation = (AdmobMediationController)GetMediation(AdsMediation.Admob);

            if (mediation != null)
            {
                mediation.HideNativeVideo(order);
            }
        }

        #endregion

#endif


        #region Common

        public void SetStatus(AdsMediation AdsMediation, AdsType adsType, string adsUnitID, string position, AdsEvents adEvent)
        {
            string eventName = $"{AdsMediation}_{adsType} | {adEvent.ToString()} | {adsUnitID} | {position}";
            string eventFirebaseName = $"{adsType}_{adEvent.ToString()}";
            Log(eventName);

            FirebaseManager.Instance.LogEvent(eventFirebaseName, new Dictionary<string, object>()
            {
                { "mediation", AdsMediation.ToString() },
                { "type", adsType.ToString() },
                { "position", position },
                { "adUnitID", adsUnitID }
            });

            if ((adsType == AdsType.Interstitial ||
                adsType == AdsType.AppOpen ||
                adsType == AdsType.Rewarded ||
                adsType == AdsType.InterOpen ||
                adsType == AdsType.NativeInter ||
                adsType == AdsType.NativeInterOpen ||
                adsType == AdsType.NativeReward ||
                adsType == AdsType.NativeAppOpen) &&
                (adEvent == AdsEvents.ShowSuccess))
            {
                lastTimeShowAd = DateTime.Now;
            }
        }

        public AdsEvents GetAdsStatus(AdsType adsType, PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return AdsEvents.None;
            }

            AdsEvents bestStatus = AdsEvents.None;

            foreach (var mediation in adsMediations)
            {

                if (mediation.GetMediationType() == AdsMediation.Max && (adsType == AdsType.Mrec || adsType == AdsType.MrecOpen))
                {
                    return AdsEvents.LoadAvailable;
                }

                AdsEvents mediationStatus = mediation.GetAdsStatus(adsType, order);

                if (mediationStatus == AdsEvents.LoadAvailable)
                {
                    return AdsEvents.LoadAvailable;
                }

                switch (mediationStatus)
                {
                    case AdsEvents.LoadRequest:
                        if (bestStatus != AdsEvents.LoadRequest)
                        {
                            bestStatus = AdsEvents.LoadRequest;
                        }
                        break;

                    case AdsEvents.LoadFail:
                    case AdsEvents.LoadTimeOut:
                    case AdsEvents.LoadNotAvailable:
                        if (bestStatus == AdsEvents.None)
                        {
                            bestStatus = mediationStatus;
                        }
                        break;
                }
            }

            return bestStatus;
        }

        private AdsMediationBase GetMediation(AdsMediation mediation)
        {
            return adsMediations.FirstOrDefault(x => x.GetMediationType() == mediation);
        }

        private AdsMediationBase GetMediationToShow(AdsType adsType, PlacementOrder order)
        {
            var primaryMediation = SettingsAds.primaryMediation;

            var primary = adsMediations.FirstOrDefault(n => n.GetMediationType() == primaryMediation);
            if (primary != null)
            {
                bool isControllerExist = primary.IsAdsControllerExist(adsType, order);
                if (primaryMediation == AdsMediation.Max)
                {
                    bool isMrec = adsType == AdsType.Mrec || adsType == AdsType.MrecOpen;
                    if (isMrec && isControllerExist)
                    {
                        return primary;
                    }
                    else if (isControllerExist && primary.IsAdsReady(adsType, order))
                    {
                        return primary;
                    }
                }
                else if (isControllerExist && primary.IsAdsReady(adsType, order))
                {
                    return primary;
                }
            }

            var fallback = adsMediations.FirstOrDefault(n => n.GetMediationType() != primaryMediation && n.IsAdsControllerExist(adsType, order) && n.IsAdsReady(adsType, order));
            if (fallback != null)
            {
                return fallback;
            }

            return primary;
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

        public void OnFullScreenAdsShow()
        {
#if HIDE_WHEN_FULLSCREEN_SHOWED
            HideAllBanner();
            HideAllMrec();
#if USE_ADMOB
            HideAllNativeBanner();
            HideAllNativeMrec();
#endif
#endif
        }

        public void OnFullScreenAdsClosed()
        {
#if HIDE_WHEN_FULLSCREEN_SHOWED
            ShowRegisteredBanners();
#if USE_ADMOB
            ShowRegisteredNativeBanners();
#endif
#endif
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
            yield return _showAppOpenDelay;
            ShowAppOpen(PlacementOrder.One, "Pause");
        }

        public void LogImpressionData(AdsMediation ad_mediation, AdsType adsType, string adsUnitID, string network, string position, object value)
        {
            string monetizationNetwork = "";
            double revenue = 0;
            string ad_unit_name = "";
            string ad_format = "";
            string country = "";
            string currency = "USD";
            string placement = position;

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

#if USE_MAX
            if (value is MaxSdk.AdInfo)
            {
                var impressionData = value as MaxSdk.AdInfo;

                if (impressionData != null)
                {
                    mediation = MediationNetwork.ApplovinMax;
                    monetizationNetwork = "applovinmax";
                    ad_format = adsType.ToString();
                    ad_unit_name = adsUnitID;
                    country = "";
                    revenue = (double)impressionData.Revenue;
                    currency = MaxSdk.GetSdkConfiguration().CountryCode;
                }

#if USE_FIREBASE

                var impressionParameters = new Dictionary<string, object>
                {
                    {"ad_platform", "AppLovin"},
                    {"ad_source", impressionData.NetworkName},
                    {"ad_unit_name", impressionData.AdUnitIdentifier},
                    {"ad_format", impressionData.AdFormat},
                    {"value", revenue},
                    {"currency", "USD"},
                    {"placement", placement}
                };

                FirebaseManager.Instance.LogEvent("ad_impression", impressionParameters);
#endif

                Log("ApplovinMax AdInfo: " + impressionData.Revenue + " Revenue: " + revenue + " CurrencyCode: " + currency + " Precision: " + impressionData.RevenuePrecision);
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

#if USE_DATABUCKETS

            DatabucketsManager.Instance.RecordEvent("ad_impression", new Dictionary<string, object>
            {
                { "ad_format", ad_format },
                { "ad_platform", mediation.ToString() },
                { "ad_network", network},
                { "ad_unit_id", adsUnitID },
                { "placement", placement },
                { "value", revenue }
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
        MAX,
    }

    [System.Serializable]
    public class AdsConfigs
    {
        public bool isUseAdInterOpen = true;
        public float adInterOpenTimeOut = 5f;
        public bool isUseAdMrecOpen = true;
        public float adMrecOpenTimeOut = 5f;
        public bool isUseAdAppOpenOpen = true;
        public float adAppOpenOpenTimeOut = 5f;
        public bool adInterOnComplete = true;
        public bool adInterOnStart = true;
        public float timePlayToShowAds = 20f;
        public bool isUseAdNative = true;
        public float adNativeBannerHeight = 140;
        public float adTimeReload = 15f;
        public float adLoadTimeOut = 5f;
        public float nativeVideoCountdownTimerDuration = 5f;
        public float nativeVideoDelayBeforeCountdown = 5f;
        public float nativeVideoCloseClickableDelay = 2f;
        public float nativeBannerTimeReload = 15f;
    }

}
