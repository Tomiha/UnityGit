//using System.Net;
//using Newtonsoft.Json;

//namespace VenlySDK.Core
//{
//    public class VyTaskResult
//    {
//        [JsonProperty("success")] public bool Success;
//        [JsonProperty("exception")] public VyException Exception;

//        public VyTaskResult<VyTaskVoid> ToVoidResult()
//        {
//            return new VyTaskResult<VyTaskVoid>()
//            {
//                Success = Success,
//                Exception = Exception,
//                Data = VyTaskVoid.Empty
//            };
//        }
//    }

//    public class VyTaskResult<T>
//    {
//        [JsonProperty("data")] public T Data;
//        [JsonProperty("success")] public bool Success;
//        [JsonProperty("exception")] public VyException Exception;

//#if ENABLE_VENLY_AZURE
//        [JsonIgnore] public HttpStatusCode? StatusCode;
//#endif

//        public VyTaskResult(T data)
//        {
//            Success = true;
//            Data = data;
//        }

//        public VyTaskResult(VyException ex = null)
//        {
//            Success = false;
//            Exception = ex;
//        }

//        public VyTaskResult ToVoidResult()
//        {
//            return new VyTaskResult
//            {
//                Success = Success,
//                Exception = Exception
//            };
//        }
//    }
//}
