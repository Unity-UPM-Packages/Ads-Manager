using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using TheLegends.Base.AppsFlyer;
using TheLegends.Base.Firebase;
using TheLegends.Base.UnitySingleton;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdsManager : PersistentMonoSingleton<AdsManager>
    {
        private List<AdsNetworkBase> adsNetworks = new List<AdsNetworkBase>();
        // [SerializeField] private AdmobNetworkController admob;

        protected static AdsSettings settingsAds = null;

        public static AdsSettings SettingsAds
        {
            get
            {
                if (settingsAds == null)
                {
                    settingsAds = Resources.Load<AdsSettings>(AdsSettings.FileName);
                }

                if (settingsAds == null)
                {
                    throw new Exception("[AdsManager]" + " AdsSettings NULL --> Please creat from Ads Manager/Ads Settings");
                }
                else
                {
                    return settingsAds;
                }
            }
            private set => settingsAds = value;
        }

        protected static AdsNetworks DefaultMediation
        {
            get { return SettingsAds.AdsNetworks.FirstOrDefault(); }
        }

        private static float timePlayToShowAds = 20f;

        private static DateTime lastTimeShowAd = DateTime.Now.AddSeconds(-600);

        public static DateTime LastTimeShowAd
        {
            get => lastTimeShowAd;
            set => lastTimeShowAd = value;
        }

        public static bool IsTimeToShowAd
        {
            get
            {
                float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;
                bool canShowAds = Mathf.FloorToInt(totalTimePlay) >= timePlayToShowAds;

                LogWarning(
                    $"Total Time play: {totalTimePlay} - Time to show ads: {timePlayToShowAds} - Can show ads: {canShowAds}");

                return canShowAds;
            }
        }


        public void Init()
        {
            StartCoroutine(DoInit());
        }

        private IEnumerator DoInit()
        {
            if (SettingsAds.AdsNetworks == null || SettingsAds.AdsNetworks.Count == 0)
            {
                LogError("AdsNetworks NULL or Empty --> return");

                yield break;
            }

            adsNetworks = GetComponentsInChildren<AdsNetworkBase>().ToList();

            // foreach (var network in SettingsAds.AdsNetworks)
            // {
            //     TODO more network
            //      if (network == AdsNetworks.Iron)
            //      {
            //          Log("Init " + network);
            //          instance.iron.Init(IsDebug);
            //          yield return new WaitForSeconds(0.25f);
            //      }
            //      else if (network == AdsNetworks.Max)
            //      {
            //          Log("Init " + network);
            //          instance.max.Init(IsDebug);
            //          yield return new WaitForSeconds(0.25f);
            //      }
            //     if (network == AdsNetworks.Admob)
            //     {
            //         Log("Init " + network);
            //         yield return admob.DoInit();
            //         yield return new WaitForSeconds(0.25f);
            //     }
            //     else
            //     {
            //         LogError("Init " + network + " NOT SUPPORT");
            //         yield return new WaitForSeconds(0.25f);
            //     }
            // }

            foreach (var network in adsNetworks)
            {
                yield return network.DoInit();
                yield return new WaitForSeconds(0.25f);
            }
        }

        private AdsNetworkBase GetNetwork(AdsNetworks network)
        {
            return adsNetworks.FirstOrDefault(x => x.GetNetworkType() == network);
        }

        public void LoadInterstitial()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadInterstitial();
            }
        }

        public void ShowInterstitial(string position)
        {
            if (!IsTimeToShowAd)
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowInterstitial(position);
            }
        }

        public void LoadRewarded()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadRewarded();
            }
        }

        public void ShowRewarded(Action OnRewarded, string position)
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowRewarded(OnRewarded, position);
            }
        }

        public void LoadAppOpen()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadAppOpen();
            }
        }

        public void ShowAppOpen(string position)
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowAppOpen(position);
            }
        }

        public void LoadBanner()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadBanner();
            }
        }

        public void ShowBanner(string position)
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowBanner(position);
            }
        }

        public void HideBanner()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideBanner();
            }
        }

        public void LoadMrec()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadMrec();
            }
        }

        public void ShowMrec(MrecPos mrecPosition, Vector2Int offset, string position)
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowMrec(mrecPosition, offset, position);
            }
        }

        public void HideMrec()
        {
            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideMrec();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            Debug.Log("OnApplicationPause " + isPaused);

            if (isPaused == false)
            {
                StartCoroutine(IEShowAppOpen());
            }
        }

        private IEnumerator IEShowAppOpen()
        {
            yield return new WaitForEndOfFrame();

            if (!IsTimeToShowAd)
            {
                yield break;
            }

            ShowAppOpen("Pause");
        }


        #region Common

        public static void SetStatus(AdsType adsType, string adsUnitID, string position, AdsEvents adEvent, AdsNetworks networks)
        {
            string eventName = $"{adsType}_{adEvent.ToString()}_{adsUnitID}";

            Log(eventName);

            FirebaseManager.Instance.LogEvent(eventName, new Dictionary<string, object>()
            {
                { "network", networks.ToString() },
                { "position", position },
                { "adUnitID", adsUnitID }
            });

            if (adEvent == AdsEvents.ShowSuccess || adEvent == AdsEvents.Close)
            {
                LastTimeShowAd = DateTime.Now;
            }
        }

        public static void LogImpressionData(AdsNetworks network, AdsType adsType, string adsUnitID, object value)
        {
            string monetizationNetwork = "";
            double revenue = 0;
            string ad_unit_name = "";
            string ad_format = "";
            string country = "";
            string currency = "USD";
            MediationNetwork mediation = MediationNetwork.Custom;

            if (value == null)
            {
                LogWarning("LogImpressionData: " + "data NULL");
            }

#if USE_ADMOB
            if (value is GoogleMobileAds.Api.AdValue)
            {
                var impressionData = value as GoogleMobileAds.Api.AdValue;

                if (impressionData != null)
                {
                    mediation = MediationNetwork.GoogleAdMob;
                    monetizationNetwork = "googleadmob";
                    ad_format = adsType.ToString();
                    ad_unit_name = adsUnitID;
                    country = "";
                    //The ad's value in micro-units, where 1,000,000 micro-units equal one unit of the currency.
                    revenue = (double)impressionData.Value / 1000000f;
                    currency = impressionData.CurrencyCode;


                }

                Debug.Log("GoogleMobileAds AdValue: " + impressionData.Value + " Revenue: " + revenue + " CurrencyCode: " + currency + " Precision: " + impressionData.Precision);

            }
#endif

#if USE_APPSFLYER
            AppsFlyerManager.Instance.LogImpression(new Dictionary<string, string>()
            {
                { "mediation", mediation.ToString() },
                { "monetizationNetwork", monetizationNetwork },
                { "ad_format", ad_format },
                { "ad_unit_name", ad_unit_name },
                { "country", country },
                { "revenue", revenue.ToString() },
                { "currency", currency },
            });

            AppsFlyerManager.Instance.LogRevenue(monetizationNetwork, mediation, currency, revenue, new Dictionary<string, string>()
            {
                { AdRevenueScheme.AD_UNIT, ad_unit_name },
                { AdRevenueScheme.AD_TYPE, ad_format },
                { AdRevenueScheme.COUNTRY, country },
            });
#endif
        }

        public static void Log(string message)
        {
            Debug.Log("AdsManager------: " + message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning("AdsManager------: " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError("AdsManager------: " + message);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        #endregion
    }

    public enum TagLog
    {
        UMP,
        ADMOB,
    }
}
