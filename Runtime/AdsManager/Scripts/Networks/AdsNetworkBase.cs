using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public abstract class AdsNetworkBase : MonoBehaviour
    {
        private AdsNetworks networkType;
        public AdsNetworks NetWorkType {get => GetNetworkType();}
        public abstract IEnumerator DoInit();

        public abstract void LoadInterstitial(PlacementOrder order);
        public abstract void ShowInterstitial(PlacementOrder order, string position);

        public abstract void LoadRewarded(PlacementOrder order);
        public abstract void ShowRewarded(PlacementOrder order, Action OnRewarded, string position);

        public abstract void LoadAppOpen(PlacementOrder order);
        public abstract void ShowAppOpen(PlacementOrder order, string position);

        public abstract void LoadBanner(PlacementOrder order);
        public abstract void ShowBanner(PlacementOrder order, string position);
        public abstract void HideBanner(PlacementOrder order);

        public abstract void LoadMrec(PlacementOrder order);
        public abstract void ShowMrec(PlacementOrder order, MrecPos mrecPosition, Vector2Int offset, string position);
        public abstract void HideMrec(PlacementOrder order);


        public abstract AdsNetworks GetNetworkType();
    }
}

