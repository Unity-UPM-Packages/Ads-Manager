#if USE_ADMOB
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobBannerController : AdsPlacementBase
    {
        protected BannerView _bannerView;
        private string _currentLoadRequestId;
        private string _loadRequestId;

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
            return AdsType.Banner;
#else
        return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            return _bannerView != null;
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

            BannerDestroy();

            if (!IsReady)
            {
                CreateBanner();

                _bannerView.OnAdClicked += OnBannerClick;
                _bannerView.OnAdPaid += OnBannerPaid;
                _bannerView.OnAdImpressionRecorded += OnBannerImpression;
                _bannerView.OnBannerAdLoadFailed += OnBannerLoadFailed;
                _bannerView.OnBannerAdLoaded += OnBannerLoaded;

                base.LoadAds();
                AdRequest request = new AdRequest();

                _currentLoadRequestId = Guid.NewGuid().ToString();
                _loadRequestId = _currentLoadRequestId;

                _bannerView.LoadAd(request);
            }
#endif
        }

        public override void ShowAds(string showPosition)
        {
            if (Status == AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "is showing --> return");
                return;
            }

            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                PreShow();
                _bannerView.Show();
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


        public virtual void HideAds()
        {
#if USE_ADMOB
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            if (IsReady)
            {
                _bannerView.Hide();
                BannerDestroy();
                OnAdsClosed();
            }
#endif
        }

        #region Internal

        protected virtual void PreShow()
        {

        }

        protected virtual void CreateBanner()
        {
#if USE_ADMOB
            AdPosition adPosition = AdsManager.Instance.SettingsAds.bannerPosition == BannerPos.Top
                ? AdPosition.Top
                : AdPosition.Bottom;

            if (AdsManager.Instance.SettingsAds.fixBannerSmallSize)
            {
                _bannerView = new BannerView(adsUnitID.Trim(), AdSize.Banner, adPosition);
            }
            else
            {
                AdSize adaptiveSize =
                    AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                _bannerView = new BannerView(adsUnitID.Trim(), adaptiveSize, adPosition);
            }
#endif
        }

        private void OnBannerClick()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnBannerPaid(AdValue value)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
            });
        }

        private void OnBannerImpression()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
        }

        public void OnBannerLoaded()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (_loadRequestId != _currentLoadRequestId)
                {
                    // If the load request ID does not match, this callback is from a previous request
                    return;
                }

                _bannerView.Hide();

                StopHandleTimeout();

                OnAdsLoadAvailable();
            });

        }

        private void OnBannerLoadFailed(AdError error)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (_loadRequestId != _currentLoadRequestId)
                {
                    // If the load request ID does not match, this callback is from a previous request
                    return;
                }

                StopHandleTimeout();

                var errorDescription = error?.GetMessage();
                OnAdsLoadFailed(errorDescription);
            });

        }

        protected void BannerDestroy()
        {
#if USE_ADMOB
            if (IsReady)
            {
                try
                {
                    _bannerView.Destroy();
                    _bannerView = null;
                }
                catch (Exception ex)
                {
                    AdsManager.Instance.LogException(ex);
                }
            }
#endif
        }
        #endregion
    }
}

#endif