using TheLegends.Base.Ads;
using UnityEngine;

public class PreloadAdsHelper : MonoBehaviour
{
    public void PreloadAds()
    {
        AdsManager.Instance.LoadNativePlatform(PlacementOrder.Two);
    }

    public void ShowAds()
    {
        Debug.Log("ShowAds");
    }
}
