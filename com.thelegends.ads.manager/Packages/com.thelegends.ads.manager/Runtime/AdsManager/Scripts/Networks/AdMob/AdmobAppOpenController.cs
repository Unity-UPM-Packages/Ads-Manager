using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobAppOpenController : AdsPlacementBase
    {
        private AppOpenAd _appOpenAd;

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
            return AdsType.AppOpen;
#else
        return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            if (_appOpenAd != null)
            {
                return _appOpenAd.CanShowAd();
            }

            return false;
#else
        return false;
#endif
        }

        public override void LoadAds()
        {
#if true
            if(!IsCanLoadAds())
            {
                return;
            }

            if (!IsReady)
            {
                if (_appOpenAd != null)
                {
                    try
                    {
                        _appOpenAd.Destroy();
                        _appOpenAd = null;
                    }
                    catch (Exception ex)
                    {
                        AdsManager.LogException(ex);
                    }
                }

                base.LoadAds();
                AdRequest request = new AdRequest();
                AppOpenAd.Load(adsUnitID.Trim(), request,
                    (AppOpenAd ad, LoadAdError error) =>
                    {
                        // if error is not null, the load request failed.
                        if(error != null)
                        {
                            AdsManager.LogError($"{AdsNetworks}_{AdsType} " + "failed to load with error : " + error);
                            OnAppOpenLoadFailed(error);
                            return;
                        }

                        if(ad == null)
                        {
                            AdsManager.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                            OnAppOpenLoadFailed(error);
                            return;
                        }

                        AdsManager.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

                        _appOpenAd = ad;

                        OnAdsLoadAvailable();
                    });
            }
#endif

        }


        public override void ShowAds(string showPosition)
        {
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && Status == AdsEvents.LoadAvailable)
            {
                _appOpenAd.OnAdClicked += base.OnAdsClick;
                _appOpenAd.OnAdPaid += OnAdsPaid;
                _appOpenAd.OnAdImpressionRecorded += OnImpression;
                _appOpenAd.OnAdFullScreenContentClosed += OnAppOpenClosed;
                _appOpenAd.OnAdFullScreenContentFailed += OnAppOpenShowFailed;
                _appOpenAd.OnAdFullScreenContentOpened += () => base.OnAdsShowSuccess();
                _appOpenAd.Show();
            }
            else
            {
                AdsManager.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }


        #region Internal

        private void OnAppOpenLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsLoadFailed(errorDescription);
        }

        private void OnAppOpenShowFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsShowFailed(errorDescription);
        }

        private void OnAppOpenClosed()
        {
            base.OnAdsClosed();
        }

        private void OnAdsPaid(AdValue value)
        {
            AdsManager.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
        }

        #endregion
    }
}
