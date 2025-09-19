using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class MaxAppOpenController : AdsPlacementBase
    {
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
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsLoadAvailable();
            });
        }

        private void OnAppOpenLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsLoadFailed(errorInfo.Message);
            });
        }

        private void OnAppOpenDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
            });
        }

        private void OnAppOpenRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, adInfo);
            });
        }

        private void OnAppOpenClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnAppOpenHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {

                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent -= OnAppOpenLoadedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent -= OnAppOpenLoadFailedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent -= OnAppOpenDisplayedEvent;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent -= OnAppOpenRevenuePaidEvent;
                MaxSdkCallbacks.AppOpen.OnAdClickedEvent -= OnAppOpenClickedEvent;
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent -= OnAppOpenHiddenEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent -= OnAppOpenDisplayFailedEvent;

                OnAdsClosed();
            });
        }

        private void OnAppOpenDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowFailed(errorInfo.Message);
            });
        }

        #endregion
    }
}
