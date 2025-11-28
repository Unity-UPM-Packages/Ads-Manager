#if USE_ADMOB

using System;
using GoogleMobileAds.Api;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobAppOpenController : AdsPlacementBase
    {
        private AppOpenAd _appOpenAd;
        protected Action OnClose;

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
#if USE_ADMOB
            if (!IsCanLoadAds())
            {
                return;
            }

            if (IsReady)
            {
                return;
            }

            base.LoadAds();
            AdRequest request = new AdRequest();

            AppOpenAd.Load(adsUnitID.Trim(), request,
                (AppOpenAd ad, LoadAdError error) =>
                {
                    if (_loadRequestId != _currentLoadRequestId)
                    {
                        // If the load request ID does not match, this callback is from a previous request
                        return;
                    }

                    StopHandleTimeout();

                    // if error is not null, the load request failed.
                    if (error != null)
                    {
                        AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "failed to load with error : " + error);
                        OnAppOpenLoadFailed(error);
                        return;
                    }

                    if (ad == null)
                    {
                        AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                        OnAppOpenLoadFailed(error);
                        return;
                    }

                    AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

                    _appOpenAd = ad;

                    OnAdsLoadAvailable();

                });
        }
#endif

        


        public void ShowAds(string showPosition, Action OnClose = null)
        {
            this.OnClose = OnClose;
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                _appOpenAd.OnAdClicked += OnAppOpenClick;
                _appOpenAd.OnAdPaid += OnAdsPaid;
                _appOpenAd.OnAdImpressionRecorded += OnAppOpenImpression;
                _appOpenAd.OnAdFullScreenContentClosed += OnAppOpenClosed;
                _appOpenAd.OnAdFullScreenContentFailed += OnAppOpenShowFailed;
                _appOpenAd.OnAdFullScreenContentOpened += OnAppOpenShowSuccess;
                _appOpenAd.Show();
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }


        #region Internal

        private void OnAppOpenClick()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnAppOpenShowSuccess()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
                AdsManager.Instance.OnFullScreenAdsShow();
            });
        }

        private void OnAppOpenImpression()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
        }

        private void OnAppOpenLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            OnAdsLoadFailed(errorDescription);
        }

        private void OnAppOpenShowFailed(AdError error)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var errorDescription = error?.GetMessage();
                OnAdsShowFailed(errorDescription);
            });
        }

        private void OnAppOpenClosed()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UILoadingController.Show(1f, () =>
                {
                    OnClose?.Invoke();
                    OnClose = null;

                    _appOpenAd.OnAdClicked -= OnAppOpenClick;
                    _appOpenAd.OnAdPaid -= OnAdsPaid;
                    _appOpenAd.OnAdImpressionRecorded -= OnAppOpenImpression;
                    _appOpenAd.OnAdFullScreenContentClosed -= OnAppOpenClosed;
                    _appOpenAd.OnAdFullScreenContentFailed -= OnAppOpenShowFailed;
                    _appOpenAd.OnAdFullScreenContentOpened -= OnAppOpenShowSuccess;

                    AdsManager.Instance.OnFullScreenAdsClosed();
                });
                OnAdsClosed();
            });
        }

        private void OnAdsPaid(AdValue value)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
            });
        }

        #endregion
    }
}

#endif