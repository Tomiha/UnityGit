using System;
using System.Diagnostics;
using Proto.Promises;

#if UNITY_2017_1_OR_NEWER
using Debug = UnityEngine.Debug;
#endif

namespace Venly.Utils
{
//Exception Handling Utilities
    public static partial class VenlyUtils
    {
        //Exception Wrappers
        public static VenlyException WrapException(Exception ex)
        {
            return new VenlyException(ex);
        }

        public static VenlyException WrapException(string msg)
        {
            var sf = new StackFrame(1);
            var method = sf.GetMethod();

            return new VenlyException($"[VENLY-API] Exception @ {method.ReflectedType?.Name}::{method.Name} || {msg}");
        }

        public static void HandleReject<T>(object err, Promise<T>.Deferred? deferred = null)
        {
            if (deferred.HasValue) deferred.Value.Reject(err);
            else
            {
                if (err is Exception ex) HandleException(ex);
                else
                {
                    if(!VenlyAPI.HandleProviderError(err))
#if ENABLE_VENLY_AZURE
                        Console.WriteLine($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
#else
                        Debug.LogWarning($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
#endif
                }
            }
        }

        public static void HandleReject(object err, Promise.Deferred? deferred = null)
        {
            if (deferred.HasValue) deferred.Value.Reject(err);
            else
            {
                if (err is Exception ex) HandleException(ex);
                else
                {
                    if (!VenlyAPI.HandleProviderError(err))
#if ENABLE_VENLY_AZURE
                        Console.WriteLine($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
#else
                        Debug.LogWarning($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
#endif
                }
            }
        }

        //public static void HandleReject<T>(Exception ex, Promise<T>.Deferred? deferred = null)
        //{
        //    if (deferred.HasValue) deferred.Value.Reject(ex);
        //    else HandleException(ex);
        //}

        //public static void HandleReject(Exception ex, Promise.Deferred? deferred = null)
        //{
        //    if (deferred.HasValue) deferred.Value.Reject(ex);
        //    else HandleException(ex);
        //}

        //Unity Exception Handlers
#if UNITY_2017_1_OR_NEWER
        public static void HandleException(Exception ex)
        {
            Debug.LogException(ex);
        }
#endif

        //Azure Exception Handlers
#if ENABLE_VENLY_AZURE
    public static void HandleException(Exception ex)
    {
        throw ex;
    }
#endif
    }
}