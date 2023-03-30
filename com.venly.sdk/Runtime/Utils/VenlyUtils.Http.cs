using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Shared;

namespace VenlySDK.Utils
{
    public static partial class VenlyUtils
    {
        //VyRequestData to HttpRequestMessage
        public static HttpRequestMessage ToRequestMessage(this VyRequestData data, VyAccessTokenDto token = null)
        {
            var requestMsg = new HttpRequestMessage(data.Method,GetUrl(data.Uri, data.Endpoint, data.Environment))
            {
                Content = data.GetHttpContent()
            };

            requestMsg.Headers.Add("Accept", "application/json");

            if (token != null)
            {
                requestMsg.Headers.Add("Authorization", $"Bearer {token.Token}");
            }

            return requestMsg;
        }

        public static VyTaskResult<T> ProcessApiResponse<T>(string response, HttpStatusCode statusCode, VyRequestData requestData)
        {
            #region Parse Error
            if (statusCode != HttpStatusCode.OK)
            {
                VyApiResponseDto apiResponse = null;

                //Venly Error Parsing...
                var errorJToken = JToken.Parse(response);
                if (errorJToken.Type == JTokenType.Array) //Array Errors
                {
                    apiResponse = new VyApiResponseDto()
                    {
                        Success = false,
                        Errors = errorJToken.ToObject<VyApiReponseError[]>() ?? Array.Empty<VyApiReponseError>()
                    };
                }
                else if(errorJToken.Type == JTokenType.Object)
                {
                    var errorJObject = errorJToken as JObject;
                    if (!errorJObject.ContainsKey("success"))
                    {
                        string errorCode;
                        string errorMessage;

                        if (errorJObject.ContainsKey("error"))//error + error_description
                        {
                            errorCode = errorJObject["error"]?.ToString();
                            errorMessage = errorJObject["error_description"]?.ToString();
                        }
                        else //errorCode + errorMessage
                        {
                            errorCode = errorJObject["errorCode"]?.ToString();
                            errorMessage = errorJObject["errorMessage"]?.ToString();
                        }

                        apiResponse = new VyApiResponseDto
                        {
                            Success = false,
                            Errors = new VyApiReponseError[]
                            {
                                new()
                                {
                                    Code = errorCode,
                                    Message = errorMessage
                                }
                            }
                        };
                    }
                    else //"normal" error
                    {
                        apiResponse = errorJObject.ToObject<VyApiResponseDto>();
                    }
                }

                if (apiResponse == null)
                    return new VyTaskResult<T>(
                        new Exception($"API Response Error, but failed to parse the message. (msg = {response})"));

                return new VyTaskResult<T>(VyException.ApiResponseError(apiResponse, statusCode, requestData));
            }
            #endregion

            #region Parse Response
            T data;
            if (!requestData.RequiresWrapping)
            {
                //NO WRAPPING REQUIRED
                if (requestData.MustSelectProperty)
                {
                    //SELECT PROPERTY
                    var json = JObject.Parse(response);
                    if (json.ContainsKey(requestData.SelectPropertyName))
                    {
                        data = JsonConvert.DeserializeObject<T>(json[requestData.SelectPropertyName].ToString());
                        return new VyTaskResult<T>(data);
                    }

                    //Key Not Found
                    var ex = VyException.ParsingError($"Property \'{requestData.SelectPropertyName}\' was not found in the response data");
                    return new VyTaskResult<T>(ex);
                }

                data = JsonConvert.DeserializeObject<T>(response);
                return new VyTaskResult<T>(data);
            }

            //WRAPPING REQUIRED
            if (requestData.MustSelectProperty)
            {
                //SELECT PROPERTY
                var wrappedResponseRaw = JsonConvert.DeserializeObject<VyApiResponseDto<JObject>>(response);
                if (wrappedResponseRaw is {Success: true})
                {
                    if (wrappedResponseRaw.Data.ContainsKey(requestData.SelectPropertyName))
                    {
                        data = JsonConvert.DeserializeObject<T>(wrappedResponseRaw.Data[requestData.MustSelectProperty].ToString());
                        return new VyTaskResult<T>(data);
                    }

                    //Key Not Found
                    var ex = VyException.ParsingError($"Property \'{requestData.SelectPropertyName}\' was not found in the response data");
                    return new VyTaskResult<T>(ex);
                }

                //Unsuccessful ApiResponse
                return new VyTaskResult<T>(VyException.ApiResponseError(wrappedResponseRaw, statusCode, requestData));
            }

            //NO PROPERTY SELECTION
            var wrappedResponse = JsonConvert.DeserializeObject<VyApiResponseDto<T>>(response);
            if (wrappedResponse is {Success: true})
            {
                return new VyTaskResult<T>(wrappedResponse.Data);
            }

            //Unsuccessful ApiResponse
            return new VyTaskResult<T>(VyException.ApiResponseError(wrappedResponse, statusCode, requestData));
            #endregion
        }

        public static VyTaskResult<T> ProcessHttpResponse<T>(HttpResponseMessage response, VyRequestData requestData)
        {
            //Response Parsing
            var responsePayload = string.Empty;
            try
            {
                responsePayload = response.Content.ReadAsStringAsync().Result;
                return ProcessApiResponse<T>(responsePayload, response.StatusCode, requestData);
            }
            catch (Exception e)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //Bad Response, and failed to retrieve errors
                    return new VyTaskResult<T>(VyException.HttpResponseError(response, requestData));
                }

                //OK Response, but failed to parse the payload
                return new VyTaskResult<T>(VyException.ParsingError($"Failed to parse the response.\nErrorMsg = {e.Message}\n\nResponseText = {responsePayload}))"));
            }
        }
    }
}