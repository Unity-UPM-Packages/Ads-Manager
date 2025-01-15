using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobMrecOpenController : AdmobMrecController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.MrecOpen;
#else
            return AdsType.None;
#endif
        }
    }
}
