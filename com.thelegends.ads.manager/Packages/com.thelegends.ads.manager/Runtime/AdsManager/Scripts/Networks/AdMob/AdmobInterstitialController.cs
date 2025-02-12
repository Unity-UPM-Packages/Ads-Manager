using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using TheLegends.Base.UI;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobInterstitialController : AdsPlacementBase
    {
        private InterstitialAd _interstitialAd;

        public override void LoadAds()
        {
#if USE_ADMOB
            if (!IsCanLoadAds())
            {
                return;
            }

            if (!IsReady)
            {
                if (_interstitialAd != null)
                {
                    try
                    {
                        _interstitialAd.Destroy();
                        _interstitialAd = null;
                    }
                    catch (Exception ex)
                    {
                        AdsManager.Instance.LogException(ex);
                    }
                }

                base.LoadAds();
                AdRequest request = new AdRequest();
                InterstitialAd.Load(adsUnitID.Trim(), request,
                    (InterstitialAd ad, LoadAdError error) =>
                    {
                        // if error is not null, the load request failed.
                        if(error != null)
                        {
                            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "ad failed to load with error : " + error);
                            OnInterLoadFailed(error);
                            return;
                        }

                        if(ad == null)
                        {
                            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                            OnInterLoadFailed(error);
                            return;
                        }

                        AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

                        _interstitialAd = ad;

                        OnAdsLoadAvailable();
                    });
            }
#else

#endif
        }

        public override void ShowAds(string showPosition)
        {
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                _interstitialAd.OnAdClicked += base.OnAdsClick;
                _interstitialAd.OnAdPaid += OnAdsPaid;
                _interstitialAd.OnAdImpressionRecorded += OnImpression;
                _interstitialAd.OnAdFullScreenContentClosed += OnInterClosed;
                _interstitialAd.OnAdFullScreenContentFailed += OnInterShowFailed;
                _interstitialAd.OnAdFullScreenContentOpened += () => base.OnAdsShowSuccess();
                _interstitialAd.Show();
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
            return AdsType.Interstitial;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            if (_interstitialAd != null)
            {
                return _interstitialAd.CanShowAd();
            }

            return false;
#else
            return false;
#endif
        }


        #region Internal

        private void OnInterLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsLoadFailed(errorDescription);
        }

        private void OnInterShowFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsShowFailed(errorDescription);
        }

        private void OnInterClosed()
        {
            UILoadingController.Show(1f, null);
            base.OnAdsClosed();
        }

        private void OnAdsPaid(AdValue value)
        {
            AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
        }

        #endregion




    }

}

