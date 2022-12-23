using UnityEngine;

namespace VenlySDK.Utils
{
    internal static class VenlyDebug
    {
        internal static bool EnableDebugLog { get; set; } = true;

        public static void LogDebug(string msg)
        {
            if(EnableDebugLog)
                Debug.Log(msg);
        }
    }
}
