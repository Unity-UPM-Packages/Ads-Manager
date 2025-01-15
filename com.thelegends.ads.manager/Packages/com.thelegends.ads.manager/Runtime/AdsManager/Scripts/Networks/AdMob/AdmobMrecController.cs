using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobMrecController : AdmobBannerController
    {
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
            base.ShowAds(showPosition);
            SetAdCustomPosition(position, offset);
        }

        public void SetAdCustomPosition(MrecPos position, Vector2Int offset)
        {
            if (!IsAdsReady())
            {
                return;
            }

            var deviceScale = MobileAds.Utils.GetDeviceScale();
            var deviceSafeWidth = MobileAds.Utils.GetDeviceSafeWidth();

            float adWidth = _bannerView.GetWidthInPixels() / deviceScale;

            if (adWidth == float.PositiveInfinity)
            {
                adWidth = 320;
            }

            float adHeight = _bannerView.GetHeightInPixels() / deviceScale;

            if (adHeight == float.PositiveInfinity)
            {
                adHeight = 50;
            }

            var safeArea = Screen.safeArea;
            float screenSafeHeightPixel = safeArea.height;
            float screenRatioSafeArea = safeArea.width / safeArea.height;

            if (deviceSafeWidth <= 0)
            {
                deviceSafeWidth = MobileAds.Utils.GetDeviceSafeWidth();
            }

            if (deviceSafeWidth == 0)
            {
                deviceSafeWidth = 832;
            }

            float adScreenSafeHeight;
            float adScreenRatio = 0.6f;
            float adNavigationHeight = 21f;

            Vector2 anchorMin = safeArea.position;
            float bottomHeight = anchorMin.y;

            adScreenSafeHeight = deviceSafeWidth / screenRatioSafeArea;
            adScreenRatio = adScreenSafeHeight / screenSafeHeightPixel;
            adNavigationHeight = bottomHeight * adScreenRatio;

            int xMax = (int)(deviceSafeWidth - adWidth);
            int yMax = (int)(adScreenSafeHeight - adHeight);
            int xCenter = (int)(xMax * 0.5f);
            int yCenter = (int)(yMax * 0.5f);

            Vector2Int newPos = Vector2Int.zero;

            switch (position)
            {
                case MrecPos.Top:
                    newPos = new Vector2Int(xCenter + offset.x, -offset.y);

                    break;
                case MrecPos.TopLeft:
                    newPos = new Vector2Int(offset.x, -offset.y);

                    break;
                case MrecPos.TopRight:
                    newPos = new Vector2Int(xMax + offset.x, -offset.y);

                    break;
                case MrecPos.Center:
                    newPos = new Vector2Int(xCenter + offset.x, yCenter + -offset.y);

                    break;
                case MrecPos.CenterLeft:
                    newPos = new Vector2Int(offset.x, yCenter + -offset.y);

                    break;
                case MrecPos.CenterRight:
                    newPos = new Vector2Int(xMax + offset.x, yCenter + -offset.y);

                    break;
                case MrecPos.Bottom:
                    newPos = new Vector2Int(xCenter + offset.x, yMax + -offset.y);

                    break;
                case MrecPos.BottomLeft:
                    newPos = new Vector2Int(offset.x, yMax + -offset.y);

                    break;
                case MrecPos.BottomRight:
                    newPos = new Vector2Int(xMax + offset.x, yMax + -offset.y);

                    break;
            }

            _bannerView.SetPosition(newPos.x, newPos.y);
        }
    }
}
