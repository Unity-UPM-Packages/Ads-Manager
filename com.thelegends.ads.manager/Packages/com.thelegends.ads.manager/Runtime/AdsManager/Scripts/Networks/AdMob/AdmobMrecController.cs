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
            var safeArea = Screen.safeArea;

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

            if (deviceSafeWidth <= 0)
            {
                deviceSafeWidth = MobileAds.Utils.GetDeviceSafeWidth();
            }

            if (deviceSafeWidth == 0)
            {
                deviceSafeWidth = 832;
            }

            float screenRatioSafeArea = safeArea.width / safeArea.height;

            float adScreenSafeHeight = deviceSafeWidth / screenRatioSafeArea;

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

            Debug.Log("BBBBBB " + newPos);

            _bannerView.SetPosition(newPos.x, newPos.y);
        }

        public void SetAdPosition(MrecPos position, Vector2Int offset)
        {
            var adWidth = _bannerView.GetWidthInPixels();
            var adHeight = _bannerView.GetHeightInPixels();
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            var deviceScale = MobileAds.Utils.GetDeviceScale();

            Vector2 targetPosition = Vector2.zero;

            switch (position)
            {
                case MrecPos.TopLeft:
                    targetPosition = new Vector2(0, 0);
                    break;
                case MrecPos.Top:
                    targetPosition = new Vector2((screenWidth/2) - (adWidth/2), 0);
                    break;
                case MrecPos.TopRight:
                    targetPosition = new Vector2(screenWidth - adWidth, 0);
                    break;
                case MrecPos.Center:
                    targetPosition = new Vector2((screenWidth/2) - (adWidth/2), -(screenHeight/2) + (adHeight/2));
                    break;
                case MrecPos.CenterLeft:
                    targetPosition = new Vector2(0, -(screenHeight/2) + (adHeight/2));
                    break;
                case MrecPos.CenterRight:
                    targetPosition = new Vector2(screenWidth - adWidth, -(screenHeight/2) + (adHeight/2));
                    break;
                case MrecPos.Bottom:
                    targetPosition = new Vector2((screenWidth/2) - (adWidth/2), -screenHeight + adHeight);
                    break;
                case MrecPos.BottomLeft:
                    targetPosition = new Vector2(0, -screenHeight + adHeight);
                    break;
                case MrecPos.BottomRight:
                    targetPosition = new Vector2(screenWidth - adWidth, -screenHeight + adHeight);
                    break;
                default:
                    targetPosition = new Vector2(0, 0);
                    break;
            }

            // var transformPoint = transform.TransformPoint(targetPosition);
            // Vector2 worldPosition = Camera.main.WorldToScreenPoint(targetPosition) * deviceScale;
            // Debug.Log("AAAAAAAAAAAA " + worldPosition);
            _bannerView.SetPosition((int)targetPosition.x, (int)targetPosition.y);
        }
    }
}
