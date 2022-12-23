using System;
using System.Net;
using Newtonsoft.Json;
using VenlySDK.Core;

#if ENABLE_VENLY_AZURE
using Microsoft.AspNetCore.Mvc;
#endif

namespace VenlySDK.Backends.PlayFab
{
    public class VyPlayFabAzureResponse<T>
    {
        [JsonProperty("success")] public bool Success { get; private set; }
        [JsonProperty("data")] public T Data { get; private set; }
        [JsonProperty("errorMessage")] public string ErrorMessage { get; private set; }

#if ENABLE_VENLY_AZURE
        public static JsonResult FromObject<T0>(T0 obj)
        {
            var response = new VyPlayFabAzureResponse<T0>
            {
                Success = true,
                Data = obj
            };

            return new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public static JsonResult FromTaskResult<T0>(VyTaskResult<T0> taskResult)
        {
            if (taskResult.Success)
            {
                var response = new VyPlayFabAzureResponse<T0>
                {
                    Success = true,
                    Data = taskResult.Data
                };

                return new JsonResult(response)
                {
                    StatusCode = (int) HttpStatusCode.OK
                };
            }
            
            return FromException(taskResult.Exception);
        }

        public static JsonResult FromException(Exception ex)
        {
            var response = new VyPlayFabAzureResponse<object>
            {
                Success = false,
                ErrorMessage = ex.Message
            };

            return new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
#endif
    }
}
