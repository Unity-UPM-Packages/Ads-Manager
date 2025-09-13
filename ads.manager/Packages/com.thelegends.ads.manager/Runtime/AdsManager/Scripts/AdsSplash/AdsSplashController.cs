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

namespace TheLegends.Base.Ads
{
    public class AdsSplashController : MonoBehaviour
    {
        [SerializeField]
        private bool isUseSelectBrand = true;

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

        private bool isUseAdInterOpen = true;

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

            // Initialize AppsFlyer
            yield return AppsFlyerManager.Instance.DoInit();

            // Initialize Firebase with remote config
            yield return InitializeFirebase();

            // Initialize Ads Manager
            yield return AdsManager.Instance.DoInit();

            UILoadingController.SetProgress(0.4f, null);
            yield return new WaitForSeconds(0.5f);

            // Fetch remote data and update configs
            FetchAndUpdateRemoteConfigs();
            OnInitFirebaseDone?.Invoke();

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

            CheckCompleteSplash();
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
                {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
                {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
                {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds},
                {"adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight},
                {"adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload},
                {"adLoadTimeOut", AdsManager.Instance.adsConfigs.adLoadTimeOut},
                {"isUseAdNative", AdsManager.Instance.adsConfigs.isUseAdNative},
                {"nativeVideoCountdownTimerDuration", AdsManager.Instance.adsConfigs.nativeVideoCountdownTimerDuration},
                {"nativeVideoDelayBeforeCountdown", AdsManager.Instance.adsConfigs.nativeVideoDelayBeforeCountdown},
                {"nativeVideoCloseClickableDelay", AdsManager.Instance.adsConfigs.nativeVideoCloseClickableDelay},
                {"nativeBannerTimeReload", AdsManager.Instance.adsConfigs.nativeBannerTimeReload}
            };

            if (isUseSelectBrand)
            {
                config.Add("isUseAdInterOpen", isUseAdInterOpen);
            }

            return config;
        }

        private void FetchAndUpdateRemoteConfigs()
        {
            FirebaseManager.Instance.FetchRemoteData(() =>
            {
                UpdateCommonConfigs();
                UpdateBrandSpecificConfigs();
            }, 0);
        }

        private void UpdateCommonConfigs()
        {
            var configs = AdsManager.Instance.adsConfigs;
            configs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", configs.adInterOnComplete);
            configs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", configs.adInterOnStart);
            configs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", configs.timePlayToShowAds);
            configs.adNativeBannerHeight = FirebaseManager.Instance.RemoteGetValueFloat("adNativeBannerHeight", configs.adNativeBannerHeight);
            configs.adNativeTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("adNativeTimeReload", configs.adNativeTimeReload);
            configs.adLoadTimeOut = FirebaseManager.Instance.RemoteGetValueFloat("adLoadTimeOut", configs.adLoadTimeOut);
            configs.isUseAdNative = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdNative", configs.isUseAdNative);
            configs.nativeVideoCountdownTimerDuration = FirebaseManager.Instance.RemoteGetValueFloat("nativeVideoCountdownTimerDuration", configs.nativeVideoCountdownTimerDuration);
            configs.nativeVideoDelayBeforeCountdown = FirebaseManager.Instance.RemoteGetValueFloat("nativeVideoDelayBeforeCountdown", configs.nativeVideoDelayBeforeCountdown);
            configs.nativeVideoCloseClickableDelay = FirebaseManager.Instance.RemoteGetValueFloat("nativeVideoCloseClickableDelay", configs.nativeVideoCloseClickableDelay);
            configs.nativeBannerTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("nativeBannerTimeReload", configs.nativeBannerTimeReload);
        }

        private void UpdateBrandSpecificConfigs()
        {
            if (isUseSelectBrand)
            {
                isUseAdInterOpen = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdInterOpen", isUseAdInterOpen);
            }
        }

        private IEnumerator LoadInitialAds()
        {

            // Load MREC for brand selection if needed
            if (canShowSelectBrand)
            {

                AdsManager.Instance.LoadNativePlatform(PlacementOrder.Three);
                yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativePlatform, PlacementOrder.Three);
                

                if (AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.Three) != AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.LoadMrec(AdsType.MrecOpen, PlacementOrder.One);
                    yield return AdsManager.Instance.WaitAdLoaded(AdsType.MrecOpen, PlacementOrder.One);
                }
            }

            // Load interstitial for app open if enabled
            if (isUseAdInterOpen)
            {
                AdsManager.Instance.LoadNativePlatform(PlacementOrder.Four);
                yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativePlatform, PlacementOrder.Four);

                if (AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.Four) != AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.LoadInterstitial(AdsType.InterOpen, PlacementOrder.One);
                    yield return AdsManager.Instance.WaitAdLoaded(AdsType.InterOpen, PlacementOrder.One);
                }
                
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

        private void CheckCompleteSplash()
        {
            if (!canShowSelectBrand)
            {
                CompleteSplash();
            }
        }

        public void CompleteSplash()
        {
            // if (AdsManager.Instance.GetAdsStatus(AdsType.MrecOpen, PlacementOrder.One) == AdsEvents.ShowSuccess)
            // {
                AdsManager.Instance.HideMrec(AdsType.MrecOpen, PlacementOrder.One);
            // }

            // if (AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.Three) == AdsEvents.Show)
            // {
                AdsManager.Instance.HideNativePlatform(PlacementOrder.Three);
            // }

            brandScreen.OnClose -= CompleteSplash;

            OnCompleteSplash?.Invoke();
        }

        private IEnumerator HandleBrandSelectionFlow()
        {
            // Show interstitial if available

            bool isShowInter = false;

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.Four) == AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.ShowNativeVideoPlatform(PlacementOrder.Four, "native_inter_open", "native_inter", null, () =>
                {
                    isShowInter = false;
                }, null);
                isShowInter = true;
            }
            else
            {
                if (AdsManager.Instance.GetAdsStatus(AdsType.InterOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.ShowInterstitial(AdsType.InterOpen, PlacementOrder.One, "Inter Open", () =>
                    {
                        isShowInter = false;
                    });
                    isShowInter = true;
                }
            }

            while (isShowInter)
            {
                yield return null;
            }

            // Show brand selection screen if can show
            if (canShowSelectBrand)
            {
                ShowBrandScreen();
            }

            UILoadingController.Hide();
        }


        private void ShowBrandScreen()
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativePlatform, PlacementOrder.Three) == AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.ShowNativePlatform(PlacementOrder.Three, "native_mrec_open", "native_mrec_open", null, null, null).Execute();
                brandScreen.Show();
                brandScreen.OnClose += CompleteSplash;
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

            PreloadAds();

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private void PreloadAds()
        {
            AdsManager.Instance.LoadInterstitial(AdsType.Interstitial, PlacementOrder.One);
            AdsManager.Instance.LoadRewarded(PlacementOrder.One);
            AdsManager.Instance.LoadMrec(AdsType.Mrec, PlacementOrder.One);
            AdsManager.Instance.LoadAppOpen(PlacementOrder.One);
            AdsManager.Instance.LoadBanner(PlacementOrder.One);
            AdsManager.Instance.LoadNativeOverlay(PlacementOrder.One);
            AdsManager.Instance.LoadNativePlatform(PlacementOrder.One);
        }


    }
}
