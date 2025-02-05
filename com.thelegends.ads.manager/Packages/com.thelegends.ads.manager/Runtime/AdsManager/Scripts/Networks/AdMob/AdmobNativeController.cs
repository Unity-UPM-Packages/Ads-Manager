using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using TheLegends.Base.Ads;
using UnityEngine;

namespace com.thelegends.ads.manager
{
    public class AdmobNativeController : AdsPlacementBase
    {
        private NativeAd _nativeAd;

        [SerializeField]
        private PlacementOrder _order = PlacementOrder.One;

        [SerializeField]
        private string positionNative = "default";
        [SerializeField]
        private bool isShowOnLoaded;

        public Action onClick = null;



        private void Awake()
        {
            position = positionNative;

            var platform = Application.platform;
            var isIOS = platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.OSXPlayer;
            var isAdmobTest = AdsManager.Instance.SettingsAds.isAdmobTest;

            var placementIndex = Mathf.Clamp((int)_order, 1, isIOS ? AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.nativeIds.Count : AdsManager.Instance.SettingsAds.ADMOB_Android_Test.nativeIds.Count) - 1;

            placement = isAdmobTest
                ? (isIOS ? AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.nativeIds[placementIndex] : AdsManager.Instance.SettingsAds.ADMOB_Android_Test.nativeIds[placementIndex])
                : (isIOS ? AdsManager.Instance.SettingsAds.ADMOB_IOS.nativeIds[placementIndex] : AdsManager.Instance.SettingsAds.ADMOB_Android.nativeIds[placementIndex]);

            Init(placement);

        }



        public override void LoadAds()
        {
#if USE_ADMOB
            if (!IsCanLoadAds())
            {
                return;
            }

            if (!IsReady)
            {
                NativeDestroy();
                base.LoadAds();
                AdLoader adLoader = new AdLoader.Builder(adsUnitID)
                    .ForNativeAd()
                    .Build();
                adLoader.OnNativeAdLoaded += OnNativeLoaded;
                adLoader.OnAdFailedToLoad += OnNativeLoadFailed;
                adLoader.OnNativeAdImpression += OnImpression;
                adLoader.OnNativeAdClicked += OnAdsClick;
                adLoader.OnNativeAdClosed += OnAdsClose;
                adLoader.LoadAd(new AdRequest());

#if UNITY_EDITOR
                OnNativeLoaded(this, null);
#endif
            }
#endif
        }


        public override void ShowAds(string showPosition)
        {
#if USE_ADMOB
            if (Status == AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "is showing --> return");
                return;
            }

            base.ShowAds(showPosition);

            if (IsReady && Status == AdsEvents.LoadAvailable)
            {
                _nativeAd.OnPaidEvent += OnAdsPaid;
                Status = AdsEvents.ShowSuccess;
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }


        public override AdsNetworks GetAdsNetworks()
        {
#if USE_ADMOB
            return AdsNetworks.Admob;
#else
            return AdsNetworks.None;
#endif
        }

        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.Native;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            return _nativeAd != null;
#else
        return false;
#endif
        }

        #region Internal

        private void NativeDestroy()
        {
#if USE_ADMOB
            if (_nativeAd != null)
            {
                try
                {
                    _nativeAd.Destroy();
                    _nativeAd = null;
                }
                catch (Exception ex)
                {
                    AdsManager.Instance.LogException(ex);
                }
            }
#endif
        }

        private void OnNativeLoaded(object sender, NativeAdEventArgs args)
        {
#if USE_ADMOB
            base.OnAdsLoadAvailable();
            NativeDestroy();

            if (args != null)
            {
                _nativeAd = args.nativeAd;
            }


            if (isShowOnLoaded)
            {
                ShowAds(position);
            }
#endif
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void OnNativeLoadFailed(object sender, AdFailedToLoadEventArgs error)
#pragma warning restore CS0618 // Type or member is obsolete
        {
#if USE_ADMOB
            var errorDescription = error.LoadAdError.GetMessage();
            base.OnAdsLoadFailed(errorDescription);
#endif
        }

        private void OnAdsClose(object sender, EventArgs args)
        {
#if USE_ADMOB
            base.OnAdsClosed();
#endif
        }

        private void OnAdsClick(object sender, EventArgs args)
        {
#if USE_ADMOB
            base.OnAdsClick();
            onClick?.Invoke();
#endif
        }

        private void OnImpression(object sender, EventArgs args)
        {
#if USE_ADMOB
            base.OnImpression();
#endif

        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void OnAdsPaid(object sender, AdValueEventArgs args)
#pragma warning restore CS0618 // Type or member is obsolete
        {
#if USE_ADMOB
            AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, args.AdValue);
#endif
        }

    #endregion
    }
}
