#if USE_ADMOB

using System;
using System.Collections;
using GoogleMobileAds.Api;
using TheLegends.Base.Firebase;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeMrecOpenController : AdmobNativePlatformController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeMrecOpen;
#else
            return AdsType.None;
#endif
        }

        public override void OnAdsClosed()
        {
            Status = AdsEvents.Close;
            adsUnitIDIndex = 0;
        }
    }
}

#endif
