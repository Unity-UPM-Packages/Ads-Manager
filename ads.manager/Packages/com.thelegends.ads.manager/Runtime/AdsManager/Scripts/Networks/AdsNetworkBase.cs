using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public abstract class AdsNetworkBase : MonoBehaviour
    {
        private AdsNetworks networkType;
        public AdsNetworks NetWorkType { get => GetNetworkType(); }
        public abstract IEnumerator DoInit();

        public abstract void LoadInterstitial(AdsType interType, PlacementOrder order);
        public abstract void ShowInterstitial(AdsType interType, PlacementOrder order, string position, Action OnClose = null);

        public abstract void LoadRewarded(PlacementOrder order);
        public abstract void ShowRewarded(PlacementOrder order, string position, Action OnRewarded = null);

        public abstract void LoadAppOpen(PlacementOrder order);
        public abstract void ShowAppOpen(PlacementOrder order, string position, Action OnClose = null);

        public abstract void LoadBanner(PlacementOrder order);
        public abstract void ShowBanner(PlacementOrder order, string position);
        public abstract void HideBanner(PlacementOrder order);

        public abstract void LoadMrec(AdsType mrecType, PlacementOrder order);
        public abstract void ShowMrec(AdsType mrecType, PlacementOrder order, AdsPos mrecPosition, Vector2Int offset, string position);
        public abstract void HideMrec(AdsType mrecType, PlacementOrder order);

        public abstract void HideAllBanner();
        public abstract void HideAllMrec();

        public abstract AdsEvents GetAdsStatus(AdsType adsType, PlacementOrder order);


        public abstract AdsNetworks GetNetworkType();
        public abstract void RemoveAds();
        public abstract bool IsAdsReady(AdsType adsType, PlacementOrder order);
        public abstract bool IsAdsControllerExist(AdsType adsType, PlacementOrder order);
    }
}

