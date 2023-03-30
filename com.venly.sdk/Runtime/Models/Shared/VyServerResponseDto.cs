using System;
using Newtonsoft.Json;
using VenlySDK.Core;

namespace VenlySDK.Models.Shared
{
    public class VyServerResponseDto
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("data")] public string Data;
        [JsonProperty("statusCode")] public int StatusCode;
        [JsonProperty("errorMessage")] public string ErrorMessage;
        [JsonProperty("isRawResponse")] public bool IsRawResponse;

#if ENABLE_VENLY_API_SERVER
        public static VyServerResponseDto FromTaskResult(VyTaskResult<string> taskResult, bool isRawResponse)
        {
            var response = new VyServerResponseDto();
            response.Success = taskResult.Success;
            response.StatusCode = taskResult.StatusCode.HasValue ? (int)taskResult.StatusCode : -1;
            response.IsRawResponse = isRawResponse;

            if (taskResult.Success)
            {
                response.Data = taskResult.Data;
                return response;
            }

            response.ErrorMessage = taskResult.Exception.Message;
            return response;
        }

        public static VyServerResponseDto Succeeded<T>(T data, bool isRawResponse = true)
        {
            return new VyServerResponseDto
            {
                Success = true,
                StatusCode = 200,
                IsRawResponse = isRawResponse,
                Data = JsonConvert.SerializeObject(data)
            };
        }

        public static VyServerResponseDto Failed(Exception ex, int statusCode = 500)
        {
            return Failed(ex.Message, statusCode);
        }

        public static VyServerResponseDto Failed(string errorMessage, int statusCode = 500)
        {
            return new VyServerResponseDto
            {
                Success = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                IsRawResponse = true
            };
        }

        public static VyServerResponseDto FromTaskResult<T>(VyTaskResult<T> taskResult, bool isRawResponse)
        {
            if (taskResult.Success)
            {
                return Succeeded(taskResult.Data, isRawResponse);
            }

            var statusCode = taskResult.StatusCode.HasValue ? (int)taskResult.StatusCode : -1;
            return Failed(taskResult.Exception, statusCode);

            //var response = new VyServerResponseDto();
            //response.Success = taskResult.Success;
            //response.StatusCode = taskResult.StatusCode.HasValue ? (int)taskResult.StatusCode : -1;
            //response.IsRawResponse = isRawResponse;

            //if (taskResult.Success)
            //{
            //    response.Data = JsonConvert.SerializeObject(taskResult.Data);
            //    return response;
            //}

            //response.ErrorMessage = taskResult.Exception.Message;
            //return response;
        }

        public static VyServerResponseDto FromException(Exception ex)
        {
            return Failed(ex.Message);
        }
#endif
    }
}
