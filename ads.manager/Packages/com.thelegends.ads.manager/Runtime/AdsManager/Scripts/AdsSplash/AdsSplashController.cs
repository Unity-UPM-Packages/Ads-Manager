using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using UnityEngine;

using TheLegends.Base.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace TheLegends.Base.Ads
{
    public class AdsSplashController : MonoBehaviour
    {
        [SerializeField]
        private MrecPos mrecOpenPos = MrecPos.CenterLeft;
        [SerializeField]
        private Vector2Int mrecOpenOffset = Vector2Int.zero;

        [Space(10)]
        [SerializeField]
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
        private UnityEvent OnLoadComplete = new UnityEvent();

        public void Start()
        {
            brandScreen.gameObject.SetActive(false);
            StartCoroutine(IELoad());
        }

        private IEnumerator IELoad()
        {
            UILoadingController.SetProgress(0.2f, null);

            yield return AppsFlyerManager.Instance.DoInit();

            var defaultRemoteConfig = new Dictionary<string, object>
            {
                {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
                {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
                {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds},
                {"adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight},
                {"adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload},
                {"adLoadTimeOut", AdsManager.Instance.adsConfigs.adLoadTimeOut},
                {"isUseAdNative", AdsManager.Instance.adsConfigs.isUseAdNative},
                {"isUseAdInterOpen", AdsManager.Instance.adsConfigs.isUseAdInterOpen},
            };

            yield return FirebaseManager.Instance.DoInit(defaultRemoteConfig);

            yield return AdsManager.Instance.DoInit();

            UILoadingController.SetProgress(0.4f, null);

            yield return new WaitForSeconds(0.5f);

            FirebaseManager.Instance.FetchRemoteData(() =>
            {
                AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
                AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
                AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
                AdsManager.Instance.adsConfigs.adNativeBannerHeight = FirebaseManager.Instance.RemoteGetValueFloat("adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight);
                AdsManager.Instance.adsConfigs.adNativeTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload);
                AdsManager.Instance.adsConfigs.adLoadTimeOut = FirebaseManager.Instance.RemoteGetValueFloat("adLoadTimeOut", AdsManager.Instance.adsConfigs.adLoadTimeOut);
                AdsManager.Instance.adsConfigs.isUseAdNative = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdNative", AdsManager.Instance.adsConfigs.isUseAdNative);
                AdsManager.Instance.adsConfigs.isUseAdInterOpen = FirebaseManager.Instance.RemoteGetValueBoolean("isUseAdInterOpen", AdsManager.Instance.adsConfigs.isUseAdInterOpen);
            });


            if (canShowSelectBrand)
            {
                AdsManager.Instance.LoadMrec(AdsType.MrecOpen, PlacementOrder.One);
                yield return WaitAdLoaded(AdsType.MrecOpen, PlacementOrder.One);
            }

            if (AdsManager.Instance.adsConfigs.isUseAdInterOpen)
            {
                
                AdsManager.Instance.LoadInterstitial(AdsType.InterOpen, PlacementOrder.One);
                yield return WaitAdLoaded(AdsType.InterOpen, PlacementOrder.One);
            }


            UILoadingController.SetProgress(0.6f, null);

            OnLoadComplete?.Invoke();

            yield return IELoadScene();

            UILoadingController.SetProgress(1f, () =>
            {
                UILoadingController.Hide();

                if (AdsManager.Instance.GetAdsStatus(AdsType.InterOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.ShowInterstitial(AdsType.InterOpen, PlacementOrder.One, "Inter Open");
                }


                if (canShowSelectBrand)
                {
                    ShowBrandScreen();
                }
            });
        }


        private IEnumerator WaitAdLoaded(AdsType type, PlacementOrder order)
        {
            while (AdsManager.Instance.GetAdsStatus(type, order) != AdsEvents.LoadAvailable && AdsManager.Instance.GetAdsStatus(type, order) != AdsEvents.LoadNotAvailable)
            {
                yield return null;
            }
        }


        private void ShowBrandScreen()
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.MrecOpen, PlacementOrder.One) == AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.ShowMrec(AdsType.MrecOpen, PlacementOrder.One, mrecOpenPos, mrecOpenOffset, "Mrec Open");
                brandScreen.Show();
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
        }


    }
}
