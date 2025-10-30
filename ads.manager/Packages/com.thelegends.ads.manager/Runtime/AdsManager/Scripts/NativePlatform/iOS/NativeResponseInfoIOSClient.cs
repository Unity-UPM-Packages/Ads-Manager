#if USE_ADMOB && UNITY_IOS && !UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using GoogleMobileAds.Common;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// iOS implementation của IResponseInfoClient
    /// Sử dụng handle-based approach matching Android pattern:
    /// - Lưu controller handle
    /// - Gọi bridge functions để retrieve response info
    /// </summary>
    public class NativeResponseInfoIOSClient : IResponseInfoClient
    {
        private readonly IntPtr _controllerHandle;

        // MARK: - DllImport Response Info Functions

        [DllImport("__Internal")]
        private static extern IntPtr AdmobNative_GetResponseId(IntPtr handle);

        [DllImport("__Internal")]
        private static extern IntPtr AdmobNative_GetMediationAdapterClassName(IntPtr handle);

        // MARK: - Constructor

        public NativeResponseInfoIOSClient(IntPtr controllerHandle)
        {
            _controllerHandle = controllerHandle;
        }

        // MARK: - IResponseInfoClient Implementation

        public string GetMediationAdapterClassName()
        {
            if (_controllerHandle == IntPtr.Zero)
            {
                Debug.LogWarning("NativeResponseInfoIOSClient: Controller handle is null");
                return null;
            }

            IntPtr resultPtr = AdmobNative_GetMediationAdapterClassName(_controllerHandle);

            if (resultPtr == IntPtr.Zero)
            {
                Debug.LogWarning("NativeResponseInfoIOSClient: GetMediationAdapterClassName returned null");
                return null;
            }

            string result = Marshal.PtrToStringAnsi(resultPtr);
            Debug.Log($"NativeResponseInfoIOSClient: GetMediationAdapterClassName = {result}");

            return result;
        }

        public string GetResponseId()
        {
            if (_controllerHandle == IntPtr.Zero)
            {
                Debug.LogWarning("NativeResponseInfoIOSClient: Controller handle is null");
                return null;
            }

            IntPtr resultPtr = AdmobNative_GetResponseId(_controllerHandle);

            if (resultPtr == IntPtr.Zero)
            {
                Debug.LogWarning("NativeResponseInfoIOSClient: GetResponseId returned null");
                return null;
            }

            string result = Marshal.PtrToStringAnsi(resultPtr);
            Debug.Log($"NativeResponseInfoIOSClient: GetResponseId = {result}");

            return result;
        }

        public IAdapterResponseInfoClient GetLoadedAdapterResponseInfo()
        {
            // iOS: Không implement chi tiết adapter response info
            // Trả về null như Android
            return null;
        }

        public System.Collections.Generic.List<IAdapterResponseInfoClient> GetAdapterResponses()
        {
            // iOS: Không implement chi tiết adapter responses
            // Trả về empty list như Android
            return new System.Collections.Generic.List<IAdapterResponseInfoClient>();
        }

        public System.Collections.Generic.Dictionary<string, string> GetResponseExtras()
        {
            // iOS: Không implement response extras
            // Trả về empty dictionary như Android khi không có extras
            return new System.Collections.Generic.Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"NativeResponseInfoIOSClient [ResponseId: {GetResponseId()}, AdapterClass: {GetMediationAdapterClassName()}]";
        }
    }
}

#endif
