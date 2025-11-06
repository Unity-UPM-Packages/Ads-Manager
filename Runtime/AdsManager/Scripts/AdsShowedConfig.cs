using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdsShowedConfig
    {
        public PlacementOrder order;
        public string position;
    }

    public class BannerShowedConfig : AdsShowedConfig
    {
    }

    public class MrecShowedConfig : AdsShowedConfig
    {
        public AdsPos adsPos;
        public Vector2 offset;
    }
}
