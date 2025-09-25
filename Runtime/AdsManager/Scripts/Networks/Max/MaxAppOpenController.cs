#if USE_MAX

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TheLegends.Base.Ads
{
    public class MaxAppOpenController : AdsPlacementBase
    {
        private string _currentLoadRequestId;
        private string _loadRequestId;

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
            return AdsType.AppOpen;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_MAX
            return MaxSdk.IsAppOpenAdReady(adsUnitID);
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

                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoadedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenLoadFailedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenDisplayedEvent;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAppOpenRevenuePaidEvent;
                MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAppOpenClickedEvent;
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenHiddenEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenDisplayFailedEvent;

                _currentLoadRequestId = Guid.NewGuid().ToString();
                _loadRequestId = _currentLoadRequestId;

                MaxSdk.LoadAppOpenAd(adsUnitID);
            }

#endif
        }

        public override void ShowAds(string showPosition)
        {
            base.ShowAds(showPosition);
#if USE_MAX
            if (IsReady && IsAvailable)
            {
                MaxSdk.ShowAppOpenAd(adsUnitID);
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }

        #region Internal

        private void OnAppOpenLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                OnAdsLoadAvailable();
            });
        }

        private void OnAppOpenLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent -= OnAppOpenLoadedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent -= OnAppOpenLoadFailedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent -= OnAppOpenDisplayedEvent;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent -= OnAppOpenRevenuePaidEvent;
                MaxSdkCallbacks.AppOpen.OnAdClickedEvent -= OnAppOpenClickedEvent;
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent -= OnAppOpenHiddenEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent -= OnAppOpenDisplayFailedEvent;

                OnAdsLoadFailed(errorInfo.Message);
            });
        }

        private void OnAppOpenDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsShowSuccess();
            });
        }

        private void OnAppOpenRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, adInfo);
            });
        }

        private void OnAppOpenClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {   
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsClick();
            });
        }

        private void OnAppOpenHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if USE_MAX
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent -= OnAppOpenLoadedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent -= OnAppOpenLoadFailedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent -= OnAppOpenDisplayedEvent;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent -= OnAppOpenRevenuePaidEvent;
                MaxSdkCallbacks.AppOpen.OnAdClickedEvent -= OnAppOpenClickedEvent;
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent -= OnAppOpenHiddenEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent -= OnAppOpenDisplayFailedEvent;

                OnAdsClosed();
            });
#endif
        }

        private void OnAppOpenDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        { 
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsShowFailed(errorInfo.Message);
            });
        }

#endregion
    }
}

#endif