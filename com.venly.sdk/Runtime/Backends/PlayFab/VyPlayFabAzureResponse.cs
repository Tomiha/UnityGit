using System;
using System.Net;
using Newtonsoft.Json;

#if ENABLE_VENLY_AZURE
using Microsoft.AspNetCore.Mvc;
#endif

namespace Venly.Backends.PlayFab
{
    public class VyPlayFabAzureResponse
    {
        [JsonProperty("success")] public bool Success { get; private set; }
        [JsonProperty("data")] public string Data { get; private set; }
        [JsonProperty("errorMessage")] public string ErrorMessage { get; private set; }
        [JsonProperty("statusCode")] public HttpStatusCode StatusCode { get; private set; }
        [JsonProperty("isRawResponse")] public bool IsRawResponse { get; private set; }


#if ENABLE_VENLY_AZURE
        public static JsonResult FromTaskResult<T>(VyTaskResult<T> taskResult, bool isRawResponse = false)
        {
            if (taskResult.Success)
            {
                var response = new VyPlayFabAzureResponse
                {
                    Success = true,
                    Data = JsonConvert.SerializeObject(taskResult.Data),
                    StatusCode = taskResult.StatusCode??HttpStatusCode.OK,
                    IsRawResponse = isRawResponse
                };

                return new JsonResult(response)
                {
                    StatusCode = (int) HttpStatusCode.OK
                };
            }
            
            return FromException(taskResult.Exception);
        }

        public static JsonResult FromException(Exception ex, bool isRawResponse = false)
        {
            var response = new VyPlayFabAzureResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                StatusCode = HttpStatusCode.InternalServerError,
                IsRawResponse = isRawResponse
            };

            return new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
#endif
    }
}
