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
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnClose?.Invoke();
                OnClose = null;
            });
        }
    }
}
