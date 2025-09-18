using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class MaxNetworkController : AdsNetworkBase
    {
        private InitiationStatus status = InitiationStatus.NotInitialized;

        private List<MaxInterstitialController> interList = new List<MaxInterstitialController>();
        private List<MaxInterstitialOpenController> interOpenList = new List<MaxInterstitialOpenController>();
        private List<MaxRewardedController> rewardList = new List<MaxRewardedController>();
        private List<MaxAppOpenController> appOpenList = new List<MaxAppOpenController>();
        private List<MaxBannerController> bannerList = new List<MaxBannerController>();
        private List<MaxMrecController> mrecList = new List<MaxMrecController>();
        private List<MaxMrecOpenController> mrecOpenList = new List<MaxMrecOpenController>();

        public override IEnumerator DoInit()
        {
            status = InitiationStatus.Initializing;

#if (UNITY_ANDROID || UNITY_IOS) && USE_MAX
            var platform = Application.platform;
            var isIOS = platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.OSXPlayer;

            var bannerIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.bannerIds, AdsManager.Instance.SettingsAds.MAX_Android.bannerIds);
            CreateAdController(bannerIds, bannerList);

            var interIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.interIds, AdsManager.Instance.SettingsAds.MAX_Android.interIds);
            CreateAdController(interIds, interList);

            var interOpenIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.interOpenIds, AdsManager.Instance.SettingsAds.MAX_Android.interOpenIds);
            CreateAdController(interOpenIds, interOpenList);

            var rewardedIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.rewardIds, AdsManager.Instance.SettingsAds.MAX_Android.rewardIds);
            CreateAdController(rewardedIds, rewardList);

            var mrecIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.mrecIds, AdsManager.Instance.SettingsAds.MAX_Android.mrecIds);
            CreateAdController(mrecIds, mrecList);

            var mrecOpenIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.mrecOpenIds, AdsManager.Instance.SettingsAds.MAX_Android.mrecOpenIds);
            CreateAdController(mrecOpenIds, mrecOpenList);

            var appOpenIds = GetAdUnitIds(isIOS, AdsManager.Instance.SettingsAds.MAX_iOS.appOpenIds, AdsManager.Instance.SettingsAds.MAX_Android.appOpenIds);
            CreateAdController(appOpenIds, appOpenList);

            MaxSdkCallbacks.OnSdkInitializedEvent += (sdkConfiguration) =>
            {
                if (sdkConfiguration.IsSuccessfullyInitialized)
                {
                    status = InitiationStatus.Initialized;
                    AdsManager.Instance.Log($"{TagLog.MAX} " + "Max SDK initialized");
                }
                else
                {
                    status = InitiationStatus.Failed;
                    AdsManager.Instance.Log($"{TagLog.MAX} " + "Max SDK initialization failed");
                }
            };

            MaxSdk.InitializeSdk();

            while (status == InitiationStatus.Initializing)
            {
                yield return null;
            }

#endif
        }

        private List<Placement> GetAdUnitIds(bool isIOS, List<Placement> iosIds, List<Placement> androidIds)
        {
            return isIOS ? iosIds : androidIds;
        }

        private void CreateAdController<T>(List<Placement> placements, List<T> adList) where T : AdsPlacementBase
        {
            if (placements.Count <= 0)
            {
                AdsManager.Instance.LogError($"{TagLog.MAX} {typeof(T).Name} IDs NULL or Empty --> return");
                return;
            }

            foreach (var adId in placements)
            {
                var adController = new GameObject().AddComponent<T>();
                adController.name = typeof(T).Name;
                adController.transform.parent = this.transform;
                ((AdsPlacementBase)adController).Init(adId);
                adList.Add(adController);
            }
        }

        public override AdsEvents GetAdsStatus(AdsType type, PlacementOrder order)
        {
            var listPlacement = new List<AdsPlacementBase>();

            switch (type)
            {
                case AdsType.Banner:
                    listPlacement = bannerList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Interstitial:
                    listPlacement = interList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Rewarded:
                    listPlacement = rewardList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.Mrec:
                    listPlacement = mrecList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.AppOpen:
                    listPlacement = appOpenList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.MrecOpen:
                    listPlacement = mrecOpenList.Cast<AdsPlacementBase>().ToList();
                    break;
                case AdsType.InterOpen:
                    listPlacement = interOpenList.Cast<AdsPlacementBase>().ToList();
                    break;
            }

            if (!IsListExist(listPlacement))
            {
                return AdsEvents.None;
            }

            var index = GetPlacementIndex((int)order, listPlacement.Count);

            if (index == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.MAX} {type} {order} is not exist");
                return AdsEvents.None;
            }

            return listPlacement[index].Status;
        }

        private bool IsListExist<T>(List<T> list) where T : AdsPlacementBase
        {
            bool isExist = false;
            isExist = list.Count > 0;

            if (!isExist)
            {
                AdsManager.Instance.LogError($"{typeof(T).Name} is empty");
            }

            return isExist;
        }

        private int GetPlacementIndex(int order, int listCount)
        {
            if (listCount <= 0)
            {
                return -1;
            }

            if (order > listCount)
            {
                return -1;
            }

            return Mathf.Clamp(order - 1, 0, listCount - 1);
        }

        public override AdsNetworks GetNetworkType()
        {
            return AdsNetworks.Max;
        }

        public override void HideAllBanner()
        {
            foreach (var banner in bannerList)
            {
                banner.HideAds();
            }
        }

        public override void HideAllMrec()
        {
            foreach (var mrec in mrecList)
            {
                mrec.HideAds();
            }
        }

        public override void LoadInterstitial(AdsType interType, PlacementOrder order)
        {
            var list = interType == AdsType.InterOpen ? (new List<MaxInterstitialController>(interOpenList)) : interList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.MAX} {interType} {order} is not exist");
                return;
            }

            list[placementIndex].LoadAds();
        }

        public override void ShowInterstitial(AdsType interType, PlacementOrder order, string position, Action OnClose = null)
        {
            var list = interType == AdsType.InterOpen ? (new List<MaxInterstitialController>(interOpenList)) : interList;

            if (!IsListExist(list))
            {
                return;
            }

            var placementIndex = GetPlacementIndex((int)order, list.Count);

            if (placementIndex == -1)
            {
                AdsManager.Instance.LogError($"{TagLog.MAX} {interType} {order} is not exist");
                return;
            }

            list[placementIndex].ShowAds(position, OnClose);
        }

        public override void LoadRewarded(PlacementOrder order)
        {
            throw new NotImplementedException();
        }

        public override void ShowRewarded(PlacementOrder order, string position, Action OnRewarded = null)
        {
            throw new NotImplementedException();
        }

        public override void LoadAppOpen(PlacementOrder order)
        {
            throw new NotImplementedException();
        }

        public override void ShowAppOpen(PlacementOrder order, string position)
        {
            throw new NotImplementedException();
        }

        public override void LoadBanner(PlacementOrder order)
        {
            throw new NotImplementedException();
        }

        public override void ShowBanner(PlacementOrder order, string position)
        {
            throw new NotImplementedException();
        }

        public override void HideBanner(PlacementOrder order)
        {
            throw new NotImplementedException();
        }

        public override void LoadMrec(AdsType mrecType, PlacementOrder order)
        {
            throw new NotImplementedException();
        }

        public override void ShowMrec(AdsType mrecType, PlacementOrder order, AdsPos mrecPosition, Vector2Int offset, string position)
        {
            throw new NotImplementedException();
        }

        public override void HideMrec(AdsType mrecType, PlacementOrder order)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAds()
        {
            foreach (var ad in bannerList)
            {
                ad.HideAds();
            }

            foreach (var ad in mrecList)
            {
                ad.HideAds();
            }
        }

        public override bool IsAdsReady(AdsType adsType, PlacementOrder order)
        {
            int orderIndex = -1;
            switch (adsType)
            {
                case AdsType.Banner:
                    orderIndex = GetPlacementIndex((int)order, bannerList.Count);
                    break;
                case AdsType.Interstitial:
                    orderIndex = GetPlacementIndex((int)order, interList.Count);
                    break;
                case AdsType.InterOpen:
                    orderIndex = GetPlacementIndex((int)order, interOpenList.Count);
                    break;
                case AdsType.Rewarded:
                    orderIndex = GetPlacementIndex((int)order, rewardList.Count);
                    break;
                case AdsType.Mrec:
                    orderIndex = GetPlacementIndex((int)order, mrecList.Count);
                    break;
                case AdsType.MrecOpen:
                    orderIndex = GetPlacementIndex((int)order, mrecOpenList.Count);
                    break;
                case AdsType.AppOpen:
                    orderIndex = GetPlacementIndex((int)order, appOpenList.Count);
                    break;
                default:
                    return false;
            }

            switch (adsType)
            {
                case AdsType.Banner:
                    return bannerList[orderIndex].IsAdsReady();
                case AdsType.Interstitial:
                    return interList[orderIndex].IsAdsReady();
                case AdsType.InterOpen:
                    return interOpenList[orderIndex].IsAdsReady();
                case AdsType.Rewarded:
                    return rewardList[orderIndex].IsAdsReady();
                case AdsType.Mrec:
                    return mrecList[orderIndex].IsAdsReady();
                case AdsType.MrecOpen:
                    return mrecOpenList[orderIndex].IsAdsReady();
                case AdsType.AppOpen:
                    return appOpenList[orderIndex].IsAdsReady();
                default:
                    return false;
            }
        }

    }
}
