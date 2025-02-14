using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoogleMobileAds.Api;
using TheLegends.Base.Ads;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeOverlayController : AdsPlacementBase
    {
        private NativeOverlayAd _nativeOverlayAd;

        public override AdsNetworks GetAdsNetworks()
        {
#if USE_ADMOB
            return AdsNetworks.Admob;
#else
            return AdsNetworks.None;
#endif
        }

        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeOverlay;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            return _nativeOverlayAd != null;
#else
            return false;
#endif
        }

        public override void LoadAds()
        {
#if USE_ADMOB
            if(!IsCanLoadAds())
            {
                return;
            }

            if (!IsReady)
            {
                NativeDestroy();

                base.LoadAds();

                AdRequest request = new AdRequest();

                var options = new NativeAdOptions
                {
                    AdChoicesPlacement  = AdChoicesPlacement.TopRightCorner,
                    MediaAspectRatio = MediaAspectRatio.Any,
                };


                NativeOverlayAd.Load(adsUnitID.Trim(), request, options,
                    (NativeOverlayAd ad, LoadAdError error) =>
                    {
                        // if error is not null, the load request failed.
                        if(error != null)
                        {
                            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "ad failed to load with error : " + error);
                            OnNativeLoadFailed(error);
                            return;
                        }

                        if(ad == null)
                        {
                            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                            OnNativeLoadFailed(error);
                            return;
                        }

                        AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

                        _nativeOverlayAd = ad;

                        OnAdsLoadAvailable();
                    });
            }
#endif

        }

        public override void ShowAds(string showPosition)
        {
            if (Status == AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "is showing --> return");
                return;
            }

            base.ShowAds(showPosition);
            RenderAd();
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                _nativeOverlayAd.OnAdClicked += base.OnAdsClick;
                _nativeOverlayAd.OnAdPaid += OnAdsPaid;
                _nativeOverlayAd.OnAdImpressionRecorded += OnImpression;
                _nativeOverlayAd.OnAdFullScreenContentClosed += OnNativeOverlayClosed;
                _nativeOverlayAd.OnAdFullScreenContentOpened += () => base.OnAdsShowSuccess();
                _nativeOverlayAd.Show();
                Status = AdsEvents.ShowSuccess;
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }

        public void RenderAd()
        {
#if USE_ADMOB
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Rendering Native Overlay ad.");

                // Define a native template style with a custom style.
                var style = new NativeTemplateStyle
                {
                    TemplateId = NativeTemplateId.Small,
                    MainBackgroundColor = Color.red,
                    CallToActionText = new NativeTemplateTextStyle()
                    {
                        BackgroundColor = Color.green,
                        TextColor = Color.white,
                        FontSize = 9,
                        Style = NativeTemplateFontStyle.Bold
                    }
                };

                _nativeOverlayAd.RenderTemplate(style, AdSize.MediumRectangle, AdPosition.Bottom);
            }
#endif

        }

        #region Internal

        private void OnNativeLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            base.OnAdsLoadFailed(errorDescription);
        }

        private void OnNativeOverlayClosed()
        {
            base.OnAdsClosed();
        }

        private void OnAdsPaid(AdValue value)
        {
            AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
        }

        public void HideAds()
        {
#if USE_ADMOB
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            if (_nativeOverlayAd != null)
            {
                _nativeOverlayAd.Hide();
                NativeDestroy();
                base.OnAdsClosed();
            }
#endif
        }

        public void NativeDestroy()
        {
#if USE_ADMOB
            if (_nativeOverlayAd != null)
            {
                try
                {
                    _nativeOverlayAd.Destroy();
                    _nativeOverlayAd = null;
                }
                catch (Exception ex)
                {
                    AdsManager.Instance.LogException(ex);
                }
            }
#endif
        }

        #endregion

        public void SetAdCustomPosition(MrecPos position, Vector2Int offset)
        {
            if (!IsAdsReady())
            {
                return;
            }

            var deviceScale = MobileAds.Utils.GetDeviceScale();
            Debug.Log("Overlay device scale " + deviceScale);

            var deviceSafeWidth = MobileAds.Utils.GetDeviceSafeWidth();

            float adWidth = _nativeOverlayAd.GetTemplateWidthInPixels() / deviceScale;

            if (adWidth == float.PositiveInfinity)
            {
                adWidth = 320;
            }

            Debug.Log("Overlay ad width " + adWidth);

            float adHeight = _nativeOverlayAd.GetTemplateHeightInPixels() / deviceScale;

            if (adHeight == float.PositiveInfinity)
            {
                adHeight = 50;
            }

            Debug.Log("Overlay ad height " + adHeight);

            var safeArea = Screen.safeArea;

            Debug.Log("Overlay safe width " + safeArea.width);
            Debug.Log("Overlay safe height " + safeArea.height);

            float screenRatioSafeArea = safeArea.width / safeArea.height;

            Debug.Log("Overlay screen ratio safe area " + screenRatioSafeArea);

            if (deviceSafeWidth <= 0)
            {
                deviceSafeWidth = MobileAds.Utils.GetDeviceSafeWidth();
            }

            if (deviceSafeWidth == 0)
            {
                deviceSafeWidth = 832;
            }

            Debug.Log("Overlay device safe width " + deviceSafeWidth);

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

            _nativeOverlayAd.SetTemplatePosition(newPos.x, newPos.y);
        }
    }

    public enum NativeTemplateType
    {
        Small = 1,
        Medium = 2,
    }

}
