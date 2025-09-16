using System;
using System.Collections;
using GoogleMobileAds.Api;
using TheLegends.Base.Firebase;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeBannerController : AdmobNativePlatformController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeBanner;
#else
            return AdsType.None;
#endif
        }

        protected override void OnAdsLoadFailed(string message)
        {
#if USE_ADMOB
            base.OnAdsLoadFailed(message);

            if (Status == AdsEvents.LoadNotAvailable)
            {
                DelayReloadAd(30);
            }
#endif
        }
    }
}
