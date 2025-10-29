#if USE_ADMOB && UNITY_IOS && !UNITY_EDITOR

using GoogleMobileAds.Common;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// iOS implementation của IAdErrorClient
    /// Simplified approach: Chỉ lưu error message (không có code)
    /// </summary>
    public class AdmobNativePlatformIOSAdErrorClient : IAdErrorClient
    {
        private readonly string _message;

        public AdmobNativePlatformIOSAdErrorClient(string errorMessage)
        {
            _message = errorMessage ?? "Unknown error";
        }

        /// <summary>
        /// iOS không có error code riêng như Android
        /// Return 0 để indicate không có code
        /// </summary>
        public int GetCode()
        {
            return 0;
        }

        /// <summary>
        /// Return error message từ native
        /// </summary>
        public string GetMessage()
        {
            return _message;
        }

        public override string ToString()
        {
            return $"AdmobNativePlatformIOSAdErrorClient: {_message}";
        }
    }
}

#endif
