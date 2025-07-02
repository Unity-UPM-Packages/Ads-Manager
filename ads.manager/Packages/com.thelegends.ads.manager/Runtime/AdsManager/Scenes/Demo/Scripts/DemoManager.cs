using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using TheLegends.Base.Ads;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using UnityEngine;
using UnityEngine.UI;

public class DemoManager : MonoBehaviour
{
    public PlacementOrder order = PlacementOrder.One;
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
    public Button loadNativeOverlayBtn;
    public Button showNativeOverlayBtn;
    public Button hideNativeOverlayBtn;
    public Button testBtn;
    public Dropdown MrecPosDropdown;
    public Button loadNativeBtn;
    public Button showNativeBtn;
    public Button hideNativeBtn;
    public AdmobNativeController nativeAdsMrec;
    public AdmobNativeController nativeAdsBanner;

    public Image image;


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
        loadNativeOverlayBtn.onClick.AddListener(LoadNativeOverlay);
        showNativeOverlayBtn.onClick.AddListener(ShowNativeOverlay);
        hideNativeOverlayBtn.onClick.AddListener(HideNativeOverlay);
        testBtn.onClick.AddListener(Test);
        loadNativeBtn.onClick.AddListener(LoadNative);
        showNativeBtn.onClick.AddListener(ShowNative);
        hideNativeBtn.onClick.AddListener(HideNative);

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
            // {"testBool" , false },
            // {"testFloat" , 1.0f },
            // {"testInt" , 2 },
            // {"testString" , "test" },
            {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
            {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
            {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds},
            {"adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight},
            {"adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload}
        };

        yield return FirebaseManager.Instance.DoInit(defaultRemoteConfig);

        FirebaseManager.Instance.FetchRemoteData(() =>
        {
            // var testBool = FirebaseManager.Instance.RemoteGetValueBoolean("testBool", false);
            // var testFloat = FirebaseManager.Instance.RemoteGetValueFloat("testFloat", 1.0f);
            // var testInt = FirebaseManager.Instance.RemoteGetValueInt("testInt", 2);
            // var testString = FirebaseManager.Instance.RemoteGetValueString("testString", "test");
            AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
            AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
            AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
            AdsManager.Instance.adsConfigs.adNativeBannerHeight = FirebaseManager.Instance.RemoteGetValueFloat("adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight);
            AdsManager.Instance.adsConfigs.adNativeTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("adNativeTimeReload", AdsManager.Instance.adsConfigs.adNativeTimeReload);
        });

        yield return AppsFlyerManager.Instance.DoInit();
    }

    private void LoadInterstitial()
    {
        AdsManager.Instance.LoadInterstitial(AdsType.Interstitial, order);
    }

    private void ShowInterstitial()
    {
        AdsManager.Instance.ShowInterstitial(AdsType.Interstitial, order, "Default", () =>
        {
            AdsManager.Instance.Log("Interstitial closed");
        });
    }

    private void Loadrewarded()
    {
        AdsManager.Instance.LoadRewarded(order);
    }

    private void ShowRewarded()
    {
        AdsManager.Instance.ShowRewarded(order, "Default", () =>
        {
            AdsManager.Instance.Log("Rewarded successfully");
        });

    }

    private void LoadAppOpen()
    {
        AdsManager.Instance.LoadAppOpen(order);
    }

    private void ShowAppOpen()
    {
        AdsManager.Instance.ShowAppOpen(order, "Default");
    }

    private void LoadBanner()
    {
        AdsManager.Instance.LoadBanner(order);
    }

    private void ShowBanner()
    {
        AdsManager.Instance.ShowBanner(order, "Default");
    }

    private void HideBanner()
    {
        AdsManager.Instance.HideBanner(order);
    }

    private void LoadMrec()
    {
        AdsManager.Instance.LoadMrec(AdsType.Mrec, order);
    }

    private void ShowMrec()
    {
        var mrecPos = (MrecPos)MrecPosDropdown.value;
        AdsManager.Instance.ShowMrec(AdsType.Mrec, order, mrecPos, new Vector2Int(0, 0), "Default");
    }

    private void HideMrec()
    {
        AdsManager.Instance.HideMrec(AdsType.Mrec, order);
    }

    private void LoadNativeOverlay()
    {
        AdsManager.Instance.LoadNativeOverlay(order);
    }

    private void ShowNativeOverlay()
    {
        AdsManager.Instance.ShowNativeOverlay(order, "default");
    }


    private void HideNativeOverlay()
    {
        AdsManager.Instance.HideNativeOverlay(order);
    }

    private void Test()
    {
        SetAdPosition((MrecPos)MrecPosDropdown.value);
        // FirebaseManager.Instance.FetchRemoteData(() =>
        // {
        //     AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
        //     AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
        //     AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
        // });
    }

    public void SetAdPosition(MrecPos position)
    {
        var adWidth = image.rectTransform.rect.width;
        var adHeight = image.rectTransform.rect.height;
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var deviceScale = MobileAds.Utils.GetDeviceScale();

        Vector2 targetPosition = Vector2.zero;

        switch (position)
        {
            case MrecPos.TopLeft:
                targetPosition = new Vector2(0, 0);
                break;
            case MrecPos.Top:
                targetPosition = new Vector2((screenWidth / 2) - (adWidth / 2), 0);
                break;
            case MrecPos.TopRight:
                targetPosition = new Vector2(screenWidth - adWidth, 0);
                break;
            case MrecPos.Center:
                targetPosition = new Vector2((screenWidth / 2) - (adWidth / 2), -(screenHeight / 2) + (adHeight / 2));
                break;
            case MrecPos.CenterLeft:
                targetPosition = new Vector2(0, -(screenHeight / 2) + (adHeight / 2));
                break;
            case MrecPos.CenterRight:
                targetPosition = new Vector2(screenWidth - adWidth, -(screenHeight / 2) + (adHeight / 2));
                break;
            case MrecPos.Bottom:
                targetPosition = new Vector2((screenWidth / 2) - (adWidth / 2), -screenHeight + adHeight);
                break;
            case MrecPos.BottomLeft:
                targetPosition = new Vector2(0, -screenHeight + adHeight);
                break;
            case MrecPos.BottomRight:
                targetPosition = new Vector2(screenWidth - adWidth, -screenHeight + adHeight);
                break;
        }

        // image.rectTransform.anchoredPosition = targetPosition;
        var transformPoint = image.transform.TransformPoint(targetPosition);
        var worldPosition = Camera.main.ScreenToWorldPoint(transformPoint);
        Debug.Log("BBBBBBBBBB " + worldPosition);
        image.transform.position = worldPosition;


    }

    public void LoadNative()
    {
        nativeAdsMrec.LoadAds();
        nativeAdsBanner.LoadAds();
    }

    public void ShowNative()
    {
        nativeAdsMrec.ShowAds();
        nativeAdsBanner.ShowAds("Default");
    }

    public void HideNative()
    {
        nativeAdsMrec.HideAds();
        nativeAdsBanner.HideAds();
    }

    public void AAAAA()
    {
        AdsManager.Instance.GetAdsStatus(AdsType.Native, order);
    }


}
