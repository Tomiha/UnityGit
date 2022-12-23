using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Proto.Promises;
using VenlySDK.Core;
#if UNITY_2017_1_OR_NEWER
using Debug = UnityEngine.Debug;
#endif

namespace VenlySDK.Utils
{
//Exception Handling Utilities
    public static partial class VenlyUtils
    {
//        //Exception Wrappers
//        public static VyException WrapException(Exception ex)
//        {
//            return new VyException(ex);
//        }

//        public static VyException WrapException(string msg)
//        {
//            var sf = new StackFrame(1);
//            var method = sf.GetMethod();

//            return new VyException($"[VENLY-API] Exception @ {method.ReflectedType?.Name}::{method.Name} || {msg}");
//        }

//        public static void HandleReject<T>(object err, Promise<T>.Deferred? deferred = null)
//        {
//            if (deferred.HasValue) deferred.Value.Reject(err);
//            else
//            {
//                if (err is Exception ex) HandleException(ex);
//                else
//                {
//                    if (!Venly.HandleProviderError(err))
//#if ENABLE_VENLY_AZURE
//                        Console.WriteLine($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
//#else
//                        Debug.LogWarning($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
//#endif
//                }
//            }
//        }

//        public static void HandleReject(object err, Promise.Deferred? deferred = null)
//        {
//            if (deferred.HasValue) deferred.Value.Reject(err);
//            else
//            {
//                if (err is Exception ex) HandleException(ex);
//                else
//                {
//                    if (!Venly.HandleProviderError(err))
//#if ENABLE_VENLY_AZURE
//                        Console.WriteLine($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
//#else
//                        Debug.LogWarning($"Unhandled reject (unknown type \'{err.GetType().Name}\')");
//#endif
//                }
//            }
//        }

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

        public static Exception SetStackTrace(this Exception target, StackTrace stack) => _SetStackTrace(target, stack);

        private static readonly Func<Exception, StackTrace, Exception> _SetStackTrace =
            new Func<Func<Exception, StackTrace, Exception>>(() =>
            {
                ParameterExpression target = Expression.Parameter(typeof(Exception));
                ParameterExpression stack = Expression.Parameter(typeof(StackTrace));
                Type traceFormatType = typeof(StackTrace).GetNestedType("TraceFormat", BindingFlags.NonPublic);
                MethodInfo toString = typeof(StackTrace).GetMethod("ToString",
                    BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {traceFormatType}, null);
                object normalTraceFormat = Enum.GetValues(traceFormatType).GetValue(0);
                MethodCallExpression stackTraceString = Expression.Call(stack, toString,
                    Expression.Constant(normalTraceFormat, traceFormatType));
                FieldInfo stackTraceStringField = typeof(Exception).GetField("_stackTraceString",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                BinaryExpression assign =
                    Expression.Assign(Expression.Field(target, stackTraceStringField), stackTraceString);
                return Expression
                    .Lambda<Func<Exception, StackTrace, Exception>>(Expression.Block(assign, target), target, stack)
                    .Compile();
            })();
    }
}