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

        protected AdsSettings settingsAds = null;

        public AdsConfigs adsConfigs;

        public AdsSettings SettingsAds
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

        protected AdsNetworks DefaultMediation
        {
            get { return SettingsAds.AdsNetworks.FirstOrDefault(); }
        }

        private DateTime lastTimeShowAd = DateTime.Now.AddSeconds(-600);

        public DateTime LastTimeShowAd
        {
            get => lastTimeShowAd;
            set => lastTimeShowAd = value;
        }

        public bool IsTimeToShowAd
        {
            get
            {
                float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;
                bool canShowAds = Mathf.FloorToInt(totalTimePlay) >= adsConfigs.timePlayToShowAds;

                LogWarning(
                    $"Total Time play: {totalTimePlay} - Time to show ads: {adsConfigs.timePlayToShowAds} - Can show ads: {canShowAds}");

                return canShowAds;
            }
        }

        private InitiationStatus status = InitiationStatus.NotInitialized;


        public void Init()
        {
            StartCoroutine(DoInit());
        }

        public IEnumerator DoInit()
        {
            if (SettingsAds.AdsNetworks == null || SettingsAds.AdsNetworks.Count == 0)
            {
                LogError("AdsNetworks NULL or Empty --> return");
                status = InitiationStatus.Failed;
                yield break;
            }

            if(status == InitiationStatus.Initialized)
            {
                LogError("AdsManager already initialized");
                yield break;
            }

            status = InitiationStatus.Initializing;

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

            status = InitiationStatus.Initialized;
        }

        private AdsNetworkBase GetNetwork(AdsNetworks network)
        {
            return adsNetworks.FirstOrDefault(x => x.GetNetworkType() == network);
        }

        public void LoadInterstitial(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadInterstitial(order);
            }
        }

        public void ShowInterstitial(PlacementOrder order, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            if (!IsTimeToShowAd)
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowInterstitial(order, position);
            }
        }

        public void LoadRewarded(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadRewarded(order);
            }
        }

        public void ShowRewarded(PlacementOrder order, Action OnRewarded, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowRewarded(order, OnRewarded, position);
            }
        }

        public void LoadAppOpen(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadAppOpen(order);
            }
        }

        public void ShowAppOpen(PlacementOrder order, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            if (!IsTimeToShowAd)
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowAppOpen(order, position);
            }
        }

        public void LoadBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadBanner(order);
            }
        }

        public void ShowBanner(PlacementOrder order, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowBanner(order, position);
            }
        }

        public void HideBanner(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideBanner(order);
            }
        }

        public void LoadMrec(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.LoadMrec(order);
            }
        }

        public void ShowMrec(PlacementOrder order, MrecPos mrecPosition, Vector2Int offset, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.ShowMrec(order, mrecPosition, offset, position);
            }
        }

        public void HideMrec(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = GetNetwork(DefaultMediation);

            if (netWork != null)
            {
                netWork.HideMrec(order);
            }
        }

        public void LoadNativeOverlay(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.LoadNativeOverlay(order);
            }
        }

        public void ShowNativeOverlay(PlacementOrder order, string position)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.ShowNativeOverlay(order, position);
            }
        }

        public void HideNativeOverlay(PlacementOrder order)
        {
            if (!IsInitialized())
            {
                return;
            }

            var netWork = (AdmobNetworkController)GetNetwork(AdsNetworks.Admob);

            if (netWork != null)
            {
                netWork.HideNativeOverlay(order);
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
            ShowAppOpen(PlacementOrder.One ,"Pause");
        }


        #region Common

        public void SetStatus(AdsType adsType, string adsUnitID, string position, AdsEvents adEvent, AdsNetworks networks)
        {
            string eventName = $"{adsType}_{adEvent.ToString()}";

            Log(eventName);

            FirebaseManager.Instance.LogEvent(eventName, new Dictionary<string, object>()
            {
                { "network", networks.ToString() },
                { "type", adsType.ToString() },
                { "position", position },
                { "adUnitID", adsUnitID }
            });

            if (adEvent == AdsEvents.ShowSuccess || adEvent == AdsEvents.Close)
            {
                LastTimeShowAd = DateTime.Now;
            }
        }

        public void LogImpressionData(AdsNetworks network, AdsType adsType, string adsUnitID, object value)
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

        public void Log(string message)
        {
            Debug.Log("AdsManager------: " + message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning("AdsManager------: " + message);
        }

        public void LogError(string message)
        {
            Debug.LogError("AdsManager------: " + message);
        }

        public void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public new bool IsInitialized()
        {
            bool isInitialized = false;
            string message = "";

            switch (status)
            {
                case InitiationStatus.Initialized:
                    isInitialized = true;
                    break;
                case InitiationStatus.NotInitialized:
                    message = "AdsManager is not initialized";
                    break;
                case InitiationStatus.Initializing:
                    message = "AdsManager initializing";
                    break;
                case InitiationStatus.Failed:
                    message = "AdsManager initial failed";
                    break;
            }

            if (!isInitialized)
            {
                LogError(message);
            }

            return isInitialized;

        }

        #endregion
    }

    public enum TagLog
    {
        UMP,
        ADMOB,
    }

    [System.Serializable]
    public class AdsConfigs
    {
        public bool adInterOnComplete = true;
        public bool adInterOnStart = true;
        public float timePlayToShowAds = 20f;
        public float adNativeBannerHeight = 140;
        public float adNativeTimeReload = 15f;
    }

}
