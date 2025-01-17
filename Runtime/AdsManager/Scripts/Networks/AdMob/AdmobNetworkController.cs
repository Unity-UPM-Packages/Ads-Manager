using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNetworkController : AdsNetworkBase
    {
        private bool isChecking = false;

        [SerializeField] private List<string> testDevices = new List<string>();

        private List<AdmobInterstitialController> interList = new List<AdmobInterstitialController>();
        private List<AdmobRewardedController> rewardedList = new List<AdmobRewardedController>();
        private List<AdmobAppOpenController> appOpenList = new List<AdmobAppOpenController>();
        private List<AdmobBannerController> bannerList = new List<AdmobBannerController>();
        private List<AdmobMrecController> mrecList = new List<AdmobMrecController>();
        private List<AdmobMrecOpenController> mrecOpenList = new List<AdmobMrecOpenController>();
        private List<AdmobInterstitialOpenController> interOpenList = new List<AdmobInterstitialOpenController>();

        public override IEnumerator DoInit()
        {
            yield return RequestUMP();

            if (ConsentInformation.CanRequestAds())
            {
                MobileAds.SetRequestConfiguration( new RequestConfiguration
                {
                    TestDeviceIds = testDevices
                });

                MobileAds.RaiseAdEventsOnUnityMainThread = true;

                MobileAds.SetiOSAppPauseOnBackground(true);

                MobileAds.Initialize(initStatus =>
                {
                    if (initStatus == null)
                    {
                        AdsManager.Instance.LogError("Google Mobile Ads initialization failed.");
                        return;
                    }

                    if (initStatus != null)
                    {
                        AdsManager.Instance.Log($"{TagLog.ADMOB} " + "Mediations checking status...");
                        // If you use mediation, you can check the status of each adapter.
                        var adapterStatusMap = initStatus.getAdapterStatusMap();
                        if (adapterStatusMap != null)
                        {
                            foreach (var item in adapterStatusMap)
                            {
                                AdsManager.Instance.Log($"{TagLog.ADMOB} " + string.Format(" Google Adapter {0} is {1}",
                                    item.Key,
                                    item.Value.InitializationState));
                            }
                        }
                        AdsManager.Instance.Log($"{TagLog.ADMOB} " + "Mediations checking done.");

                        // if (loadOnInitDone)
                        // {
                        //     Invoke(nameof(InterInit), 0.5f);
                        //     Invoke(nameof(RewardInit), 1.0f);
                        // }
                    }

                    AdsManager.Instance.Log($"{TagLog.ADMOB} " + "Initialize: " + initStatus.ToString());

#if (UNITY_ANDROID || UNITY_IOS) && USE_ADMOB

                    var platform = Application.platform;
                    var isIOS = platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.OSXPlayer;

                    var isAdmobTest = AdsManager.Instance.SettingsAds.isAdmobTest;

                    var interIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.interIds, AdsManager.Instance.SettingsAds.ADMOB_Android.interIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.interIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.interIds);
                    CreateAdController(interIds, interList);

                    var rewardedIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.rewardIds, AdsManager.Instance.SettingsAds.ADMOB_Android.rewardIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.rewardIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.rewardIds);
                    CreateAdController(rewardedIds, rewardedList);

                    var appOpenIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.appOpenIds, AdsManager.Instance.SettingsAds.ADMOB_Android.appOpenIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.appOpenIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.appOpenIds);
                    CreateAdController(appOpenIds, appOpenList);

                    var bannerIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.bannerIds, AdsManager.Instance.SettingsAds.ADMOB_Android.bannerIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.bannerIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.bannerIds);
                    CreateAdController(bannerIds, bannerList);

                    var mrecIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.mrecIds, AdsManager.Instance.SettingsAds.ADMOB_Android.mrecIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.mrecIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.mrecIds);
                    CreateAdController(mrecIds, mrecList);

                    var mrecOpenIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.mrecOpenIds, AdsManager.Instance.SettingsAds.ADMOB_Android.mrecOpenIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.mrecOpenIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.mrecOpenIds);
                    CreateAdController(mrecOpenIds, mrecOpenList);

                    var interOpenIds = GetAdUnitIds(isIOS, isAdmobTest, AdsManager.Instance.SettingsAds.ADMOB_IOS.interOpenIds, AdsManager.Instance.SettingsAds.ADMOB_Android.interOpenIds, AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.interOpenIds, AdsManager.Instance.SettingsAds.ADMOB_Android_Test.interOpenIds);
                    CreateAdController(interOpenIds, interOpenList);
#endif
                });
            }
            else
            {
                AdsManager.Instance.Log($"{TagLog.UMP} " + "UMP ConsentStatus --> " + ConsentInformation.ConsentStatus.ToString() + " CanRequestAds: " + ConsentInformation.CanRequestAds().ToString().ToUpper() + " --> NOT INIT");
                yield return RequestUMP();
            }
        }



        private IEnumerator RequestUMP()
        {
            if (isChecking)
            {
                AdsManager.Instance.LogError($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " CHECKING");
                yield break;
            }

            isChecking = true;

            // Create a ConsentRequestParameters object.
            ConsentRequestParameters request = new ConsentRequestParameters();

            // Check the current consent information status.
            ConsentInformation.Update(request, (updateError =>
            {
                if (updateError != null)
                {
                    // Handle the error.
                    AdsManager.Instance.LogError($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + updateError.Message);
                    isChecking = false;
                    return;
                }

                if (ConsentInformation.CanRequestAds()) // Determine the consent-related action to take based on the ConsentStatus.
                {
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    AdsManager.Instance.Log($"{TagLog.UMP} " + "Update " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " -- Consent has already been gathered or not required");
                    isChecking = false;
                    return;
                }

                AdsManager.Instance.Log(ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW ConsentForm If Required");

                // If the error is null, the consent information state was updated.
                // You are now ready to check if a form is available.
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        // Consent gathering failed.
                        AdsManager.Instance.LogError($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + formError.Message);
                        return;
                    }
                    else
                    {
                        // Form showing succeeded.
                        AdsManager.Instance.Log($"{TagLog.UMP} " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                    }

                });
            }));

            while (isChecking && (ConsentInformation.ConsentStatus == ConsentStatus.Required || ConsentInformation.ConsentStatus == ConsentStatus.Unknown))
            {
                yield return null;
            }
        }

        private List<Placement> GetAdUnitIds(bool isIOS, bool isAdmobTest, List<Placement> iosIds, List<Placement> androidIds, List<Placement> iosTestIds, List<Placement> androidTestIds)
        {
            return isAdmobTest ? (isIOS ? iosTestIds : androidTestIds) : (isIOS ? iosIds : androidIds);
        }

        private void CreateAdController<T>(List<Placement> placements, List<T> adList) where T : AdsPlacementBase
        {
            if (placements.Count <= 0)
            {
                AdsManager.Instance.LogError($"{TagLog.ADMOB} {typeof(T).Name} IDs NULL or Empty --> return");
                return;
            }

            foreach (var adId in placements)
            {
                var adController = new GameObject().AddComponent<T>();
                adController.name = typeof(T).Name;
                adController.transform.parent = this.transform;
                ((AdsPlacementBase)adController).Init(adId);
                adList.Add(adController);
            }
        }

        public void OpenAdInspector()
        {
            AdsManager.Instance.Log("Opening ad Inspector.");
            MobileAds.OpenAdInspector((AdInspectorError error) =>
            {
                // If the operation failed, an error is returned.
                if (error != null)
                {
                    AdsManager.Instance.Log("Ad Inspector failed to open with error: " + error);
                    return;
                }

                AdsManager.Instance.Log("Ad Inspector opened successfully.");
            });
        }

        public override void LoadInterstitial()
        {
            interList[0].LoadAds();
        }

        public override void ShowInterstitial(string position)
        {
            interList[0].ShowAds(position);
        }

        public override void LoadRewarded()
        {
            rewardedList[0].LoadAds();
        }

        public override void ShowRewarded(Action OnRewarded, string position)
        {
            rewardedList[0].ShowAds(OnRewarded, position);
        }

        public override void LoadAppOpen()
        {
            appOpenList[0].LoadAds();
        }

        public override void ShowAppOpen(string position)
        {
            appOpenList[0].ShowAds(position);
        }

        public override void LoadBanner()
        {
            bannerList[0].LoadAds();
        }

        public override void HideBanner()
        {
            bannerList[0].HideAds();
        }

        public override void ShowBanner(string position)
        {
            bannerList[0].ShowAds(position);
        }

        public override void LoadMrec()
        {
            mrecList[0].LoadAds();
        }

        public override void ShowMrec(MrecPos mrecPosition, Vector2Int offset, string position)
        {
            mrecList[0].ShowAds(mrecPosition, offset, position);
        }

        public override void HideMrec()
        {
            mrecList[0].HideAds();
        }


        public override AdsNetworks GetNetworkType()
        {
            return AdsNetworks.Admob;
        }
    }
}

