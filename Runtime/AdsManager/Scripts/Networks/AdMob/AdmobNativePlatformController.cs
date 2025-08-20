using System;
using System.Collections;
using GoogleMobileAds.Api;
using TheLegends.Base.Firebase;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativePlatformController : AdsPlacementBase
    {
        private AdmobNativePlatform _nativePlatformAd;
        
        private string _currentLoadRequestId;
        private string _layoutName = "default_layout";
        
        private Action OnClose;
        private Action OnShow;

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
            return AdsType.NativePlatform;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            return _nativePlatformAd != null && _nativePlatformAd.IsAdAvailable();
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
                NativePlatformDestroy();

                base.LoadAds();

                AdRequest request = new AdRequest();

                _currentLoadRequestId = Guid.NewGuid().ToString();
                string loadRequestId = _currentLoadRequestId;

                Debug.Log("LoadAds: " + adsUnitID);

                AdmobNativePlatform.Load(adsUnitID.Trim(), request, (native, error) =>
                {
                    PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
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
                            OnNativePlatformLoadFailed(error);
                            return;
                        }

                        if(native == null)
                        {
                            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                            OnNativePlatformLoadFailed(error);
                            return;
                        }

                        AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + native.GetResponseInfo());

                        _nativePlatformAd = native;

                        SetCountdownDuration(AdsManager.Instance.adsConfigs.nativeTimeClose);

                        OnAdsLoadAvailable();

                    });
                });

            }
#endif
        }

        private void ShowAds(string showPosition, Action OnShow = null, Action OnClose = null)
        {
#if USE_ADMOB
            position = showPosition;

            if (Status == AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "is showing --> return");
                return;
            }

            this.OnClose = OnClose;
            this.OnShow = OnShow;
            base.ShowAds(showPosition);

            if (IsReady && IsAvailable)
            {
                RegisterAdEvents();
                _nativePlatformAd.Show(_layoutName);
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }

        public void ShowAds(string showPosition, string layoutName, Action OnShow = null, Action OnClose = null)
        {
            _layoutName = layoutName;
            ShowAds(showPosition, OnShow, OnClose);
        }

        public void SetLayoutName(string layoutName)
        {
            _layoutName = layoutName;
        }

        public void HideAds()
        {
            if (Status != AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            NativePlatformDestroy();
            OnNativePlatformClosed();
        }

        #region Internal

        private void SetCountdownDuration(float seconds)
        {
#if USE_ADMOB
            _nativePlatformAd?.SetCountdownDuration(seconds);
#endif
        }

        private void NativePlatformDestroy()
        {
#if USE_ADMOB
            try
            {
                if (_nativePlatformAd != null)
                {
                    UnregisterAdEvents();
                    _nativePlatformAd.Destroy();
                    _nativePlatformAd = null;
                }
            }
            catch (Exception ex)
            {
                AdsManager.Instance.LogException(ex);
            }
#endif
        }

        private void OnNativePlatformLoadFailed(LoadAdError error)
        {
#if USE_ADMOB
            var errorDescription = error?.GetMessage();
            OnAdsLoadFailed(errorDescription);
#endif
        }

        private void RegisterAdEvents()
        {
#if USE_ADMOB
            if (_nativePlatformAd == null) return;

            _nativePlatformAd.OnAdPaid += OnAdsPaid;
            _nativePlatformAd.OnAdClicked += OnNativePlatformClick;
            _nativePlatformAd.OnAdDidRecordImpression += OnNativePlatformImpression;
            _nativePlatformAd.OnVideoStart += OnVideoStart;
            _nativePlatformAd.OnVideoEnd += OnVideoEnd;
            _nativePlatformAd.OnVideoMute += OnVideoMute;
            _nativePlatformAd.OnVideoPlay += OnVideoPlay;
            _nativePlatformAd.OnVideoPause += OnVideoPause;
            _nativePlatformAd.OnAdClosed += OnNativePlatformClosed;
            _nativePlatformAd.OnAdShow += OnNativePlatformShow;
#endif
        }

        private void UnregisterAdEvents()
        {
#if USE_ADMOB
            if (_nativePlatformAd == null) return;

            _nativePlatformAd.OnAdPaid -= OnAdsPaid;
            _nativePlatformAd.OnAdClicked -= OnNativePlatformClick;
            _nativePlatformAd.OnAdDidRecordImpression -= OnNativePlatformImpression;
            _nativePlatformAd.OnVideoStart -= OnVideoStart;
            _nativePlatformAd.OnVideoEnd -= OnVideoEnd;
            _nativePlatformAd.OnVideoMute -= OnVideoMute;
            _nativePlatformAd.OnVideoPlay -= OnVideoPlay;
            _nativePlatformAd.OnVideoPause -= OnVideoPause;
            _nativePlatformAd.OnAdClosed -= OnNativePlatformClosed;
            _nativePlatformAd.OnAdShow -= OnNativePlatformShow;
#endif
        }

        private void OnAdsPaid(AdValue adValue)
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, adValue);
            });
#endif
        }

        private void OnNativePlatformClick()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
#endif
        }

        private void OnNativePlatformImpression(object sender, EventArgs args)
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
#endif
        }

        private void OnVideoStart()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} Video started");
#endif
        }

        private void OnVideoEnd()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} Video ended");
#endif
        }

        private void OnVideoMute(object sender, bool isMuted)
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} Video mute state: {isMuted}");
#endif
        }

        private void OnVideoPlay()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} Video playing");
#endif
        }

        private void OnVideoPause()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} Video paused");
#endif
        }

        private void OnNativePlatformClosed()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClosed();
                OnClose?.Invoke();
            });
        }

        private void OnNativePlatformShow()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
                OnShow?.Invoke();
            });
        }

        #endregion


        private void OnDestroy()
        {
            NativePlatformDestroy();
        }
    }
}
