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

        public override void HideAds()
        {
#if USE_ADMOB
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");

                return;
            }

            if (_bannerView != null)
            {
                _bannerView.Hide();
                BannerDestroy();
                Status = AdsEvents.Close;
            }
        }
#endif
    }
}
