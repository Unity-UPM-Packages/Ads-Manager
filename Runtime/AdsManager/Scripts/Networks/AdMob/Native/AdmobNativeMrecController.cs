#if USE_ADMOB

using System;
using System.Collections;
using GoogleMobileAds.Api;
using TheLegends.Base.Firebase;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeMrecController : AdmobNativeBannerController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeMrec;
#else
            return AdsType.None;
#endif
        }

        protected override void OnNativePlatformShow()
        {
#if USE_ADMOB
            OnShow += () => RegisterConfig();
            base.OnNativePlatformShow();
#endif
        }

        protected override void RegisterConfig()
        {
            AdsManager.Instance.RegisterNativeMrecConfig(new NativeShowedConfig
            {
                order = this.Order,
                position = position,
                layoutName = _layoutName,
                countdown = _storedCountdown,
                adsPos = _storedPosition,
                reloadTime = _autoReloadTime,
                showOnLoaded = _isShowOnLoaded
            });
        }
    }
}

#endif
