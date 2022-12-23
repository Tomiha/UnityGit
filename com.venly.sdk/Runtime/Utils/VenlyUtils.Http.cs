using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;

namespace VenlySDK.Utils
{
    public static partial class VenlyUtils
    {
        //VyRequestData to HttpRequestMessage
        public static HttpRequestMessage ToRequestMessage(this VyRequestData data, VyAccessTokenDto token = null)
        {
            var requestMsg = new HttpRequestMessage(data.Method,GetUrl(data.Uri, data.Endpoint, data.Environment))
            {
                Content = data.Content
            };

            requestMsg.Headers.Add("Accept", "application/json");

            if (token != null)
            {
                requestMsg.Headers.Add("Authorization", $"Bearer {token.Token}");
            }

            return requestMsg;
        }

        public static VyTaskResult<T> ProcessHttpResponse<T>(HttpResponseMessage response, VyRequestData requestData)
        {
            var result = new VyTaskResult<T>()
            {
                Success = false
            };

            //Error Handling
            string responsePayload = string.Empty;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                try
                {
                    responsePayload = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception)
                {
                    // ignored
                }

                if (string.IsNullOrEmpty(responsePayload))
                {
                    result.Exception = VyException.HttpResponseError(response, requestData);
                    return result;
                }

                //Temp hack
                VyResponseDto apiResponse = null;
                if (responsePayload.StartsWith("["))
                {
                    apiResponse = new VyResponseDto()
                    {
                        Success = false,
                        Errors = JArray.Parse(responsePayload).ToObject<VyReponseError[]>()
                    };
                }
                else
                {
                    apiResponse = JsonConvert.DeserializeObject<VyResponseDto>(responsePayload);
                }

                result.Exception = VyException.ApiResponseError(apiResponse, response, requestData);
                return result;
            }

            //Response Parsing
            try
            {
                responsePayload = response.Content.ReadAsStringAsync().Result;

                if (!requestData.RequiresWrapping)
                {
                    //NO WRAPPING REQUIRED
                    if (requestData.MustSelectProperty)
                    {
                        //SELECT PROPERTY
                        var json = JObject.Parse(responsePayload);
                        if (json.ContainsKey(requestData.SelectPropertyName))
                        {
                            result.Data =
                                JsonConvert.DeserializeObject<T>(json[requestData.SelectPropertyName].ToString());
                            result.Success = true;
                        }
                        else
                        {
                            result.Exception = VyException.ParsingError(
                                $"Property \'{requestData.SelectPropertyName}\' was not found in the response data");
                        }
                    }
                    else
                    {
                        //NO PROPERTY SELECTION
                        result.Data = JsonConvert.DeserializeObject<T>(responsePayload);
                        result.Success = true;
                    }
                }
                else
                {
                    //WRAPPING REQUIRED
                    if (requestData.MustSelectProperty)
                    {
                        //SELECT PROPERTY
                        var apiResponse = JsonConvert.DeserializeObject<VyResponseDto<JObject>>(responsePayload);
                        if (apiResponse.Success)
                        {
                            if (apiResponse.Data.ContainsKey(requestData.SelectPropertyName))
                            {
                                result.Data =
                                    JsonConvert.DeserializeObject<T>(apiResponse.Data[requestData.MustSelectProperty]
                                        .ToString());
                                result.Success = true;
                            }
                            else
                            {
                                result.Exception = VyException.ParsingError(
                                    $"Property \'{requestData.SelectPropertyName}\' was not found in the response data");
                            }
                        }
                        else
                        {
                            result.Exception = VyException.ApiResponseError(apiResponse, response, requestData);
                        }
                    }
                    else
                    {
                        //NO PROPERTY SELECTION
                        var apiResponse = JsonConvert.DeserializeObject<VyResponseDto<T>>(responsePayload);
                        if (apiResponse.Success)
                        {
                            result.Data = apiResponse.Data;
                            result.Success = true;
                        }
                        else
                        {
                            result.Exception = VyException.ApiResponseError(apiResponse, response, requestData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.Exception = VyException.ParsingError(
                    $"Failed to parse the response.\nErrorMsg = {e.Message}\n\nResponseText = {responsePayload}))");
            }

            return result;
        }

        //public static VyTaskResult<T> ProcessResponseString<T>(string response, VyRequestData requestData)
        //{

        //}
    }
}