using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using TheLegends.Base.Databuckets;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public abstract class AdsPlacementBase : MonoBehaviour
    {
        protected Placement placement;
        public Placement Placement { get => placement; set => placement = value; }

        public PlacementOrder Order { get; set; }
        protected int adsUnitIDIndex = 0;

        protected string adsUnitID = string.Empty;
        protected string position = "default";

        protected int reloadCount = 0;

        protected string _loadRequestId = "";

        protected string _currentLoadRequestId = "";
        protected string networkName = "";

        protected AdsEvents status;
        public AdsEvents Status
        {
            get => status;
            protected set
            {
                if (status != value)
                {
                    status = value;
                    AdsManager.Instance.SetStatus(AdsMediation, AdsType, adsUnitID, position, value);
                }
            }
        }

        public bool IsReady { get => IsAdsReady(); }

        public abstract bool IsAdsReady();

        public bool IsAvailable { get => IsAdsAvailable(); }

        public AdsMediation AdsMediation { get => GetAdsMediation(); }

        public AdsType AdsType { get => GetAdsType(); }


        public virtual void Init(Placement placement, PlacementOrder order)
        {
            SetTimeOut();
            this.Placement = placement;
            this.Order = order;
        }

        protected virtual void SetTimeOut()
        {
            timeOut = AdsManager.Instance.adsConfigs.adLoadTimeOut;
        }

        public abstract AdsMediation GetAdsMediation();

        public abstract AdsType GetAdsType();

        private Coroutine _reloadAdsCoroutine;

        private Coroutine _timeoutCoroutine;

        protected float timeOut = 10f;

        private readonly Dictionary<float, WaitForSeconds> _waitDictionary = new Dictionary<float, WaitForSeconds>();

        protected WaitForSeconds GetWait(float time)
        {
            if (_waitDictionary.TryGetValue(time, out var wait)) return wait;

            wait = new WaitForSeconds(time);
            _waitDictionary[time] = wait;
            return wait;
        }

        public virtual void LoadAds()
        {
            if (_reloadAdsCoroutine != null)
            {
                StopCoroutine(_reloadAdsCoroutine);
                _reloadAdsCoroutine = null;
            }

            Status = AdsEvents.LoadRequest;

            DatabucketsManager.Instance.RecordEvent("start_ad_request");

            _currentLoadRequestId = Guid.NewGuid().ToString();
            _loadRequestId = _currentLoadRequestId;

            StartHandleTimeout();
        }

        protected void StartHandleTimeout()
        {
            StopHandleTimeout();
            _timeoutCoroutine = StartCoroutine(HandleTimeOutCoroutine(timeOut));
        }

        protected void StopHandleTimeout()
        {
            if (_timeoutCoroutine != null)
            {
                StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }
        }

        protected IEnumerator HandleTimeOutCoroutine(float time)
        {
            yield return GetWait(time);
            HandleTimeOut();
        }

        protected IEnumerator ReloadAdsCoroutine(float time)
        {
            yield return GetWait(time);
            _reloadAdsCoroutine = null;
            LoadAds();
        }

        protected bool IsCanLoadAds()
        {
            if (!AdsManager.Instance.IsCanShowAds && AdsType != AdsType.Rewarded)
            {
                AdsManager.Instance.LogWarning($"{AdsMediation}_{AdsType} " + "is not can show ads --> return");
                return false;
            }

            if (_reloadAdsCoroutine != null)
            {
                AdsManager.Instance.LogWarning($"{AdsMediation}_{AdsType} " + "is scheduled loading --> return");
                return false;
            }

            if (Status == AdsEvents.LoadRequest)
            {
                AdsManager.Instance.LogWarning($"{AdsMediation}_{AdsType} " + "is loading --> return");
                return false;
            }

            if (IsAvailable)
            {
                AdsManager.Instance.LogWarning($"{AdsMediation}_{AdsType} " + "is available --> return");
                return false;
            }

            if (placement.stringIDs != null && placement.stringIDs.Count > 0)
            {
                adsUnitIDIndex %= placement.stringIDs.Count;
                adsUnitID = placement.stringIDs[adsUnitIDIndex];
                AdsManager.Instance.Log($"{AdsMediation}_{AdsType} " + "Startting LoadAds " + adsUnitID);
            }

            if (string.IsNullOrEmpty(adsUnitID))
            {
                AdsManager.Instance.LogWarning($"{AdsMediation}_{AdsType} " + "UnitId NULL or Empty --> return");
                return false;
            }

            return true;
        }

        public virtual void OnAdsLoadAvailable()
        {
            Status = AdsEvents.LoadAvailable;
            reloadCount = 0;

            DatabucketsManager.Instance.RecordEventWithTiming("ad_request", new Dictionary<string, object>
            {
                { "ad_format", AdsType.ToString() },
                { "ad_platform", AdsMediation.ToString() },
                { "ad_network", networkName},
                { "ad_unit_id", adsUnitID },
                { "is_load", 1 }
            }, "load_time", "start_ads_request");
        }

        public bool IsAdsAvailable()
        {
            return Status == AdsEvents.LoadAvailable && (AdsType == AdsType.Rewarded || AdsManager.Instance.IsCanShowAds);
        }

        protected virtual void OnAdsLoadFailed(string message)
        {

            Status = AdsEvents.LoadFail;
            _currentLoadRequestId = "";

            DatabucketsManager.Instance.RecordEventWithTiming("ad_request", new Dictionary<string, object>
            {
                { "ad_format", AdsType.ToString() },
                { "ad_platform", AdsMediation.ToString() },
                { "ad_network", "Unavailable"},
                { "ad_unit_id", adsUnitID },
                { "is_load", 0 }
            }, "load_time", "start_ads_request");

            float timeWait = 5f;

            switch (GetAdsType())
            {
                case AdsType.InterOpen:
                case AdsType.MrecOpen:
                case AdsType.NativeMrecOpen:
                case AdsType.NativeInterOpen:
                    timeWait = 0.125f;
                    break;
            }

            string extendString = "";

            if (reloadCount < AdsManager.Instance.SettingsAds.autoReLoadMax && timeWait > 0)
            {
                extendString = " re-trying in " + (timeWait * (reloadCount + 1)) + " seconds " + (reloadCount + 1) + "/" + AdsManager.Instance.SettingsAds.autoReLoadMax;
            }


            AdsManager.Instance.LogError($"{AdsMediation.ToString()}_{AdsType.ToString()} " +
                                         "OnAdsLoadFailed " + adsUnitID + " Error: " + message + extendString);

            if (reloadCount < AdsManager.Instance.SettingsAds.autoReLoadMax)
            {
                adsUnitIDIndex++;
                reloadCount++;
                if (_reloadAdsCoroutine != null) StopCoroutine(_reloadAdsCoroutine);
                _reloadAdsCoroutine = StartCoroutine(ReloadAdsCoroutine(timeWait * reloadCount));
            }
            else
            {
                Status = AdsEvents.LoadNotAvailable;
                reloadCount = 0;
            }

        }

        protected void HandleTimeOut()
        {
            if (Status == AdsEvents.LoadRequest)
            {
                OnAdsLoadTimeOut();
            }
        }

        protected virtual void OnAdsLoadTimeOut()
        {
            Status = AdsEvents.LoadTimeOut;
            StopHandleTimeout();
            OnAdsLoadFailed("TimeOut");
        }

        public virtual void ShowAds(string showPosition)
        {
            position = showPosition;
        }


        public virtual void OnAdsShowSuccess()
        {
            Status = AdsEvents.ShowSuccess;
        }

        public virtual void OnAdsShowFailed(string message)
        {
            Status = AdsEvents.ShowFail;

            AdsManager.Instance.LogError($"{AdsMediation.ToString()}_{AdsType.ToString()} " + "OnAdsShowFailed " +
                                         adsUnitID + " Error: " + message);
        }


        public virtual void OnAdsClosed()
        {
            Status = AdsEvents.Close;
            adsUnitIDIndex = 0;
            LoadAds();

            if (AdsType == AdsType.Interstitial 
            || AdsType == AdsType.InterOpen 
            || AdsType == AdsType.AppOpen
            || AdsType == AdsType.NativeInter
            || AdsType == AdsType.NativeInterOpen
            || AdsType == AdsType.NativeAppOpen)
            {
                DatabucketsManager.Instance.RecordEvent("ad_complete", new Dictionary<string, object>
                {
                    { "ad_format", AdsType.ToString() },
                    { "ad_platform", AdsMediation.ToString() },
                    { "ad_network", networkName},
                    { "ad_unit_id", adsUnitID },
                    { "placement", position}
                });
            }

        }

        public virtual void OnAdsClick()
        {
            Status = AdsEvents.Click;

            DatabucketsManager.Instance.RecordEvent("ad_click", new Dictionary<string, object>
            {
                { "ad_format", AdsType.ToString() },
                { "ad_platform", AdsMediation.ToString() },
                { "ad_network", networkName},
                { "ad_unit_id", adsUnitID },
                { "placement", position}
            });
        }

        public virtual void OnAdsCancel()
        {
            Status = AdsEvents.Cancel;
        }

        public virtual void OnImpression()
        {
            AdsManager.Instance.Log($"{AdsType} " + "ad recorded an impression.");
        }

    }
}

