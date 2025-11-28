#if USE_ADMOB
using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace TheLegends.Base.Ads
{
    public class AdmobNativePlatform
    {
        private readonly IAdmobNativePlatformClient _client;

        private AdmobNativePlatform(IAdmobNativePlatformClient client)
        {
            _client = client;
            RegisterAdEvents();
        }

        public event Action<AdValue> OnAdPaid;
        public event Action OnAdClicked;
        public event EventHandler<EventArgs> OnAdDidRecordImpression;
        public event Action OnVideoStart;
        public event Action OnVideoEnd;
        public event EventHandler<bool> OnVideoMute;
        public event Action OnVideoPlay;
        public event Action OnVideoPause;
        public event Action OnAdClosed;
        public event Action OnAdShow;
        public event Action OnAdShowedFullScreenContent;
        public event Action OnAdDismissedFullScreenContent;

        public static void Load(string adUnitId, AdRequest request, Action<AdmobNativePlatform, LoadAdError> adLoadCallback)
        {
            if (adLoadCallback == null)
            {
                Debug.LogError("adLoadCallback cannot be null.");
                return;
            }

            IAdmobNativePlatformClient client;

#if UNITY_ANDROID && !UNITY_EDITOR
            client = new AdmobNativePlatformAndroidClient();
#elif UNITY_IOS && !UNITY_EDITOR
            client = new AdmobNativePlatformIOSClient();
#else
            client = new DummyNativeClient();
#endif


            client.Initialize();

            client.OnAdLoaded += (sender, args) =>
            {
                adLoadCallback(new AdmobNativePlatform(client), null);
            };

            client.OnAdFailedToLoad += (sender, args) =>
            {
                var nativeError = new LoadAdError(args.LoadAdErrorClient);
                adLoadCallback(null, nativeError);
            };

            client.LoadAd(adUnitId, request);
        }

        public void Show(string layoutName) => _client?.ShowAd(layoutName);
        public void Destroy()
        {
            if (_client == null) return;
            UnregisterAdEvents();
            _client.DestroyAd();
        }
        public bool IsAdAvailable() => _client != null && _client.IsAdAvailable();

        #region Builder Pattern Support - Forward to Native

        public void WithCountdown(float initialDelaySeconds, float countdownDurationSeconds, float closeButtonDelaySeconds)
        {
            _client?.WithCountdown(initialDelaySeconds, countdownDurationSeconds, closeButtonDelaySeconds);
        }

        public void WithPosition(int positionX, int positionY)
        {
            _client?.WithPosition(positionX, positionY);
        }

        #endregion

        public IResponseInfoClient GetResponseInfo()
        {
            return _client.GetResponseInfoClient();
        }


        private void RegisterAdEvents()
        {
            if (_client == null) return;
            _client.OnPaidEvent += HandlePaidEvent;
            _client.OnAdClicked += HandleAdClicked;
            _client.OnAdDidRecordImpression += HandleAdDidRecordImpression;
            _client.OnVideoStart += HandleVideoStart;
            _client.OnVideoEnd += HandleVideoEnd;
            _client.OnVideoMute += HandleVideoMute;
            _client.OnVideoPlay += HandleVideoPlay;
            _client.OnVideoPause += HandleVideoPause;
            _client.OnAdClosed += HandleAdClosed;
            _client.OnAdShow += HandleAdShow;
            _client.OnAdShowedFullScreenContent += HandleAdShowedFullScreenContent;
            _client.OnAdDismissedFullScreenContent += HandleAdDismissedFullScreenContent;
        }

        private void UnregisterAdEvents()
        {
            if (_client == null) return;
            _client.OnPaidEvent -= HandlePaidEvent;
            _client.OnAdClicked -= HandleAdClicked;
            _client.OnAdDidRecordImpression -= HandleAdDidRecordImpression;
            _client.OnVideoStart -= HandleVideoStart;
            _client.OnVideoEnd -= HandleVideoEnd;
            _client.OnVideoMute -= HandleVideoMute;
            _client.OnVideoPlay -= HandleVideoPlay;
            _client.OnVideoPause -= HandleVideoPause;
            _client.OnAdClosed -= HandleAdClosed;
            _client.OnAdShow -= HandleAdShow;
            _client.OnAdShowedFullScreenContent -= HandleAdShowedFullScreenContent;
            _client.OnAdDismissedFullScreenContent -= HandleAdDismissedFullScreenContent;
        }


        private void HandlePaidEvent(AdValue adValue) => OnAdPaid?.Invoke(adValue);
        private void HandleAdClicked() => OnAdClicked?.Invoke();
        private void HandleAdDidRecordImpression(object sender, EventArgs args) => OnAdDidRecordImpression?.Invoke(this, args);
        private void HandleVideoStart() => OnVideoStart?.Invoke();
        private void HandleVideoEnd() => OnVideoEnd?.Invoke();
        private void HandleVideoMute(object sender, bool isMuted) => OnVideoMute?.Invoke(this, isMuted);
        private void HandleVideoPlay() => OnVideoPlay?.Invoke();
        private void HandleVideoPause() => OnVideoPause?.Invoke();
        private void HandleAdClosed() => OnAdClosed?.Invoke();
        private void HandleAdShow() => OnAdShow?.Invoke();
        private void HandleAdShowedFullScreenContent() => OnAdShowedFullScreenContent?.Invoke();
        private void HandleAdDismissedFullScreenContent() => OnAdDismissedFullScreenContent?.Invoke();

    }
}

#endif