using System;

namespace VenlySDK.Utils
{
    internal static class VenlyLog
    {
        internal static bool EnableDebugLog { get; set; } = true;

        public static void Debug(string msg)
        {
#if UNITY_EDITOR
            if(EnableDebugLog)
                UnityEngine.Debug.Log(msg);
#endif
        }

        public static void Exception(Exception ex)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogException(ex);
#endif

#if ENABLE_VENLY_AZURE
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Exception] {ex.Message}");
            Console.ForegroundColor = c;

            throw ex;
#endif
        }
    }
}
