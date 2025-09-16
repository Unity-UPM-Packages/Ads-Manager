using System;
using System.Collections;
using GoogleMobileAds.Api;
using TheLegends.Base.Firebase;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeVideoController : AdmobNativePlatformController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeVideo;
#else
            return AdsType.None;
#endif
        }
    }
}
