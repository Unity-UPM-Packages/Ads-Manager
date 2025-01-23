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

        public int autoReLoadMax = 3;

        [Header("IRON")]
        public bool showIRON;
        public bool isIronTest = false;
        public string ironAndroidAppKey = string.Empty;
        public string ironIOSAppKey = string.Empty;
        public bool ironEnableAdmob;
        public string ironAdmobAndroidAppID = string.Empty;
        public string ironAdmobIOSAppID = string.Empty;
        [Header("MAX")]
        public bool showMAX;
        public bool isMaxTest = false;
        public string maxSdkKey = string.Empty;
        public bool maxEnableAdmob;
        public string maxAdmobAndroidAppID = string.Empty;
        public string maxAdmobIOSAppID = string.Empty;
        public MaxUnitID MAX_Android = new MaxUnitID();
        public MaxUnitID MAX_iOS = new MaxUnitID();
        [Header("ADMOB")]
        public bool showADMOB;
        public bool isAdmobTest = false;
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
            nativeOverlayIds = CreatePlacement("ca-app-pub-3940256099942544/2247696110")
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
            nativeOverlayIds = CreatePlacement("ca-app-pub-3940256099942544/3986624511")
        };

        private static List<Placement> CreatePlacement(string adUnitId)
        {
            return new List<Placement>
            {
                new Placement
                {
                    stringIDs = new List<string> { adUnitId }
                }
            };
        }

    }
}
