using System;
using Baracuda.Threading;
using GoogleMobileAds.Api;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobInterstitialController : AdsPlacementBase
    {
        private InterstitialAd _interstitialAd;
        private string _currentLoadRequestId;

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

                _currentLoadRequestId = Guid.NewGuid().ToString();
                string loadRequestId = _currentLoadRequestId;

                InterstitialAd.Load(adsUnitID.Trim(), request,
                    (InterstitialAd ad, LoadAdError error) =>
                    {

                        if (loadRequestId != _currentLoadRequestId)
                        {
                            // If the load request ID does not match, this callback is from a previous request
                            return;
                        }

                        StopHandleTimeout();

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
                _interstitialAd.OnAdClicked += OnInterClick;
                _interstitialAd.OnAdPaid += OnInterPaid;
                _interstitialAd.OnAdImpressionRecorded += OnInterImpression;
                _interstitialAd.OnAdFullScreenContentClosed += OnInterClosed;
                _interstitialAd.OnAdFullScreenContentFailed += OnInterShowFailed;
                _interstitialAd.OnAdFullScreenContentOpened += OnInterShowSuccess;
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

        private void OnInterClick()
        {
            Dispatcher.Invoke(() =>
            {
                OnAdsClick();
            });
        }

        private void OnInterImpression()
        {
            Dispatcher.Invoke(() =>
            {
                OnImpression();
            });
        }

        private void OnInterShowSuccess()
        {
            Dispatcher.Invoke(() =>
            {
                OnAdsShowSuccess();
            });
        }

        private void OnInterLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            OnAdsLoadFailed(errorDescription);
        }

        private void OnInterShowFailed(AdError error)
        {
            Dispatcher.Invoke(() =>
            {
                var errorDescription = error?.GetMessage();
                OnAdsShowFailed(errorDescription);
            });
        }

        protected virtual void OnInterClosed()
        {
            Dispatcher.Invoke(() =>
            {
                UILoadingController.Show(1f, null);
                OnAdsClosed();
            });
        }

        private void OnInterPaid(AdValue value)
        {
            Dispatcher.Invoke(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
            });

        }

        #endregion




    }

}

