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

        public abstract void LoadInterstitial();
        public abstract void ShowInterstitial(string position);

        public abstract void LoadRewarded();
        public abstract void ShowRewarded(Action OnRewarded, string position);

        public abstract void LoadAppOpen();
        public abstract void ShowAppOpen(string position);

        public abstract void LoadBanner();
        public abstract void ShowBanner(string position);
        public abstract void HideBanner();

        public abstract void LoadMrec();
        public abstract void ShowMrec(MrecPos mrecPosition, Vector2Int offset, string position);
        public abstract void HideMrec();

        public abstract AdsNetworks GetNetworkType();
    }
}

