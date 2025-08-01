using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobMrecController : AdmobBannerController
    {
        private Vector2Int offset = new Vector2Int(0, 0);
        private MrecPos mrecPosition = MrecPos.None;
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.Mrec;
#else
        return AdsType.None;
#endif
        }

        protected override void CreateBanner()
        {
#if USE_ADMOB
            _bannerView = new BannerView(adsUnitID.Trim(), AdSize.MediumRectangle, AdPosition.Center);
#endif
        }

        public void ShowAds(MrecPos position, Vector2Int offset, string showPosition)
        {
            this.offset = offset;
            this.mrecPosition = position;
            base.ShowAds(showPosition);

        }

        protected override void PreShow()
        {
#if UNITY_EDITOR
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                SetAdCustomPosition(mrecPosition, offset);
            });
#else
            SetAdCustomPosition(mrecPosition, offset);
#endif

        }


        public void SetAdCustomPosition(MrecPos position, Vector2Int offset)
        {
            if (!IsAdsReady())
            {
                return;
            }

            var deviceScale = MobileAds.Utils.GetDeviceScale();

            float adWidth = _bannerView.GetWidthInPixels() / deviceScale;
            float adHeight = _bannerView.GetHeightInPixels() / deviceScale;

            var safeAreaWidth = Screen.safeArea.width / deviceScale;
            var safeAreaHeight = Screen.safeArea.height / deviceScale;

            int xMax = (int)(safeAreaWidth - adWidth);
            int yMax = (int)(safeAreaHeight - adHeight);
            int xCenter = xMax / 2;
            int yCenter = yMax / 2;

            Vector2Int newPos = Vector2Int.zero;

            switch (position)
            {
                case MrecPos.Top:
                    newPos = new Vector2Int(xCenter + offset.x, offset.y);

                    break;
                case MrecPos.TopLeft:
                    newPos = new Vector2Int(offset.x, offset.y);

                    break;
                case MrecPos.TopRight:
                    newPos = new Vector2Int(xMax + offset.x, offset.y);

                    break;
                case MrecPos.Center:
                    newPos = new Vector2Int(xCenter + offset.x, yCenter + offset.y);

                    break;
                case MrecPos.CenterLeft:
                    newPos = new Vector2Int(offset.x, yCenter + offset.y);

                    break;
                case MrecPos.CenterRight:
                    newPos = new Vector2Int(xMax + offset.x, yCenter + offset.y);

                    break;
                case MrecPos.Bottom:
                    newPos = new Vector2Int(xCenter + offset.x, yMax + offset.y);

                    break;
                case MrecPos.BottomLeft:
                    newPos = new Vector2Int(offset.x, yMax + offset.y);

                    break;
                case MrecPos.BottomRight:
                    newPos = new Vector2Int(xMax + offset.x, yMax + offset.y);

                    break;
            }


            _bannerView.SetPosition(newPos.x, newPos.y);
        }

    }
}
