using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Shared
{
    [Serializable]
    public class VyApiResponseDto
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("pagination")] public VyApiResponsePagination Pagination;
        [JsonProperty("errors")] public VyApiReponseError[] Errors;
    }

    [Serializable]
    public class VyApiResponseDto<T> : VyApiResponseDto
    {
        [JsonProperty("result")] public T Data;

        public static VyApiResponseDto<T> FromException(Exception ex)
        {
            return new()
            {
                Success = false,
                Data = default,
                Errors = new[] {VyApiReponseError.FromException(ex)}
            };
        }
    }

    [Serializable]
    public struct VyApiReponseError
    {
        [JsonProperty("code")] public string Code;
        [JsonProperty("errorCode")] private string _errorCode { set => Code = value; } //Legacy Fallback
        [JsonProperty("traceCode")] public string TraceCode;
        [JsonProperty("message")] public string Message;
        [JsonProperty("errorMessage")] private string _errorMessage { set => Message = value; } //Legacy Fallback
        [JsonProperty("id")] public string Id;

        public static VyApiReponseError FromException(Exception ex)
        {
            var rootEx = ex;
            while (rootEx.InnerException != null)
            {
                rootEx = rootEx.InnerException;
            }

            return new VyApiReponseError()
            {
                Code = "PLAYFAB AZURE ERROR",
                TraceCode = "null",
                Message = rootEx.Message
            };
        }
    }

    [Serializable]
    public struct VyApiResponsePagination
    {
        [JsonProperty("pageNumber")] public int PageNumber;
        [JsonProperty("pageSize")] public int PageSize;
        [JsonProperty("numberOfPages")] public int NumberOfPages;
        [JsonProperty("numberOfElements")] public int NumberOfElements;
        [JsonProperty("hasNextPage")] public bool HasNextPage;
        [JsonProperty("hasPreviousPage")] public bool HasPreviousPage;
    }
}