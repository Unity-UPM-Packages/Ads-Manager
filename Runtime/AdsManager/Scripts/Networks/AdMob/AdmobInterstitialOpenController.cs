using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheLegends.Base.Ads
{
    public class AdmobInterstitialOpenController : AdmobInterstitialController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.InterOpen;
#else
            return AdsType.None;
#endif
        }

        protected override void OnInterClosed()
        {
            Status = AdsEvents.Close;
        }
    }

}
