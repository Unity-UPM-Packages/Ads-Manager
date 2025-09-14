// File: DummyNativeClient.cs
// Có thể đặt trong namespace TheLegends.Base.Ads
namespace TheLegends.Base.Ads
{
    using System;
    using UnityEngine;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Common;

    /// <summary>
    /// Một client "giả" không làm gì cả.
    /// Được sử dụng cho các nền tảng không được hỗ trợ (như Editor)
    /// để tránh lỗi NullReferenceException.
    /// </summary>
    internal class DummyNativeClient : IAdmobNativePlatformClient
    {
        public event EventHandler<EventArgs> OnAdLoaded;
        public event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;
        public event Action<AdValue> OnPaidEvent;
        public event EventHandler<EventArgs> OnAdDidRecordImpression;
        public event Action OnAdClicked;
        public event Action OnVideoStart;
        public event Action OnVideoEnd;
        public event EventHandler<bool> OnVideoMute;
        public event Action OnVideoPlay;
        public event Action OnVideoPause;
        public event Action OnAdClosed;
        public event Action OnAdShow;
        public event Action OnAdShowedFullScreenContent;
        public event Action OnAdDismissedFullScreenContent;

        // ... khai báo tất cả các event khác nhưng không bao giờ kích hoạt chúng ...

        public void Initialize()
        {
            Debug.Log("DummyNativeClient: Initialized. (Editor Mode)");
        }

        public void LoadAd(string adUnitId, AdRequest request)
        {
            // string message = "Native Ads are not supported in the Unity Editor.";
            // Debug.LogWarning(message);

            // // Ngay lập tức gọi lại callback thất bại
            // var errorClient = new DummyLoadAdErrorClient(message);
            // var args = new LoadAdErrorClientEventArgs { LoadAdErrorClient = errorClient };
            // OnAdFailedToLoad?.Invoke(this, args);
            OnAdLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void ShowAd(string layoutName)
        {
            OnAdShow?.Invoke();
            DestroyAd();
        }
        public void DestroyAd()
        {
            OnAdDismissedFullScreenContent?.Invoke();
            OnAdClosed?.Invoke();
        }
        public bool IsAdAvailable() => true;
        public IResponseInfoClient GetResponseInfoClient() => null;

        public void WithCountdown(float initialDelaySeconds, float countdownDurationSeconds, float closeButtonDelaySeconds)
        {
            Debug.Log("DummyNativeClient: WithCountdown");
        }

        public void WithAutoReload(string adUnitId, long intervalSeconds)
        {
            Debug.Log("DummyNativeClient: WithAutoReload");
        }

        public void WithShowOnLoaded(bool enabled)
        {
            Debug.Log("DummyNativeClient: WithShowOnLoaded");
        }

        public float GetHeightInPixels()
        {
            Debug.Log("DummyNativeClient: GetHeightInPixels");
            return 0f;
        }

        public float GetWidthInPixels()
        {
            Debug.Log("DummyNativeClient: GetWidthInPixels");
            return 0f;
        }

        public void WithPosition(int positionX, int positionY)
        {
            Debug.Log("DummyNativeClient: WithPosition " + positionX + "x" + positionY);
        }
    }

    /// <summary>
    /// Một ILoadAdErrorClient "giả" để đi kèm với DummyNativeClient.
    /// </summary>
    internal class DummyLoadAdErrorClient : ILoadAdErrorClient
    {
        private readonly string _message;
        public DummyLoadAdErrorClient(string message) { _message = message; }
        public int GetCode() => -1;
        public string GetDomain() => "com.thelegends.base.ads.dummy";
        public string GetMessage() => _message;
        public IAdErrorClient GetCause() => null;
        public IResponseInfoClient GetResponseInfoClient() => null;
    }
}