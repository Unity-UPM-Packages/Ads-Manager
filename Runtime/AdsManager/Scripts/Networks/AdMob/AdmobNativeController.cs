using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using TheLegends.Base.Ads;
using UnityEngine;
using UnityEngine.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeController : AdsPlacementBase
    {
        private NativeAd _nativeAd;

        [SerializeField]
        private GameObject container;

        [SerializeField]
        private PlacementOrder _order = PlacementOrder.One;

        [SerializeField]
        private string positionNative = "default";

        private float timeAutoReload;
        private bool isCLosedByHide = false;

        [Space(10)]
        [Header("Native Components")]
        [SerializeField]
        private Image adImage;

        [SerializeField]
        private BoxCollider adImageCollider;

        [SerializeField]
        private Image callToAction;

        [SerializeField]
        private Text callToActionText;

        [SerializeField]
        private Image adChoice;

        [SerializeField]
        private Image adIcon;

        [SerializeField]
        private Text advertiser;

        [SerializeField]
        private Text adHeadline;

        [SerializeField]
        private Text adBody;

        [SerializeField]
        private Text store;

        [SerializeField]
        private Image starFilling;

        [SerializeField]
        private Text price;


        public Action onClick = null;

        private string _currentLoadRequestId;
        private string _loadRequestId;



        private void Start()
        {
            container.SetActive(false);

            position = positionNative;

            var platform = Application.platform;
            var isIOS = platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.OSXPlayer;
            var isAdmobTest = AdsManager.Instance.SettingsAds.isAdmobTest;

            var placementIndex = Mathf.Clamp((int)_order, 1,
                isIOS
                    ? AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.nativeIds.Count
                    : AdsManager.Instance.SettingsAds.ADMOB_Android_Test.nativeIds.Count) - 1;

            placement = isAdmobTest
                ? (isIOS
                    ? AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.nativeIds[placementIndex]
                    : AdsManager.Instance.SettingsAds.ADMOB_Android_Test.nativeIds[placementIndex])
                : (isIOS
                    ? AdsManager.Instance.SettingsAds.ADMOB_IOS.nativeIds[placementIndex]
                    : AdsManager.Instance.SettingsAds.ADMOB_Android.nativeIds[placementIndex]);

            timeAutoReload = AdsManager.Instance.adsConfigs.adNativeTimeReload;

            Init(placement);
        }


        public override void LoadAds()
        {
#if USE_ADMOB
            if (!IsCanLoadAds())
            {
                return;
            }

            NativeDestroy();
            base.LoadAds();

            AdLoader adLoader = new AdLoader.Builder(adsUnitID)
                .ForNativeAd()
                .Build();

            adLoader.OnNativeAdLoaded += OnNativeLoaded;
            adLoader.OnAdFailedToLoad += OnNativeLoadFailed;
            adLoader.OnNativeAdImpression += OnImpression;
            adLoader.OnNativeAdClicked += OnAdsClick;
            adLoader.OnNativeAdClosed += OnAdsClose;
            adLoader.LoadAd(new AdRequest());

#if UNITY_EDITOR
            OnNativeLoaded(this, null);
#endif

#endif
        }


        public override void ShowAds(string showPosition)
        {
#if USE_ADMOB
            position = showPosition;

            if (Status == AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "is showing --> return");

                return;
            }

            base.ShowAds(showPosition);

#if UNITY_EDITOR
            if (IsAvailable)
            {
                Status = AdsEvents.ShowSuccess;
                container.SetActive(true);
                isCLosedByHide = false;

                if (adImageCollider && AdsManager.Instance.adsConfigs.adNativeBannerHeight > 0)
                {
                    adImageCollider.size = new Vector3(adImage.rectTransform.rect.width,
                        AdsManager.Instance.adsConfigs.adNativeBannerHeight, adImageCollider.size.z);
                }

                DelayReloadAd(timeAutoReload);
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }

#else
            if (IsReady && IsAvailable)
            {
                _nativeAd.OnPaidEvent += OnAdsPaid;
                Status = AdsEvents.ShowSuccess;
                isCLosedByHide = false;
                FetchData();
                container.SetActive(true);
                DelayReloadAd(timeAutoReload);
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif

#endif
        }


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
            return AdsType.Native;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            return _nativeAd != null;
#else
            return false;
#endif
        }

        #region Internal

        private void NativeDestroy()
        {
#if USE_ADMOB
            try
            {
                container.SetActive(false);

                CancelReloadAds();

                if (_nativeAd != null)
                {
                    _nativeAd.OnPaidEvent -= OnAdsPaid;
                    _nativeAd.Destroy();
                    _nativeAd = null;
                }
            }
            catch (Exception ex)
            {
                AdsManager.Instance.LogException(ex);
            }

#endif
        }

        private void OnNativeLoaded(object sender, NativeAdEventArgs args)
        {
#if USE_ADMOB
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (_loadRequestId != _currentLoadRequestId)
                {
                    // If the load request ID does not match, this callback is from a previous request
                    return;
                }

                if (loadTimeOutCoroutine != null)
                {
                    StopCoroutine(loadTimeOutCoroutine);
                    loadTimeOutCoroutine = null;
                }

                base.OnAdsLoadAvailable();

                if (args != null)
                {
                    _nativeAd = args.nativeAd;
                }

                if (isCLosedByHide)
                {
                    AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "last closed by Hide() --> return");

                    return;
                }

                ShowAds(position);
            });
#endif
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void OnNativeLoadFailed(object sender, AdFailedToLoadEventArgs error)
#pragma warning restore CS0618 // Type or member is obsolete
        {
#if USE_ADMOB
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (_loadRequestId != _currentLoadRequestId)
                {
                    // If the load request ID does not match, this callback is from a previous request
                    return;
                }

                if (loadTimeOutCoroutine != null)
                {
                    StopCoroutine(loadTimeOutCoroutine);
                    loadTimeOutCoroutine = null;
                }

                var errorDescription = error.LoadAdError.GetMessage();
                base.OnAdsLoadFailed(errorDescription);
            });
#endif
        }

        private void OnAdsClose(object sender, EventArgs args)
        {
#if USE_ADMOB
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                base.OnAdsClosed();
            });
#endif
        }

        private void OnAdsClick(object sender, EventArgs args)
        {
#if USE_ADMOB
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                base.OnAdsClick();
                onClick?.Invoke();
            });
#endif
        }

        private void OnImpression(object sender, EventArgs args)
        {
#if USE_ADMOB
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                base.OnImpression();
            });
#endif
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void OnAdsPaid(object sender, AdValueEventArgs args)
#pragma warning restore CS0618 // Type or member is obsolete
        {
#if USE_ADMOB
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, args.AdValue);
            });
#endif
        }

        private void FetchData()
        {
            if (adImage)
            {
                var images = _nativeAd.GetImageTextures();

                if (images.Count > 0)
                {
                    adImage.sprite = Sprite.Create(images[0], new Rect(0, 0, images[0].width, images[0].height),
                        new Vector2(0.5f, 0.5f));
                }

                if (adImageCollider && AdsManager.Instance.adsConfigs.adNativeBannerHeight > 0)
                {
                    adImageCollider.size = new Vector3(adImage.rectTransform.rect.width,
                        AdsManager.Instance.adsConfigs.adNativeBannerHeight, adImageCollider.size.z);
                }

                _nativeAd.RegisterImageGameObjects(new List<GameObject> { adImage.gameObject });
            }

            if (callToAction && callToActionText)
            {
                callToActionText.text = _nativeAd.GetCallToActionText().ToLower();

                _nativeAd.RegisterCallToActionGameObject(callToAction.gameObject);
            }

            if (adChoice)
            {
                Texture2D choice = _nativeAd.GetAdChoicesLogoTexture();

                if (choice != null)
                {
                    adChoice.sprite = Sprite.Create(choice, new Rect(0, 0, choice.width, choice.height),
                        new Vector2(0.5f, 0.5f));
                }

                _nativeAd.RegisterAdChoicesLogoGameObject(adChoice.gameObject);
            }

            if (adIcon)
            {
                Texture2D icon = _nativeAd.GetIconTexture();

                if (icon != null)
                {
                    adIcon.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height),
                        new Vector2(0.5f, 0.5f));
                }

                _nativeAd.RegisterIconImageGameObject(adIcon.gameObject);
            }

            if (advertiser)
            {
                advertiser.text = _nativeAd.GetAdvertiserText();

                _nativeAd.RegisterAdvertiserTextGameObject(advertiser.gameObject);
            }

            if (adHeadline)
            {
                adHeadline.text = _nativeAd.GetHeadlineText();

                _nativeAd.RegisterHeadlineTextGameObject(adHeadline.gameObject);
            }

            if (adBody)
            {
                adBody.text = _nativeAd.GetBodyText();

                _nativeAd.RegisterBodyTextGameObject(adBody.gameObject);
            }

            if (store)
            {
                store.text = _nativeAd.GetStore();

                _nativeAd.RegisterStoreGameObject(store.gameObject);
            }

            if (price)
            {
                price.text = _nativeAd.GetPrice();

                _nativeAd.RegisterPriceGameObject(price.gameObject);
            }

            if (starFilling)
            {
                double storeStarRating = _nativeAd.GetStarRating();

                if (storeStarRating is double.NaN || storeStarRating <= 0)
                {
                    storeStarRating = 4.25;
                }

                starFilling.fillAmount = (float)(storeStarRating * 0.2f);
            }
        }

        public void HideAds()
        {
            isCLosedByHide = true;
            NativeDestroy();
            base.OnAdsClosed();
        }

        private void DelayReloadAd(float time)
        {
            Invoke(nameof(LoadAds), time);
        }

        private void CancelReloadAds()
        {
            CancelInvoke(nameof(LoadAds));
        }

        #endregion
    }
}
