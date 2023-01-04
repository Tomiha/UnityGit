using System;
using Newtonsoft.Json;

namespace VenlySDK.Models
{
    [Serializable]
    public class VyResponseDto
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("pagination")] public VyResponsePagination Pagination;
        [JsonProperty("errors")] public VyReponseError[] Errors;
    }

    [Serializable]
    public class VyResponseDto<T> : VyResponseDto
    {
        [JsonProperty("result")] public T Data;

        public static VyResponseDto<T> FromException(Exception ex)
        {
            return new()
            {
                Success = false,
                Data = default,
                Errors = new[] {VyReponseError.FromException(ex)}
            };
        }
    }

    [Serializable]
    public struct VyReponseError
    {
        [JsonProperty("errorCode")] public string Code;
        [JsonProperty("traceCode")] public string TraceCode;
        [JsonProperty("errorMessage")] public string Message;

        public static VyReponseError FromException(Exception ex)
        {
            var rootEx = ex;
            while (rootEx.InnerException != null)
            {
                rootEx = rootEx.InnerException;
            }

            return new VyReponseError()
            {
                Code = "PLAYFAB AZURE ERROR",
                TraceCode = "null",
                Message = rootEx.Message
            };
        }
    }

    [Serializable]
    public struct VyResponsePagination
    {
        [JsonProperty("pageNumber")] public int PageNumber;
        [JsonProperty("pageSize")] public int PageSize;
        [JsonProperty("numberOfPages")] public int NumberOfPages;
        [JsonProperty("numberOfElements")] public int NumberOfElements;
        [JsonProperty("hasNextPage")] public bool HasNextPage;
        [JsonProperty("hasPreviousPage")] public bool HasPreviousPage;
    }
}