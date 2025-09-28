#if USE_MAX
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MaxSdkBase;

namespace TheLegends.Base.Ads
{
    public class MaxBannerController : AdsPlacementBase
    {
        private string _currentLoadRequestId;
        private string _loadRequestId;
        private float timeAutoReload;

        private bool isReady = false;
        private bool isShowOnLoaded = false;
        private string showPosition = string.Empty;
        public override AdsNetworks GetAdsNetworks()
        {
#if USE_MAX
            return AdsNetworks.Max;
#else
            return AdsNetworks.None;
#endif
        }

        public override AdsType GetAdsType()
        {
#if USE_MAX
            return AdsType.Banner;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_MAX
            return isReady;
#else
            return false;
#endif
        }

        void Awake()
        {
            timeAutoReload = AdsManager.Instance.adsConfigs.adNativeTimeReload;
        }

        public override void LoadAds()
        {
#if USE_MAX
            if (!IsCanLoadAds())
            {
                return;
            }

            if (!IsReady)
            {
                base.LoadAds();

                CreateBanner();

                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerRevenuePaidEvent;

                _currentLoadRequestId = Guid.NewGuid().ToString();
                _loadRequestId = _currentLoadRequestId;

                MaxSdk.LoadBanner(adsUnitID);
            }
#endif
        }

        public override void ShowAds(string showPosition)
        {
            base.ShowAds(showPosition);
            this.showPosition = showPosition;
            isShowOnLoaded = true;
#if USE_MAX
            if (IsReady && IsAvailable)
            {
                Status = AdsEvents.ShowSuccess;
                MaxSdk.ShowBanner(adsUnitID);
                // CancelReloadAds();
                // DelayReloadAd(timeAutoReload);
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }

        protected virtual void CreateBanner()
        {
#if USE_MAX
            // AdViewPosition adPosition = AdsManager.Instance.SettingsAds.bannerPosition == BannerPos.Top
            //     ? AdViewPosition.TopCenter
            //     : AdViewPosition.BottomCenter;

            // if (AdsManager.Instance.SettingsAds.fixBannerSmallSize)
            // {
            //     var adViewConfiguration = new AdViewConfiguration(adPosition);
            //     MaxSdk.CreateBanner(adsUnitID, adViewConfiguration);
            //     MaxSdk.SetBannerBackgroundColor(adsUnitID, Color.black);
            // }

            BannerPosition adPosition = AdsManager.Instance.SettingsAds.bannerPosition == BannerPos.Top
                ? BannerPosition.TopCenter
                : BannerPosition.BottomCenter;

            MaxSdk.CreateBanner(adsUnitID, adPosition);
            MaxSdk.SetBannerBackgroundColor(adsUnitID, Color.black);

            if (!AdsManager.Instance.SettingsAds.fixBannerSmallSize)
            {
                var density = MaxSdkUtils.GetScreenDensity();
                var bannerWidth = Screen.width / density;
                MaxSdk.SetBannerWidth(adsUnitID, bannerWidth);
            }
#endif
        }

        #region Internal

        protected override void OnAdsLoadFailed(string message)
        {
            base.OnAdsLoadFailed(message);
            if (Status == AdsEvents.LoadNotAvailable)
            {
                DelayReloadAd(timeAutoReload);
            }
        }

        private void OnBannerLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                isReady = true;
                OnAdsLoadAvailable();

                if (isShowOnLoaded)
                {
                    ShowAds(showPosition);
                }
            });
        }


        private void OnBannerLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                OnAdsLoadFailed(errorInfo.Message);

                MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnBannerLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnBannerLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent -= OnBannerClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerRevenuePaidEvent;
            });
        }

        private void OnBannerClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsClick();
            });
        }

        private void OnBannerRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, adInfo);
            });
        }

        protected void BannerDestroy()
        {
#if USE_MAX
            if (IsReady)
            {
                try
                {
                    MaxSdk.DestroyBanner(adsUnitID);
                    CancelReloadAds();
                }
                catch (Exception ex)
                {
                    AdsManager.Instance.LogException(ex);
                }
            }
#endif
        }

        #endregion

        public void HideAds()
        {
#if USE_MAX
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            if (IsReady)
            {
                MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnBannerLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnBannerLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent -= OnBannerClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerRevenuePaidEvent;

                MaxSdk.HideBanner(adsUnitID);
                isReady = false;
                isShowOnLoaded = false;
                showPosition = string.Empty;
                BannerDestroy();
                OnAdsClosed();
            }
#endif
        }

        private void DelayReloadAd(float time)
        {
            Invoke(nameof(LoadAds), time);
        }

        private void CancelReloadAds()
        {
            CancelInvoke(nameof(LoadAds));
        }
    }
}

#endif
