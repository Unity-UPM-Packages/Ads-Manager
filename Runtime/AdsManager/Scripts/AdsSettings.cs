using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLegends.Base.Ads
{
    [CreateAssetMenu(fileName = "AdsSettings", menuName = "DataAsset/AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        public const string ResDir = "Assets/TripSoft/AdsManager/Resources";
        public const string FileName = "AdsSettingsAsset";
        public const string FileExtension = ".asset";

        [SerializeField]
        private List<AdsNetworks> _adsNetworks = new List<AdsNetworks>();
        public List<AdsNetworks> AdsNetworks => this._adsNetworks;

        private static AdsSettings _instance;
        public static AdsSettings Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = Resources.Load<AdsSettings>(FileName);
                return _instance;
            }
        }

        [SerializeField]
        private AdsNetworks _flagNetWorks;
        public AdsNetworks FlagNetWorks
        {
            get => this._flagNetWorks;

            set
            {
                if(this._flagNetWorks == value)
                {
                    return;
                }

                _flagNetWorks = value;

                _adsNetworks.Clear();

                var networksList = Enum.GetValues(typeof(AdsNetworks)).Cast<AdsNetworks>().ToList();

                foreach (var network in networksList)
                {
                    if ((_flagNetWorks & network) != 0)
                    {
                        _adsNetworks.Add(network);
                    }

                    switch (network)
                    {
                        case Ads.AdsNetworks.Iron:
                            showIRON = (_flagNetWorks & network) != 0;
                            break;
                        case Ads.AdsNetworks.Max:
                            showMAX = (_flagNetWorks & network) != 0;
                            break;
                        case Ads.AdsNetworks.Admob:
                            showADMOB = (_flagNetWorks & network) != 0;
                            break;
                    }
                }
            }
        }



        public string appsFlyerDevKey = "Qhno4yJY6KHmZp9uS9DRe4";
        public string appleAppId = "";

        public BannerPos bannerPosition = BannerPos.Bottom;
        public bool fixBannerSmallSize;

        public int autoReLoadMax = 2;
        public bool isTest = false;

        [Header("IRON")]
        public bool showIRON;
        public string ironAndroidAppKey = string.Empty;
        public string ironIOSAppKey = string.Empty;
        public bool ironEnableAdmob;
        public string ironAdmobAndroidAppID = string.Empty;
        public string ironAdmobIOSAppID = string.Empty;
        [Header("MAX")]
        public bool showMAX;
        public bool isShowMediationDebugger = false;
        public MaxUnitID MAX_Android = new MaxUnitID();
        public MaxUnitID MAX_iOS = new MaxUnitID();
        [Header("ADMOB")]
        public bool showADMOB;
        public bool isUseNativeUnity = false;
        public bool isShowAdmobNativeValidator = false;
        public AdmobUnitID ADMOB_Android = new AdmobUnitID();
        public AdmobUnitID ADMOB_IOS = new AdmobUnitID();
        public AdmobUnitID ADMOB_Android_Test = new AdmobUnitID
        {
            bannerIds = CreatePlacement("ca-app-pub-3940256099942544/6300978111"),
            interIds = CreatePlacement("ca-app-pub-3940256099942544/1033173712"),
            rewardIds = CreatePlacement("ca-app-pub-3940256099942544/5224354917"),
            appOpenIds = CreatePlacement("ca-app-pub-3940256099942544/9257395921"),
            mrecIds = CreatePlacement("ca-app-pub-3940256099942544/6300978111"),
            interOpenIds = CreatePlacement("ca-app-pub-3940256099942544/1033173712"),
            mrecOpenIds = CreatePlacement("ca-app-pub-3940256099942544/6300978111"),
            nativeUnityIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeOverlayIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativePlatformIds = CreatePlacement("ca-app-pub-3940256099942544/1044960115"),
            nativeBannerIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeInterIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeRewardIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeMrecIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeAppOpenIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeInterOpenIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeMrecOpenIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110"),
            nativeVideoIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110")
        };

        public AdmobUnitID ADMOB_IOS_Test = new AdmobUnitID
        {
            bannerIds = CreatePlacement("ca-app-pub-3940256099942544/2934735716"),
            interIds = CreatePlacement("ca-app-pub-3940256099942544/4411468910"),
            rewardIds = CreatePlacement("ca-app-pub-3940256099942544/1712485313"),
            appOpenIds = CreatePlacement("ca-app-pub-3940256099942544/5575463023"),
            mrecIds = CreatePlacement("ca-app-pub-3940256099942544/2934735716"),
            interOpenIds = CreatePlacement("ca-app-pub-3940256099942544/4411468910"),
            mrecOpenIds = CreatePlacement("ca-app-pub-3940256099942544/2934735716"),
            nativeUnityIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeOverlayIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativePlatformIds = CreatePlacement("ca-app-pub-3940256099942544/2521693316"),
            nativeBannerIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeInterIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeRewardIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeMrecIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeAppOpenIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeInterOpenIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeMrecOpenIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511"),
            nativeVideoIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511")
        };

        private static List<Placement> CreatePlacement(params string[] adUnitIds)
        {
            var placements = new List<Placement>();
            
            foreach (string adUnitId in adUnitIds)
            {
                placements.Add(new Placement
                {
                    stringIDs = new List<string> { adUnitId }
                });
            }
            
            return placements;
        }

    }
}
