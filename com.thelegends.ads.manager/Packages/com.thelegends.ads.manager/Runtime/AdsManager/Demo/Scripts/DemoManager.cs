using System;
using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.Ads;
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
    }


    private void InitAdsManager()
    {
        AdsManager.Instance.Init();
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
            AdsManager.Log("Rewarded successfully");
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



}
