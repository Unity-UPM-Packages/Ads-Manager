using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MaxSdkBase;

namespace TheLegends.Base.Ads
{
    public class MaxMrecController : AdsPlacementBase
    {
        private bool isReady = false;
        public override AdsNetworks GetAdsNetworks()
        {
#if USE_MAX
            return AdsNetworks.Max;
#else
            return AdsNetworks.None;
#endif
        }

        public override AdsType GetAdsType()
        {
#if USE_MAX
            return AdsType.Mrec;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_MAX
            return isReady;
#else
            return false;
#endif
        }

        public override void LoadAds()
        {
            // #if USE_MAX
            //             if (!IsCanLoadAds())
            //             {
            //                 return;
            //             }

            //             if (!IsReady)
            //             {
            //                 base.LoadAds();

            //                 MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecLoadedEvent;
            //                 MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecLoadFailedEvent;
            //                 MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecClickedEvent;
            //                 MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecRevenuePaidEvent;

            //                 MaxSdk.LoadMRec(adsUnitID);
            //             }
            // #endif
        }

        public override void ShowAds(string showPosition)
        {
//             base.ShowAds(showPosition);
// #if USE_MAX
//             if (IsReady && IsAvailable)
//             {
//                 Status = AdsEvents.ShowSuccess;
//                 MaxSdk.ShowMRec(adsUnitID);
//             }
//             else
//             {
//                 AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
//                 reloadCount = 0;
//                 LoadAds();
//             }
// #endif
        }

        private void LoadAds(AdsPos position, Vector2Int offset)
        {
#if USE_MAX
            if (!IsCanLoadAds())
            {
                return;
            }

            if (!IsReady)
            {
                base.LoadAds();

                CreateMRec(position, offset);

                MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecLoadedEvent;
                MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecLoadFailedEvent;
                MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecClickedEvent;
                MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecRevenuePaidEvent;

                MaxSdk.LoadMRec(adsUnitID);
            }
#endif
        }

        public void ShowAds(AdsPos position, Vector2Int offset, string showPosition)
        {
#if USE_MAX
            LoadAds(position, offset);
            ShowAds(showPosition);
#endif
        }



        private void CreateMRec(AdsPos position, Vector2Int offset)
        {
#if USE_MAX
            var adPosition = SetAdCustomPosition(position, offset);
            var adViewConfiguration = new AdViewConfiguration(adPosition.x, adPosition.y);
            MaxSdk.CreateMRec(adsUnitID, adViewConfiguration);
#endif
        }

        #region Internal

        private void OnMRecLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                isReady = true;
                OnAdsLoadAvailable();
            });
        }

        private void OnMRecLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsLoadFailed(errorInfo.Message);
            });
        }

        private void OnMRecClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnMRecRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, adInfo);
            });
        }

        protected void MRecDestroy()
        {
#if USE_MAX
            if (IsReady)
            {
                try
                {
                    MaxSdk.DestroyMRec(adsUnitID);
                }
                catch (Exception ex)
                {
                    AdsManager.Instance.LogException(ex);
                }
            }
#endif
        }

        #endregion

        public virtual void HideAds()
        {
#if USE_MAX
            if (Status != AdsEvents.ShowSuccess && Status != AdsEvents.Click)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + " is not showing --> return");
                return;
            }

            if (IsReady)
            {
                MaxSdk.HideBanner(adsUnitID);
                MRecDestroy();
                OnAdsClosed();
            }
#endif
        }

        public Vector2Int SetAdCustomPosition(AdsPos position, Vector2Int offset)
        {
            if (!IsAdsReady())
            {
                return Vector2Int.zero;
            }

            var density = MaxSdkUtils.GetScreenDensity();

            float adWidth = 300;
            float adHeight = 250;

            Debug.Log("AAAAA " + "adWidthMrec: " + adWidth + " adHeightMrec: " + adHeight);

            var safeAreaWidth = Screen.width / density;
            var safeAreaHeight = Screen.height / density;

            int xMax = (int)(safeAreaWidth - adWidth);
            int yMax = (int)(safeAreaHeight - adHeight);
            int xCenter = xMax / 2;
            int yCenter = yMax / 2;

            Vector2Int newPos = Vector2Int.zero;

            switch (position)
            {
                case AdsPos.Top:
                    newPos = new Vector2Int(xCenter + offset.x, offset.y);

                    break;
                case AdsPos.TopLeft:
                    newPos = new Vector2Int(offset.x, offset.y);

                    break;
                case AdsPos.TopRight:
                    newPos = new Vector2Int(xMax + offset.x, offset.y);

                    break;
                case AdsPos.Center:
                    newPos = new Vector2Int(xCenter + offset.x, yCenter + offset.y);

                    break;
                case AdsPos.CenterLeft:
                    newPos = new Vector2Int(offset.x, yCenter + offset.y);

                    break;
                case AdsPos.CenterRight:
                    newPos = new Vector2Int(xMax + offset.x, yCenter + offset.y);

                    break;
                case AdsPos.Bottom:
                    newPos = new Vector2Int(xCenter + offset.x, yMax + offset.y);

                    break;
                case AdsPos.BottomLeft:
                    newPos = new Vector2Int(offset.x, yMax + offset.y);

                    break;
                case AdsPos.BottomRight:
                    newPos = new Vector2Int(xMax + offset.x, yMax + offset.y);
                    break;
            }


            return newPos;
        }
    }
}