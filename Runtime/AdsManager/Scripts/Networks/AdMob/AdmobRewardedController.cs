using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobRewardedController : AdsPlacementBase
    {
        private RewardedAd _rewardedAd;

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
            if(!IsCanLoadAds())
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
                        AdsManager.LogException(ex);
                    }
                }

                base.LoadAds();
                AdRequest request = new AdRequest();
                RewardedAd.Load(adsUnitID.Trim(), request,
                    (RewardedAd ad, LoadAdError error) =>
                    {
                        // if error is not null, the load request failed.
                        if(error != null)
                        {
                            AdsManager.LogError($"{AdsNetworks}_{AdsType} " + "ad failed to load with error : " + error);
                            OnRewardedLoadFailed(error);
                            return;
                        }

                        if(ad == null)
                        {
                            AdsManager.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                            OnRewardedLoadFailed(error);
                            return;
                        }

                        AdsManager.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

                        _rewardedAd = ad;

                        OnAdsLoadAvailable();
                    });
            }
#endif
        }

        public void ShowAds(Action OnRewarded, string showPosition)
        {
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && Status == AdsEvents.LoadAvailable)
            {
                _rewardedAd.OnAdClicked += base.OnAdsClick;
                _rewardedAd.OnAdPaid += OnAdsPaid;
                _rewardedAd.OnAdImpressionRecorded += OnImpression;
                _rewardedAd.OnAdFullScreenContentClosed += OnRewardedClosed;
                _rewardedAd.OnAdFullScreenContentFailed += OnRewardedShowFailed;
                _rewardedAd.OnAdFullScreenContentOpened += () => base.OnAdsShowSuccess();
                _rewardedAd.Show(reward =>
                {
                    if (reward != null)
                    {
                        AdsManager.Log($"{AdsNetworks}_{AdsType} " + $"{adsUnitID} " + "claimed");
                        OnRewarded?.Invoke();
                    }
                });
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

        private void OnRewardedLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsLoadFailed(errorDescription);
        }

        private void OnRewardedShowFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsShowFailed(errorDescription);
        }

        private void OnRewardedClosed()
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

