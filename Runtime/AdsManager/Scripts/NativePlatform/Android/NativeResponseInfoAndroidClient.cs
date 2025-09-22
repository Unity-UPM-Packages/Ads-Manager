#if USE_ADMOB
using GoogleMobileAds.Common;
using UnityEngine;
using System.Collections.Generic;

namespace TheLegends.Base.Ads
{
    internal class AdmobNativePlatformAndroidResponseInfoClient : IResponseInfoClient
    {
        private readonly AndroidJavaObject _kotlinController;

        private AndroidJavaObject _responseInfoJO;

        public AdmobNativePlatformAndroidResponseInfoClient(AndroidJavaObject kotlinController)
        {
            _kotlinController = kotlinController;
        }

        private AndroidJavaObject GetResponseInfoJavaObject()
        {
            if (_responseInfoJO == null && _kotlinController != null)
            {
                _responseInfoJO = _kotlinController.Call<AndroidJavaObject>("getResponseInfo");
            }
            return _responseInfoJO;
        }

        public string GetMediationAdapterClassName()
        {
            var responseInfo = GetResponseInfoJavaObject();
            return responseInfo?.Call<string>("getMediationAdapterClassName");
        }

        public string GetResponseId()
        {
            var responseInfo = GetResponseInfoJavaObject();
            return responseInfo?.Call<string>("getResponseId");
        }

        public override string ToString()
        {
            var responseInfo = GetResponseInfoJavaObject();
            return responseInfo?.Call<string>("toString");
        }
        public IAdapterResponseInfoClient GetLoadedAdapterResponseInfo() => null;
        public List<IAdapterResponseInfoClient> GetAdapterResponses() => new List<IAdapterResponseInfoClient>();

        public Dictionary<string, string> GetResponseExtras()
        {
            var responseInfo = GetResponseInfoJavaObject();
            if (responseInfo == null)
            {
                return new Dictionary<string, string>();
            }

            AndroidJavaObject bundle = responseInfo.Call<AndroidJavaObject>("getExtras");

            if (bundle == null)
            {
                return new Dictionary<string, string>();
            }

            return ConvertBundleToDictionary(bundle);
        }
        
        private Dictionary<string, string> ConvertBundleToDictionary(AndroidJavaObject bundle)
        {
            var dictionary = new Dictionary<string, string>();

            AndroidJavaObject keySet = bundle.Call<AndroidJavaObject>("keySet");
            AndroidJavaObject[] keys = keySet.Call<AndroidJavaObject[]>("toArray");

            foreach (var key in keys)
            {
                string keyString = key.Call<string>("toString");
                string valueString = bundle.Call<string>("getString", keyString);
                
                if (valueString != null)
                {
                    dictionary[keyString] = valueString;
                }
            }

            return dictionary;
        }
    }
}

#endif