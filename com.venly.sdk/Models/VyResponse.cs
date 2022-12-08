using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public struct VyResponse<T>
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("result")] public T Data;
        [JsonProperty("pagination")] public VyResponsePagination Pagination;
        [JsonProperty("errors")] public VyReponseError[] Errors;

        public Exception ToException(string uri = "")
        {
            var sb = new StringBuilder();
            foreach (var err in Errors)
            {
                if (string.IsNullOrEmpty(uri))
                {
                    sb.AppendLine($"{{ERROR >> [code={err.Code}, traceCode={err.TraceCode}, msg={err.Message}]}}");
                }
                else
                {
                    sb.AppendLine($"{{ERROR(uri={uri}) >> [code={err.Code}, traceCode={err.TraceCode}, msg={err.Message}]}}");
                }
            }

            return new Exception(sb.ToString());
        }

        public static VyResponse<T> FromException(Exception ex)
        {
            return new()
            {
                Success = false,
                Data = default,
                Errors = new []{VyReponseError.FromException(ex)}
            };
        }
    }

    [Serializable]
    public struct VyReponseError
    {
        [JsonProperty("code")] public string Code;
        [JsonProperty("traceCode")] public string TraceCode;
        [JsonProperty("message")] public string Message;

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
