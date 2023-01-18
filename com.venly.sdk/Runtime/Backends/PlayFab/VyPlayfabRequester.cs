using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.CloudScriptModels;
using VenlySDK.Core;
using VenlySDK.Utils;

namespace VenlySDK.Backends.PlayFab
{
    public class VyPlayfabRequester : VyRequester
    {
        //CloudScript Vars
        private PlayFabAuthenticationContext _authContext = null;
        public const string AuthContextDataKey = "PlayFab-AuthContext";

        public bool HasAuthContext => _authContext != null;

        public override void SetData(string key, object data)
        {
            if (key.Equals(AuthContextDataKey))
            {
                _authContext = (PlayFabAuthenticationContext) data;
            }
        }

        //Make Request Override >> PlayFab Integration
        public override VyTask<T> MakeRequest<T>(VyRequestData requestData)
        {
            if (!HasAuthContext)
            {
                return VyTask<T>.Failed(new Exception("[Playfab requester] AuthContext not set."));
            }

            var taskNotifier = VyTask<T>.Create();
            //var serializedRequestData = requestData.Serialize();

            ExecuteFunctionRequest request = new ExecuteFunctionRequest
            {
                FunctionName = VenlySettings.PlayFabBackendSettings.VenlyAzureFunction,
                AuthenticationContext = _authContext,
                FunctionParameter = requestData
            };

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    if (result.Error == null)
                    {
                        try
                        {
                            var azureResponse = JsonConvert.DeserializeObject<VyPlayFabAzureResponse>(result.FunctionResult.ToString());
                            VyTaskResult<T> taskResult;

                            if (azureResponse is {Success: true})
                            {
                                if (azureResponse.IsRawResponse)
                                {
                                    //Small trick to safely unescape the JSON encoded JSON string...
                                    object decodedJsonString = JsonConvert.DeserializeObject(azureResponse.Data);
                                    taskResult = VenlyUtils.ProcessApiResponse<T>(decodedJsonString.ToString(), azureResponse.StatusCode, requestData);
                                    taskNotifier.Notify(taskResult);
                                    return;
                                }

                                T data = JsonConvert.DeserializeObject<T>(azureResponse.Data);
                                taskResult = new VyTaskResult<T>(data);
                                taskNotifier.Notify(taskResult);
                                return;
                            }
                            
                            taskResult = new VyTaskResult<T>(new VyException(azureResponse.ErrorMessage));
                            taskNotifier.Notify(taskResult);
                            return;
                        }
                        catch (Exception ex)
                        {
                            taskNotifier.NotifyFail(new VyException(ex));
                            return;
                        }
                    }
                    
                    //PlayFay Result contains Error
                    taskNotifier.NotifyFail(new VyException($"[Playfab Azure Error] {result.Error.Message}"));
                },
                (error) =>
                {
                    //PlayFab Error
                    taskNotifier.NotifyFail(new VyException($"[Playfab Azure Error] {error.ErrorMessage}"));
                });


            return taskNotifier.Task;
        }
    }
}