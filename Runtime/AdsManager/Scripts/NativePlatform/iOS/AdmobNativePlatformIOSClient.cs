#if USE_ADMOB && UNITY_IOS && !UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace TheLegends.Base.Ads
{
    public class AdmobNativePlatformIOSClient : IAdmobNativePlatformClient
    {
        // === Events của Interface ===
        public event EventHandler<EventArgs> OnAdLoaded;
        public event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;
        public event Action OnAdClosed;
        public event Action OnAdShow;
        public event Action<AdValue> OnPaidEvent;
        public event EventHandler<EventArgs> OnAdDidRecordImpression;
        public event Action OnAdClicked;
        public event Action OnVideoStart;
        public event Action OnVideoEnd;
        public event EventHandler<bool> OnVideoMute;
        public event Action OnVideoPlay;
        public event Action OnVideoPause;
        public event Action OnAdShowedFullScreenContent;
        public event Action OnAdDismissedFullScreenContent;

        private IntPtr _nativeControllerPtr;

        // MARK: - DllImport Declarations

        [DllImport("__Internal")]
        private static extern IntPtr AdmobNative_Create();

        [DllImport("__Internal")]
        private static extern void AdmobNative_Destroy(IntPtr handle);

        [DllImport("__Internal")]
        private static extern void AdmobNative_RegisterCallbacks(
            IntPtr handle,
            VoidCallback onAdLoaded,
            ErrorCallback onAdFailedToLoad,
            VoidCallback onAdShow,
            VoidCallback onAdClosed,
            PaidEventCallback onPaidEvent,
            VoidCallback onAdDidRecordImpression,
            VoidCallback onAdClicked,
            VoidCallback onVideoStart,
            VoidCallback onVideoEnd,
            VideoMuteCallback onVideoMute,
            VoidCallback onVideoPlay,
            VoidCallback onVideoPause,
            VoidCallback onAdShowedFullScreenContent,
            VoidCallback onAdDismissedFullScreenContent
        );

        [DllImport("__Internal")]
        private static extern void AdmobNative_LoadAd(IntPtr handle, string adUnitId);

        [DllImport("__Internal")]
        private static extern void AdmobNative_ShowAd(IntPtr handle, string layoutName);

        [DllImport("__Internal")]
        private static extern void AdmobNative_DestroyAd(IntPtr handle);

        [DllImport("__Internal")]
        private static extern bool AdmobNative_IsAdAvailable(IntPtr handle);

        [DllImport("__Internal")]
        private static extern void AdmobNative_WithCountdown(IntPtr handle, float initial, float duration, float closeDelay);

        [DllImport("__Internal")]
        private static extern void AdmobNative_WithPosition(IntPtr handle, int x, int y);

        [DllImport("__Internal")]
        private static extern float AdmobNative_GetWidthInPixels(IntPtr handle);

        [DllImport("__Internal")]
        private static extern float AdmobNative_GetHeightInPixels(IntPtr handle);

        // MARK: - Callback Delegates

        private delegate void VoidCallback();
        private delegate void ErrorCallback(string errorMessage);
        private delegate void PaidEventCallback(int precisionType, long valueMicros, string currencyCode);
        private delegate void VideoMuteCallback(bool isMuted);

        // Static instance để giữ reference tránh GC
        private static AdmobNativePlatformIOSClient _instance;

        // MARK: - Constructor & Initialization

        public AdmobNativePlatformIOSClient()
        {
            _instance = this;
        }

        public void Initialize()
        {
            Debug.Log("AdmobNativePlatformIOSClient: Initializing...");

            // Create native controller
            _nativeControllerPtr = AdmobNative_Create();

            if (_nativeControllerPtr == IntPtr.Zero)
            {
                Debug.LogError("AdmobNativePlatformIOSClient: Failed to create native controller");
                return;
            }

            // Register callbacks
            AdmobNative_RegisterCallbacks(
                _nativeControllerPtr,
                OnAdLoadedCallback,
                OnAdFailedToLoadCallback,
                OnAdShowCallback,
                OnAdClosedCallback,
                OnPaidEventCallback,
                OnAdDidRecordImpressionCallback,
                OnAdClickedCallback,
                OnVideoStartCallback,
                OnVideoEndCallback,
                OnVideoMuteCallback,
                OnVideoPlayCallback,
                OnVideoPauseCallback,
                OnAdShowedFullScreenContentCallback,
                OnAdDismissedFullScreenContentCallback
            );

            Debug.Log("AdmobNativePlatformIOSClient: Initialized successfully");
        }

        // MARK: - Interface Implementation

        public void LoadAd(string adUnitId, AdRequest request)
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                Debug.LogError("AdmobNativePlatformIOSClient: Controller not initialized");
                return;
            }

            Debug.Log($"AdmobNativePlatformIOSClient: Loading ad with ID: {adUnitId}");
            AdmobNative_LoadAd(_nativeControllerPtr, adUnitId);
        }

        public void ShowAd(string layoutName)
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                Debug.LogError("AdmobNativePlatformIOSClient: Controller not initialized");
                return;
            }

            Debug.Log($"AdmobNativePlatformIOSClient: Showing ad with layout: {layoutName}");
            AdmobNative_ShowAd(_nativeControllerPtr, layoutName);
        }

        public void DestroyAd()
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                return;
            }

            Debug.Log("AdmobNativePlatformIOSClient: Destroying ad");
            AdmobNative_DestroyAd(_nativeControllerPtr);
        }

        public bool IsAdAvailable()
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                return false;
            }

            return AdmobNative_IsAdAvailable(_nativeControllerPtr);
        }

        public IResponseInfoClient GetResponseInfoClient()
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                return null;
            }

            return new NativeResponseInfoIOSClient(_nativeControllerPtr);
        }

        // MARK: - Builder Pattern Support

        public void WithCountdown(float initialDelaySeconds, float countdownDurationSeconds, float closeButtonDelaySeconds)
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                Debug.LogError("AdmobNativePlatformIOSClient: Controller not initialized");
                return;
            }

            Debug.Log($"AdmobNativePlatformIOSClient: WithCountdown({initialDelaySeconds}, {countdownDurationSeconds}, {closeButtonDelaySeconds})");
            AdmobNative_WithCountdown(_nativeControllerPtr, initialDelaySeconds, countdownDurationSeconds, closeButtonDelaySeconds);
        }

        public void WithPosition(int positionX, int positionY)
        {
            if (_nativeControllerPtr == IntPtr.Zero)
            {
                Debug.LogError("AdmobNativePlatformIOSClient: Controller not initialized");
                return;
            }

            Debug.Log($"AdmobNativePlatformIOSClient: WithPosition({positionX}, {positionY})");
            AdmobNative_WithPosition(_nativeControllerPtr, positionX, positionY);
        }

        // MARK: - MonoPInvokeCallback Methods (Static callbacks từ native)

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdLoadedCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdLoaded callback");
                _instance?.OnAdLoaded?.Invoke(_instance, EventArgs.Empty);
            });
        }

        [MonoPInvokeCallback(typeof(ErrorCallback))]
        private static void OnAdFailedToLoadCallback(string errorMessage)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.LogError($"AdmobNativePlatformIOSClient: OnAdFailedToLoad - {errorMessage}");

                var errorClient = new AdmobNativePlatformIOSAdErrorClient(errorMessage);
                var args = new LoadAdErrorClientEventArgs
                {
                    LoadAdErrorClient = errorClient
                };

                _instance?.OnAdFailedToLoad?.Invoke(_instance, args);
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdShowCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdShow callback");
                _instance?.OnAdShow?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdClosedCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdClosed callback");
                _instance?.OnAdClosed?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(PaidEventCallback))]
        private static void OnPaidEventCallback(int precisionType, long valueMicros, string currencyCode)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"AdmobNativePlatformIOSClient: OnPaidEvent - {valueMicros} {currencyCode} (precision: {precisionType})");

                var adValue = new AdValue
                {
                    Precision = (AdValue.PrecisionType)precisionType,
                    Value = valueMicros,
                    CurrencyCode = currencyCode
                };

                _instance?.OnPaidEvent?.Invoke(adValue);
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdDidRecordImpressionCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdDidRecordImpression callback");
                _instance?.OnAdDidRecordImpression?.Invoke(_instance, EventArgs.Empty);
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdClickedCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdClicked callback");
                _instance?.OnAdClicked?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnVideoStartCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnVideoStart callback");
                _instance?.OnVideoStart?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnVideoEndCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnVideoEnd callback");
                _instance?.OnVideoEnd?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VideoMuteCallback))]
        private static void OnVideoMuteCallback(bool isMuted)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"AdmobNativePlatformIOSClient: OnVideoMute callback - {isMuted}");
                _instance?.OnVideoMute?.Invoke(_instance, isMuted);
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnVideoPlayCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnVideoPlay callback");
                _instance?.OnVideoPlay?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnVideoPauseCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnVideoPause callback");
                _instance?.OnVideoPause?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdShowedFullScreenContentCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdShowedFullScreenContent callback");
                _instance?.OnAdShowedFullScreenContent?.Invoke();
            });
        }

        [MonoPInvokeCallback(typeof(VoidCallback))]
        private static void OnAdDismissedFullScreenContentCallback()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("AdmobNativePlatformIOSClient: OnAdDismissedFullScreenContent callback");
                _instance?.OnAdDismissedFullScreenContent?.Invoke();
            });
        }
    }
}

#endif
