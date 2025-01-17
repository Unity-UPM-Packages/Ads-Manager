using System;
using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.Ads;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using UnityEngine;
using UnityEngine.UI;

public class DemoManager : MonoBehaviour
{
    public Button initBtn;
    public Button loadInterstitialBtn;
    public Button showInterstitialBtn;
    public Button loadRewardedBtn;
    public Button showRewardedBtn;
    public Button loadAppOpenBtn;
    public Button showAppOpenBtn;
    public Button loadBannerBtn;
    public Button showBannerBtn;
    public Button hideBannerBtn;
    public Button loadMrecBtn;
    public Button showMrecBtn;
    public Button hideMrecBtn;
    public Button testBtn;
    public Dropdown MrecPosDropdown;


    private void OnEnable()
    {
        initBtn.onClick.AddListener(InitAdsManager);
        loadInterstitialBtn.onClick.AddListener(LoadInterstitial);
        showInterstitialBtn.onClick.AddListener(ShowInterstitial);
        loadRewardedBtn.onClick.AddListener(Loadrewarded);
        showRewardedBtn.onClick.AddListener(ShowRewarded);
        loadAppOpenBtn.onClick.AddListener(LoadAppOpen);
        showAppOpenBtn.onClick.AddListener(ShowAppOpen);
        loadBannerBtn.onClick.AddListener(LoadBanner);
        showBannerBtn.onClick.AddListener(ShowBanner);
        hideBannerBtn.onClick.AddListener(HideBanner);
        loadMrecBtn.onClick.AddListener(LoadMrec);
        showMrecBtn.onClick.AddListener(ShowMrec);
        hideMrecBtn.onClick.AddListener(HideMrec);
        testBtn.onClick.AddListener(Test);
    }


    private void InitAdsManager()
    {
        // AdsManager.Instance.Init();
        // FirebaseManager.Instance.Init();
        // AppsFlyerManager.Instance.Init();
        StartCoroutine(DoInit());
    }

    private IEnumerator DoInit()
    {
        yield return AdsManager.Instance.DoInit();

        var defaultRemoteConfig = new Dictionary<string, object>
        {
            {"testBool" , false },
            {"testFloat" , 1.0f },
            {"testInt" , 2 },
            {"testString" , "test" },
            // {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
            // {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
            // {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds}
        };

        yield return FirebaseManager.Instance.DoInit(defaultRemoteConfig);

        FirebaseManager.Instance.FetchRemoteData(() =>
        {
            var testBool = FirebaseManager.Instance.RemoteGetValueBoolean("testBool", false);
            var testFloat = FirebaseManager.Instance.RemoteGetValueFloat("testFloat", 1.0f);
            var testInt = FirebaseManager.Instance.RemoteGetValueInt("testInt", 2);
            var testString = FirebaseManager.Instance.RemoteGetValueString("testString", "test");
            // AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
            // AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
            // AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
        });

        yield return AppsFlyerManager.Instance.DoInit();
    }

    private void LoadInterstitial()
    {
        AdsManager.Instance.LoadInterstitial();
    }

    private void ShowInterstitial()
    {
        AdsManager.Instance.ShowInterstitial("Default");
    }

    private void Loadrewarded()
    {
        AdsManager.Instance.LoadRewarded();
    }

    private void ShowRewarded()
    {
        AdsManager.Instance.ShowRewarded(() =>
        {
            AdsManager.Instance.Log("Rewarded successfully");
        }, "Default");

    }

    private void LoadAppOpen()
    {
        AdsManager.Instance.LoadAppOpen();
    }

    private void ShowAppOpen()
    {
        AdsManager.Instance.ShowAppOpen("Default");
    }

    private void LoadBanner()
    {
        AdsManager.Instance.LoadBanner();
    }

    private void ShowBanner()
    {
        AdsManager.Instance.ShowBanner("Default");
    }

    private void HideBanner()
    {
        AdsManager.Instance.HideBanner();
    }

    private void LoadMrec()
    {
        AdsManager.Instance.LoadMrec();
    }

    private void ShowMrec()
    {
        var mrecPos = (MrecPos)MrecPosDropdown.value;
        AdsManager.Instance.ShowMrec(mrecPos, new Vector2Int(0, 0), "Default");
    }

    private void HideMrec()
    {
        AdsManager.Instance.HideMrec();
    }

    private void Test()
    {
        FirebaseManager.Instance.FetchRemoteData(() =>
        {
;
            AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
            AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
            AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
        });
    }



}
