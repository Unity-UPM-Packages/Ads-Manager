using System;
using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using UnityEngine;

using TheLegends.Base.UI;
using UnityEngine.SceneManagement;

namespace TheLegends.Base.Ads
{
    public class AdsSplashController : MonoBehaviour
    {
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

        public void Start()
        {
            brandScreen.gameObject.SetActive(false);
            StartCoroutine(IELoad());
        }

        private IEnumerator IELoad()
        {
            UILoadingController.SetProgress(0.2f, null);

            yield return AdsManager.Instance.DoInit();

            UILoadingController.SetProgress(0.4f, null);

            var defaultRemoteConfig = new Dictionary<string, object>
            {
                {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
                {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
                {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds},
                {"adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight},
                {"adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload},
                {"adLoadTimeOut", AdsManager.Instance.adsConfigs.adLoadTimeOut},
            };

            yield return FirebaseManager.Instance.DoInit(defaultRemoteConfig);

            FirebaseManager.Instance.FetchRemoteData(() =>
            {
                AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
                AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
                AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
                AdsManager.Instance.adsConfigs.adNativeBannerHeight = FirebaseManager.Instance.RemoteGetValueFloat("adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight);
                AdsManager.Instance.adsConfigs.adNativeTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload);
                AdsManager.Instance.adsConfigs.adLoadTimeOut = FirebaseManager.Instance.RemoteGetValueFloat("adLoadTimeOut", AdsManager.Instance.adsConfigs.adLoadTimeOut);
            });

            AdsManager.Instance.LoadMrec(AdsType.MrecOpen, PlacementOrder.One);
            AdsManager.Instance.LoadInterstitial(AdsType.InterOpen, PlacementOrder.One);

            yield return IECheckAdsOpenAvailable();

            yield return AppsFlyerManager.Instance.DoInit();


            UILoadingController.SetProgress(0.6f, null);

            yield return IELoadScene();

            UILoadingController.SetProgress(1f, () =>
            {
                UILoadingController.Hide();

                AdsManager.Instance.ShowInterstitial(AdsType.InterOpen, PlacementOrder.One, "Inter Open");

                if (canShowSelectBrand)
                {
                    ShowBrandScreen();
                }
            });
        }

        private IEnumerator IECheckAdsOpenAvailable()
        {
            while (!AdsManager.Instance.IsAdsTypeAvailable(AdsType.MrecOpen, PlacementOrder.One) ||
                   !AdsManager.Instance.IsAdsTypeAvailable(AdsType.InterOpen, PlacementOrder.One))
            {
                yield return null;
            }
        }

        private void ShowBrandScreen()
        {
            AdsManager.Instance.ShowMrec(AdsType.MrecOpen, PlacementOrder.One, MrecPos.CenterLeft, mrecOpenOffset, "Mrec Open");

            brandScreen.Show();
        }

        private IEnumerator IELoadScene()
        {
            Debug.Log("LoadScene");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }


    }
}
