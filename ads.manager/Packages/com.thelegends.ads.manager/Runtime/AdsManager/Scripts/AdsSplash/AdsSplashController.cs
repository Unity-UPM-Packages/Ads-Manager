using System;
using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using UnityEngine;

using TheLegends.Base.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using EditorAttributes;
using System.Linq;

namespace TheLegends.Base.Ads
{
    public class AdsSplashController : MonoBehaviour
    {
        [SerializeField]
        private bool isUseSelectBrand = true;

        private readonly WaitForSeconds _loadDelay = new WaitForSeconds(0.5f);

        [Space(10)]
        [SerializeField, ShowField(nameof(isUseSelectBrand))]
        private AdsPos mrecOpenPos = AdsPos.CenterLeft;
        [SerializeField, ShowField(nameof(isUseSelectBrand))]
        private Vector2Int mrecOpenOffset = Vector2Int.zero;

        [Space(10)]
        [SerializeField, ShowField(nameof(isUseSelectBrand))]
        private BrandScreenController brandScreen;



        [Space(10)]
        [SerializeField]
        private string sceneName;

        private bool canShowSelectBrand
        {
            get
            {
                var temp = PlayerPrefs.GetInt("canShowSelectBrand", 1);
                return temp == 1;
            }
        }

        [Space(10)]
        [SerializeField]
        private UnityEvent OnInitFirebaseDone = new UnityEvent();

        [Space(10)]
        [SerializeField]
        private UnityEvent OnInitAdsDone = new UnityEvent();

        [Space(10)]
        [SerializeField]
        private UnityEvent OnCompleteSplash = new UnityEvent();

        public void Start()
        {
            brandScreen.gameObject.SetActive(false);
            StartCoroutine(IELoad());
        }

        private IEnumerator IELoad()
        {
            UILoadingController.SetProgress(0.2f, null);

            // Initialize Firebase with remote config
            yield return InitializeFirebase();

            // Initialize AppsFlyer
            yield return AppsFlyerManager.Instance.DoInit(OnGetAppsFlyerConversionData);

            // Fetch remote data and update configs
            yield return FetchAndUpdateRemoteConfigs();
            OnInitFirebaseDone?.Invoke();

            // Initialize Ads Manager
            yield return AdsManager.Instance.DoInit();

            UILoadingController.SetProgress(0.4f, null);
            yield return _loadDelay;

            // Load ads based on brand selection settings
            if (isUseSelectBrand)
            {
                yield return LoadInitialAds();
            }

            UILoadingController.SetProgress(0.6f, null);

            // Load the target scene
            yield return IELoadScene();

            // Complete initialization
            CompleteInitialization();
        }

        private void OnGetAppsFlyerConversionData(string conversionData)
        {
            Dictionary<string, object> conversionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(conversionData);

            var campaign_id = conversionDataDictionary.FirstOrDefault(k => k.Key == "campaign_id").Value as string;
            if (!string.IsNullOrEmpty(campaign_id))
            {
                FirebaseManager.Instance.SetUserProperty("af_campaign_id", campaign_id);
            } else
            {
                if (AdsManager.Instance.SettingsAds.isTest)
                {
                    FirebaseManager.Instance.SetUserProperty("af_campaign_id", "campaign_id_test");
                }
            }
            
        }

        private IEnumerator InitializeFirebase()
        {
            var defaultRemoteConfig = CreateDefaultRemoteConfig();
            yield return FirebaseManager.Instance.DoInit(defaultRemoteConfig);
        }

        private Dictionary<string, object> CreateDefaultRemoteConfig()
        {
            var config = new Dictionary<string, object>
            {
                {"isUseAdInterOpen", AdsManager.Instance.adsConfigs.isUseAdInterOpen},
                {"isUseAdMrecOpen", AdsManager.Instance.adsConfigs.isUseAdMrecOpen},
                {"isUseAdAppOpenOpen", AdsManager.Instance.adsConfigs.isUseAdAppOpenOpen},
                {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
                {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
                {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds},
                {"adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight},
                {"adTimeReload", AdsManager.Instance.adsConfigs.adTimeReload},
                {"adLoadTimeOut", AdsManager.Instance.adsConfigs.adLoadTimeOut},
                {"isUseAdNative", AdsManager.Instance.adsConfigs.isUseAdNative},
                {"nativeVideoCountdownTimerDuration", AdsManager.Instance.adsConfigs.nativeVideoCountdownTimerDuration},
                {"nativeVideoDelayBeforeCountdown", AdsManager.Instance.adsConfigs.nativeVideoDelayBeforeCountdown},
                {"nativeVideoCloseClickableDelay", AdsManager.Instance.adsConfigs.nativeVideoCloseClickableDelay},
                {"nativeBannerTimeReload", AdsManager.Instance.adsConfigs.nativeBannerTimeReload}
            };

            return config;
        }

        private IEnumerator FetchAndUpdateRemoteConfigs()
        {
            bool isFetching = true;
            FirebaseManager.Instance.FetchRemoteData(() =>
            {
                UpdateCommonConfigs();
                isFetching = false;
            }, () =>
            {
                isFetching = false;
            });

            while (isFetching)
            {
                yield return null;
            }
        }


        private void UpdateCommonConfigs()
        {
            var configs = AdsManager.Instance.adsConfigs;
            configs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", configs.adInterOnComplete);
            configs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", configs.adInterOnStart);
            configs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", configs.timePlayToShowAds);
            configs.adNativeBannerHeight = FirebaseManager.Instance.RemoteGetValueFloat("adNativeBannerHeight", configs.adNativeBannerHeight);
            configs.adTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("adTimeReload", configs.adTimeReload);
            configs.adLoadTimeOut = FirebaseManager.Instance.RemoteGetValueFloat("adLoadTimeOut", configs.adLoadTimeOut);
            configs.isUseAdNative = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdNative", configs.isUseAdNative);
            configs.nativeVideoCountdownTimerDuration = FirebaseManager.Instance.RemoteGetValueFloat("nativeVideoCountdownTimerDuration", configs.nativeVideoCountdownTimerDuration);
            configs.nativeVideoDelayBeforeCountdown = FirebaseManager.Instance.RemoteGetValueFloat("nativeVideoDelayBeforeCountdown", configs.nativeVideoDelayBeforeCountdown);
            configs.nativeVideoCloseClickableDelay = FirebaseManager.Instance.RemoteGetValueFloat("nativeVideoCloseClickableDelay", configs.nativeVideoCloseClickableDelay);
            configs.nativeBannerTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("nativeBannerTimeReload", configs.nativeBannerTimeReload);
            configs.isUseAdInterOpen = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdInterOpen", configs.isUseAdInterOpen);
            configs.isUseAdMrecOpen = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdMrecOpen", configs.isUseAdMrecOpen);
            configs.isUseAdAppOpenOpen = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdAppOpenOpen", configs.isUseAdAppOpenOpen);
        }

        private IEnumerator LoadInitialAds()
        {

            if (!AdsManager.Instance.IsCanShowAds)
            {
                yield break;
            }

            // Load MREC for brand selection if needed
            if (canShowSelectBrand && AdsManager.Instance.adsConfigs.isUseAdMrecOpen)
            {

#if USE_ADMOB
                AdsManager.Instance.LoadNativeMrecOpen(PlacementOrder.One);
                yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativeMrecOpen, PlacementOrder.One);
#endif


                if (AdsManager.Instance.GetAdsStatus(AdsType.NativeMrecOpen, PlacementOrder.One) != AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.LoadMrec(AdsType.MrecOpen, PlacementOrder.One);
                    yield return AdsManager.Instance.WaitAdLoaded(AdsType.MrecOpen, PlacementOrder.One);
                }
            }

            // Load interstitial for app open if enabled
            if (AdsManager.Instance.adsConfigs.isUseAdInterOpen)
            {
#if USE_ADMOB
                AdsManager.Instance.LoadNativeInterOpen(PlacementOrder.One);
                yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativeInterOpen, PlacementOrder.One);
#endif

                if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, PlacementOrder.One) != AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.LoadInterstitial(AdsType.InterOpen, PlacementOrder.One);
                    yield return AdsManager.Instance.WaitAdLoaded(AdsType.InterOpen, PlacementOrder.One);
                }

            }
            
            if (AdsManager.Instance.adsConfigs.isUseAdAppOpenOpen
                && !AdsManager.Instance.adsConfigs.isUseAdInterOpen)
            {
                AdsManager.Instance.LoadAppOpen(PlacementOrder.One);
                yield return AdsManager.Instance.WaitAdLoaded(AdsType.AppOpen, PlacementOrder.One);
            }
        }

        private void CompleteInitialization()
        {
            float finishProgress = isUseSelectBrand ? 1f : 0.9f;

            UILoadingController.SetProgress(finishProgress, () =>
            {
                if (isUseSelectBrand)
                {
                    StartCoroutine(HandleBrandSelectionFlow());
                }

                OnInitAdsDone?.Invoke();

                if (!isUseSelectBrand)
                {
                    CompleteSplash();
                }
            });
        }

        public void CompleteSplash()
        {

            AdsManager.Instance.HideMrec(AdsType.MrecOpen, PlacementOrder.One);
#if USE_ADMOB
            AdsManager.Instance.HideNativeMrecOpen(PlacementOrder.One);
#endif

            brandScreen.OnClose -= CompleteSplash;

            OnCompleteSplash?.Invoke();
        }

        private IEnumerator HandleBrandSelectionFlow()
        {
            // Show interstitial if available

            bool isShowAdOpen = false;

            if (AdsManager.Instance.adsConfigs.isUseAdInterOpen)
            {
                if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
                {
#if USE_ADMOB
                    AdsManager.Instance.ShowNativeInterOpen(PlacementOrder.One, "native_inter_open", NativeName.Native_Inter, null, () =>
                    {
                        isShowAdOpen = false;
                    }, null)
                    .WithCountdown(AdsManager.Instance.adsConfigs.nativeVideoCountdownTimerDuration, AdsManager.Instance.adsConfigs.nativeVideoDelayBeforeCountdown, AdsManager.Instance.adsConfigs.nativeVideoCloseClickableDelay)
                    .Execute();

                    isShowAdOpen = true;
#endif
                }
                else
                {
                    if (AdsManager.Instance.GetAdsStatus(AdsType.InterOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.ShowInterstitial(AdsType.InterOpen, PlacementOrder.One, "Inter Open", () =>
                        {
                            isShowAdOpen = false;
                        });
                        isShowAdOpen = true;
                    }
                }
            }

            if (AdsManager.Instance.adsConfigs.isUseAdAppOpenOpen
                && !AdsManager.Instance.adsConfigs.isUseAdInterOpen)
            {
                if (AdsManager.Instance.GetAdsStatus(AdsType.AppOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.ShowAppOpen(PlacementOrder.One, "App Open", () =>
                    {
                        isShowAdOpen = false;
                    });
                    isShowAdOpen = true;
                }
            }


            while (isShowAdOpen)
            {
                yield return null;
            }

            // Show brand selection screen if can show
            if (canShowSelectBrand && AdsManager.Instance.adsConfigs.isUseAdMrecOpen)
            {
                ShowBrandScreen();
            } else {
                CompleteSplash();
            }

            UILoadingController.Hide();
        }


        private void ShowBrandScreen()
        {
            if (!AdsManager.Instance.adsConfigs.isUseAdMrecOpen) return;
            
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeMrecOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
            {
#if USE_ADMOB
                AdsManager.Instance.ShowNativeMrecOpen(PlacementOrder.One, "native_mrec_open", NativeName.Native_Mrec, null, null, null)
                .WithPosition(mrecOpenPos, mrecOpenOffset)
                .Execute();
                brandScreen.Show();
                brandScreen.OnClose += CompleteSplash;
#endif
            }
            else if (AdsManager.Instance.GetAdsStatus(AdsType.MrecOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.ShowMrec(AdsType.MrecOpen, PlacementOrder.One, mrecOpenPos, mrecOpenOffset, "Mrec Open");
                brandScreen.Show();
                brandScreen.OnClose += CompleteSplash;
            }
            else
            {
                CompleteSplash();
            }
        }

        private IEnumerator IELoadScene()
        {
            Debug.Log("LoadScene");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            StartCoroutine(IEPreloadAds());

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private IEnumerator IEPreloadAds()
        {
            var settings = AdsManager.Instance.SettingsAds.preloadSettings;

            if (settings.preloadBanner)
            {
                AdsManager.Instance.LoadBanner(PlacementOrder.One);
                yield return null;
            }

            if (settings.preloadInterstitial)
            {
                AdsManager.Instance.LoadInterstitial(AdsType.Interstitial, PlacementOrder.One);
                yield return null;
            }
            
            if (settings.preloadRewarded)
            {
                AdsManager.Instance.LoadRewarded(PlacementOrder.One);
                yield return null;
            }

            if (settings.preloadMREC)
            {
                AdsManager.Instance.LoadMrec(AdsType.Mrec, PlacementOrder.One);
                yield return null;
            }

            if (settings.preloadAppOpen)
            {
                AdsManager.Instance.LoadAppOpen(PlacementOrder.One);
                yield return null;
            }



#if USE_ADMOB
            if (settings.nativeAds.preloadNativeOverlay)
            {
                AdsManager.Instance.LoadNativeOverlay(PlacementOrder.One);
                yield return null;
            }
            if (settings.nativeAds.preloadNativeBanner)
            {
                AdsManager.Instance.LoadNativeBanner(PlacementOrder.One);
                yield return null;
            }
            if (settings.nativeAds.preloadNativeInter)
            {
                AdsManager.Instance.LoadNativeInter(PlacementOrder.One);
                yield return null;
            }
            if (settings.nativeAds.preloadNativeReward)
            {
                AdsManager.Instance.LoadNativeReward(PlacementOrder.One);
                yield return null;
            }
            if (settings.nativeAds.preloadNativeMrec)
            {
                AdsManager.Instance.LoadNativeMrec(PlacementOrder.One);
                yield return null;
            }
            if (settings.nativeAds.preloadNativeAppOpen)
            {
                AdsManager.Instance.LoadNativeAppOpen(PlacementOrder.One);
                yield return null;
            }
            if (settings.nativeAds.preloadNativeVideo)
            {
                AdsManager.Instance.LoadNativeVideo(PlacementOrder.One);
                yield return null;
            }
#endif
        }


    }
}
