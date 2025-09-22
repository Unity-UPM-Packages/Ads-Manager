#if USE_ADMOB
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
            OnClose?.Invoke();
            OnClose = null;
            Status = AdsEvents.Close;
        }
    }

}

#endif
