using System;
using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.UI;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class MaxInterstitialController : AdsPlacementBase
    {
        private string _currentLoadRequestId;
        private string _loadRequestId;

        protected Action OnClose;

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
            return AdsType.Interstitial;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_MAX
            return MaxSdk.IsInterstitialReady(adsUnitID);
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

                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

                _currentLoadRequestId = Guid.NewGuid().ToString();
                _loadRequestId = _currentLoadRequestId;

                MaxSdk.LoadInterstitial(adsUnitID);
            }
#endif
        }

        public void ShowAds(string showPosition, Action OnClose = null)
        {
            this.OnClose = OnClose;
            base.ShowAds(showPosition);

#if USE_MAX
            if (IsReady && IsAvailable)
            {
                MaxSdk.ShowInterstitial(adsUnitID);
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

        protected void OnInterstitialLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                OnAdsLoadAvailable();
            });
            
        }

        protected void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo)
        {
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                OnAdsLoadFailed(adInfo.Message);
            });
            
        }

        protected void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsShowSuccess();
            });
        }

        protected void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, info);
            });
            
        }

        protected void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsClick();
            });
            
        }

        protected virtual void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                UILoadingController.Show(1f, () =>
                {
                    OnClose?.Invoke();
                    OnClose = null;
                });

                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialLoadFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnInterstitialRevenuePaidEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialHiddenEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialAdFailedToDisplayEvent;

                OnAdsClosed();
            });
            
        }

        protected void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
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
