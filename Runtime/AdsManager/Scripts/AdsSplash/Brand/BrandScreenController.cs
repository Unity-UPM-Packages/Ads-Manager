using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using TheLegends.Base.Firebase;

namespace TheLegends.Base.Ads
{
    public class BrandScreenController : MonoBehaviour
    {
        private AdsSplashController adsSplashController;

        [SerializeField]
        private BrandItemController[] brandItems;

        [SerializeField]
        private Text autoCloseTxt;
        [SerializeField]
        private Button confirmBtn;

        private int numChoose = 0;

        private void OnEnable()
        {
            confirmBtn.onClick.AddListener(ActionConfirm);
        }

        private void OnDisable()
        {
            confirmBtn.onClick.RemoveListener(ActionConfirm);
        }

        private void ActionConfirm()
        {
            FirebaseManager.Instance.LogEvent("splash_screen_confirm");
            StopAllCoroutines();
            ActiveScreen(false);
            PlayerPrefs.SetInt("canShowSelectBrand", 0);
            adsSplashController.CompleteSplash();
        }

        private void Start()
        {
            confirmBtn.gameObject.SetActive(false);

            foreach (var item in brandItems)
            {
                item.SetSelected(false);
            }

        }

        public void Show()
        {
            FirebaseManager.Instance.LogEvent("splash_screen_show");

            if (PlayerPrefs.GetInt("splash_screen_show_first", 0) == 0)
            {
                PlayerPrefs.SetInt("splash_screen_show_first", 1);
                FirebaseManager.Instance.LogEvent("splash_screen_show_first");
            }

            ActiveScreen(true);
            StartCoroutine(IEAutoClose(10));
        }

        public void OnItemSelected(BrandItemController branItem)
        {
            numChoose++;

            foreach (var item in brandItems)
            {
                item.SetSelected(item == branItem);
            }

            if (numChoose == 1)
            {
                StopAllCoroutines();
                StartCoroutine(IEAutoClose(6));

                LMotion.Create(0f, 1f, 2f).WithOnComplete(() =>
                {
                    confirmBtn.gameObject.SetActive(true);
                }).RunWithoutBinding();
            }

        }

        private IEnumerator IEAutoClose(float time)
        {
            float eslapeTime = 0;

            while (eslapeTime < time)
            {
                eslapeTime += Time.deltaTime;
                autoCloseTxt.text = "AUTO CLOSE AND SHOW LATER IN " + (time - eslapeTime).ToString("#0") + "s";
                yield return null;
            }

            ActiveScreen(false);

            adsSplashController.CompleteSplash();
        }

        private void ActiveScreen(bool isActive)
        {
            if (!isActive)
            {
                AdsManager.Instance.HideMrec(AdsType.MrecOpen, PlacementOrder.One);
            }
            gameObject.SetActive(isActive);
        }

        public void SetAdsSplashController(AdsSplashController adsSplashController)
        {
            this.adsSplashController = adsSplashController;
        }

    }
}
