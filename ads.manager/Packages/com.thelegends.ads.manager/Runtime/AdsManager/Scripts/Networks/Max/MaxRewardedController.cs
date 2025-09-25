#if USE_MAX
using System;
using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.UI;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class MaxRewardedController : AdsPlacementBase
    {
        private string _currentLoadRequestId;
        private string _loadRequestId;
        private Action OnRewarded;

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
            return AdsType.Rewarded;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_MAX
            return MaxSdk.IsRewardedAdReady(adsUnitID);
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

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedHiddenEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;

                _currentLoadRequestId = Guid.NewGuid().ToString();
                _loadRequestId = _currentLoadRequestId;

                MaxSdk.LoadRewardedAd(adsUnitID);
            }
#endif
        }

        public void ShowAds(string showPosition, Action OnRewarded = null)
        {
            this.OnRewarded = OnRewarded;
            base.ShowAds(showPosition);
#if USE_MAX
            if (IsReady && IsAvailable)
            {
                MaxSdk.ShowRewardedAd(adsUnitID);
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

        private void OnRewardedLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                OnAdsLoadAvailable();
            });
        }

        private void OnRewardedDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsShowSuccess();
            });
        }

        private void OnRewardedRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, adInfo);
            });
        }

        private void OnRewardedLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                if (_loadRequestId != _currentLoadRequestId) return;

                StopHandleTimeout();

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedHiddenEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnAdReceivedRewardEvent;

                OnAdsLoadFailed(errorInfo.Message);
            });
        }

        private void OnRewardedClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsClick();
            });
        }

        private void OnRewardedHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedHiddenEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnAdReceivedRewardEvent;
                
                OnAdsClosed();
            });
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        { 
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                OnAdsShowFailed(errorInfo.Message);
            });
        }

        private void OnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        { 
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (adUnitId != adsUnitID) return;

                UILoadingController.Show(1f, () =>
                {
                    OnRewarded?.Invoke();
                });
            });
        }

        #endregion
    }
}

#endif