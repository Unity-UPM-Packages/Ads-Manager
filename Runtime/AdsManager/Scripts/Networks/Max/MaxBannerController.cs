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

        private bool isReady = false;
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
#if USE_MAX
            if (IsReady && IsAvailable)
            {
                Status = AdsEvents.ShowSuccess;
                MaxSdk.ShowBanner(adsUnitID);
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

            if (AdsManager.Instance.SettingsAds.fixBannerSmallSize)
            {
                MaxSdk.CreateBanner(adsUnitID, adPosition);
                MaxSdk.SetBannerBackgroundColor(adsUnitID, Color.black);
            }
#endif
        }

        #region Internal

        private void OnBannerLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                isReady = true;
                OnAdsLoadAvailable();
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
                BannerDestroy();
                OnAdsClosed();
            }
#endif
        }
    }
}

#endif
