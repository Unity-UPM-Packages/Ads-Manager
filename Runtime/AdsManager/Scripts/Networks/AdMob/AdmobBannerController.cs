using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobBannerController : AdsPlacementBase
    {
        protected BannerView _bannerView;
        private string _currentLoadRequestId;
        private string _loadRequestId;
        private bool isShowing = false;

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

                _bannerView.OnAdClicked += base.OnAdsClick;
                _bannerView.OnAdPaid += OnAdsPaid;
                _bannerView.OnAdImpressionRecorded += OnImpression;
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
                _bannerView.Show();
                Status = AdsEvents.ShowSuccess;
                isShowing = true;
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }

        private void OnAdsPaid(AdValue value)
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
            });

        }

        public virtual void HideAds()
        {
#if USE_ADMOB
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            if (_bannerView != null)
            {
                isShowing = false;
                _bannerView.Hide();
                BannerDestroy();
                base.OnAdsClosed();
            }
#endif
        }

        #region Internal

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

        public void OnBannerLoaded()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (_loadRequestId != _currentLoadRequestId)
                {
                    // If the load request ID does not match, this callback is from a previous request
                    return;
                }

                if (loadTimeOutCoroutine != null)
                {
                    StopCoroutine(loadTimeOutCoroutine);
                    loadTimeOutCoroutine = null;
                }

                base.OnAdsLoadAvailable();

                if (!isShowing)
                {
                    _bannerView.Hide();
                }

            });

        }

        private void OnBannerLoadFailed(AdError error)
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (_loadRequestId != _currentLoadRequestId)
                {
                    // If the load request ID does not match, this callback is from a previous request
                    return;
                }

                if (loadTimeOutCoroutine != null)
                {
                    StopCoroutine(loadTimeOutCoroutine);
                    loadTimeOutCoroutine = null;
                }

                var errorDescription = error?.GetMessage();
                base.OnAdsLoadFailed(errorDescription);
            });

        }

        protected void BannerDestroy()
        {
#if USE_ADMOB
            if (_bannerView != null)
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
