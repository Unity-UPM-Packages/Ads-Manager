#if USE_ADMOB

using System;
using GoogleMobileAds.Api;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobRewardedController : AdsPlacementBase
    {
        private RewardedAd _rewardedAd;
        private string _currentLoadRequestId;

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
            return AdsType.Rewarded;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            if (_rewardedAd != null)
            {
                return _rewardedAd.CanShowAd();
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

            if (!IsReady)
            {
                if (_rewardedAd != null)
                {
                    try
                    {
                        _rewardedAd.Destroy();
                        _rewardedAd = null;
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

                RewardedAd.Load(adsUnitID.Trim(), request,
                    (RewardedAd ad, LoadAdError error) =>
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
                            OnRewardedLoadFailed(error);
                            return;
                        }

                        if(ad == null)
                        {
                            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                            OnRewardedLoadFailed(error);
                            return;
                        }

                        AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

                        _rewardedAd = ad;

                        OnAdsLoadAvailable();
                    });
            }
#endif
        }

        public void ShowAds(string showPosition, Action OnRewarded = null)
        {
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                _rewardedAd.OnAdClicked += OnRewardClick;
                _rewardedAd.OnAdPaid += OnRewardPaid;
                _rewardedAd.OnAdImpressionRecorded += OnRewardImpression;
                _rewardedAd.OnAdFullScreenContentClosed += OnRewardedClosed;
                _rewardedAd.OnAdFullScreenContentFailed += OnRewardedShowFailed;
                _rewardedAd.OnAdFullScreenContentOpened += OnRewardShowSuccess;
                _rewardedAd.Show(reward =>
                {
                    if (reward != null)
                    {
                        AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + $"{adsUnitID} " + "claimed");
                        UILoadingController.Show(1f, () =>
                        {
                            OnRewarded?.Invoke();
                        });
                    }
                });
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

        private void OnRewardClick()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnRewardPaid(AdValue value)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsPaid(value);
            });
        }

        private void OnRewardImpression()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
        }

        private void OnRewardShowSuccess()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
            });
        }

        private void OnRewardedLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            OnAdsLoadFailed(errorDescription);
        }

        private void OnRewardedShowFailed(AdError error)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var errorDescription = error?.GetMessage();
                OnAdsShowFailed(errorDescription);
            });
        }

        private void OnRewardedClosed()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
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