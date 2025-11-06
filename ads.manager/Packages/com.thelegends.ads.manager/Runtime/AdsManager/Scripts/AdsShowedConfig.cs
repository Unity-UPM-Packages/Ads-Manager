using System;
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

    public class NativeShowedConfig : AdsShowedConfig
    {
        public string layoutName;
        public NativePlatformShowBuilder.CountdownConfig countdown;
        public NativePlatformShowBuilder.PositionConfig adsPos;
        public float reloadTime;
        public bool showOnLoaded;
    }
}
