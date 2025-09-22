#if USE_MAX

namespace TheLegends.Base.Ads
{
    public class MaxMrecOpenController : MaxMrecController
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
#if USE_MAX
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            if (IsReady)
            {
                MaxSdkCallbacks.MRec.OnAdLoadedEvent -= OnMRecLoadedEvent;
                MaxSdkCallbacks.MRec.OnAdLoadFailedEvent -= OnMRecLoadFailedEvent;
                MaxSdkCallbacks.MRec.OnAdClickedEvent -= OnMRecClickedEvent;
                MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= OnMRecRevenuePaidEvent;

                MaxSdk.HideBanner(adsUnitID);
                MRecDestroy();
            }
#endif
        }
    }
}

#endif
