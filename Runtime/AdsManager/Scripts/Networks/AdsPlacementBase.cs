using System.Collections;
using System.Collections.Generic;
using Baracuda.Threading;
using LitMotion;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public abstract class AdsPlacementBase : MonoBehaviour
    {
        protected Placement placement;
        public Placement Placement { get => placement; set => placement = value; }

        protected int adsUnitIDIndex = 0;

        protected string adsUnitID = string.Empty;
        protected string position = "default";

        protected int reloadCount = 0;

        protected AdsEvents status;
        public AdsEvents Status
        {
            get => status;
            protected set
            {
                if (status != value)
                {
                    status = value;
                    AdsManager.Instance.SetStatus(AdsType, adsUnitID, position, value, AdsNetworks);
                }
            }
        }

        public bool IsReady { get => IsAdsReady(); }

        public abstract bool IsAdsReady();

        public bool IsAvailable { get => IsAdsAvailable(); }

        public AdsNetworks AdsNetworks { get => GetAdsNetworks(); }

        public AdsType AdsType { get =>  GetAdsType(); }

        protected MotionHandle timeOutHandle;



        public virtual void Init(Placement placement)
        {
            this.Placement = placement;
        }

        public abstract AdsNetworks GetAdsNetworks();

        public abstract AdsType GetAdsType();

        public virtual void LoadAds()
        {
            Status = AdsEvents.LoadRequest;
            timeOutHandle = LMotion.Create(0f, 0f, AdsManager.Instance.adsConfigs.adLoadTimeOut).WithOnComplete(HandleTimeOut).RunWithoutBinding();
        }

        protected bool IsCanLoadAds()
        {
            if (Status == AdsEvents.LoadRequest)
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} "  + "is loading --> return");
                return false;
            }

            if(IsAvailable)
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is available --> return");
                return false;
            }

            if(placement.stringIDs != null && placement.stringIDs.Count > 0)
            {
                adsUnitIDIndex %= placement.stringIDs.Count;
                adsUnitID = placement.stringIDs[adsUnitIDIndex];
            }

            if (string.IsNullOrEmpty(adsUnitID))
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "UnitId NULL or Empty --> return");
                return false;
            }

            return true;
        }

        public virtual void OnAdsLoadAvailable()
        {
            Status = AdsEvents.LoadAvailable;
            reloadCount = 0;
        }

        public bool IsAdsAvailable()
        {
            return Status == AdsEvents.LoadAvailable;
        }

        protected virtual void OnAdsLoadFailed(string message)
        {

            Status = AdsEvents.LoadFail;

            string extendString = "";

            if (reloadCount < AdsManager.Instance.SettingsAds.autoReLoadMax)
            {
                extendString = " re-trying in " + (5 * (reloadCount + 1)) + " seconds " + (reloadCount + 1) + "/" + AdsManager.Instance.SettingsAds.autoReLoadMax;
            }


            AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString()} " +
                                         "OnAdsLoadFailed " + adsUnitID + " Error: " + message + extendString);

            if (reloadCount < AdsManager.Instance.SettingsAds.autoReLoadMax)
            {
                adsUnitIDIndex++;
                reloadCount++;
                Invoke(nameof(LoadAds), 5 * reloadCount);
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

            AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString()} " + "OnAdsShowFailed " +
                                         adsUnitID + " Error: " + message);
        }


        public virtual void OnAdsClosed()
        {
            Status = AdsEvents.Close;
            adsUnitIDIndex = 0;
            LoadAds();
        }

        public virtual void OnAdsClick()
        {
            Status = AdsEvents.Click;
        }

        public virtual void OnImpression()
        {
            AdsManager.Instance.Log($"{AdsType} "+ "ad recorded an impression.");
        }




    }
}

