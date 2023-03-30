using System;
using System.Net;
using Newtonsoft.Json;

namespace VenlySDK.Core
{
    public struct VyTaskVoid
    {
        internal static VyTaskVoid Empty = new VyTaskVoid();
    }

    public struct VyUnit
    {
        internal static VyUnit Default = new VyUnit();
    }

    public class VyTaskResult
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("cancelled")] public bool Cancelled { get; set; }
        [JsonProperty("exception")] public Exception Exception { get; set; }

#if ENABLE_VENLY_API_SERVER
        [JsonIgnore] public HttpStatusCode? StatusCode;
#endif

        public VyTaskResult()
        {
            Success = true;
            Exception = null;
        }

        public VyTaskResult(Exception ex)
        {
            Success = false;
            Exception = ex;

            if (ex is OperationCanceledException) Cancelled = true;
        }
    }

    public class VyTaskResult<T> : VyTaskResult
    {
        [JsonProperty("data")] public T Data { get; set; }

        public VyTaskResult(Exception ex) : base(ex)
        {
            Data = default;
        }

        public VyTaskResult(T data) : base()
        {
            Data = data;
        }
    }
}