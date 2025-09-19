using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class MaxInterstitialOpenController : MaxInterstitialController
    {
        public override AdsType GetAdsType()
        {
#if USE_MAX
            return AdsType.InterOpen;
#else
            return AdsType.None;
#endif
        }

        protected override void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != adsUnitID) return;
            
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnClose?.Invoke();
                OnClose = null;
                Status = AdsEvents.Close;

                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialLoadFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnInterstitialRevenuePaidEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialHiddenEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialAdFailedToDisplayEvent;
            });
        }
    }
}
