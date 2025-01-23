using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    [Serializable]
    public class AdmobUnitID
    {
        public string appID = string.Empty;
        public List<Placement> bannerIds = new List<Placement>();
        public List<Placement> interIds = new List<Placement>();
        public List<Placement> rewardIds = new List<Placement>();
        public List<Placement> mrecIds = new List<Placement>();
        public List<Placement> appOpenIds = new List<Placement>();
        public List<Placement> interOpenIds = new List<Placement>();
        public List<Placement> mrecOpenIds = new List<Placement>();
        public List<Placement> nativeOverlayIds = new List<Placement>();
    }

    [Serializable]
    public class MaxUnitID
    {
        public List<Placement> bannerIds = new List<Placement>();
        public List<Placement> interIds = new List<Placement>();
        public List<Placement> rewardIds = new List<Placement>();
        public List<Placement> mrecIds = new List<Placement>();
        public List<Placement> appOpenIds = new List<Placement>();
    }



    [Serializable]
    public class Placement
    {
        public List<string> stringIDs = new List<string>();
    }
}
