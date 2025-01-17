using System.Collections;
using System.Collections.Generic;
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

        protected int reloadMax = 3;
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

        public AdsNetworks AdsNetworks { get => GetAdsNetworks(); }

        public AdsType AdsType { get =>  GetAdsType(); }

        public virtual void Init(Placement placement)
        {
            this.Placement = placement;
        }

        public abstract AdsNetworks GetAdsNetworks();

        public abstract AdsType GetAdsType();

        public abstract bool IsAdsReady();

        public virtual void LoadAds()
        {
            Status = AdsEvents.LoadRequest;
        }

        protected bool IsCanLoadAds()
        {
            if (Status == AdsEvents.LoadRequest)
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} "  + "is loading --> return");
                return false;
            }

            if(Status == AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is ready --> return");
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

        protected virtual void OnAdsLoadFailed(string message)
        {
            Status = AdsEvents.LoadFail;

            if (reloadCount < reloadMax)
            {
                adsUnitIDIndex++;
                reloadCount++;
                AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString() } " + "OnAdsLoadFailed " + adsUnitID + " Error: " + message + " re-trying in " + (5 * reloadCount) + " seconds " + reloadCount + "/" + reloadMax);
                Invoke(nameof(LoadAds), 5 * reloadCount);
            }
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

            AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString()} " + "OnAdsShowFailed " + adsUnitID + " Error: " + message);
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

