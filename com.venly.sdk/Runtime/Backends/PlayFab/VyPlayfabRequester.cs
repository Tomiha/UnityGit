using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.CloudScriptModels;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;

namespace VenlySDK.Backends.PlayFab
{
    public class VyPlayfabRequester : IVenlyRequester
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
            var serializedRequestData = requestData.Serialize();

            ExecuteFunctionRequest request = new ExecuteFunctionRequest
            {
                FunctionName = VenlySettings.PlayFabBackendSettings.VenlyAzureFunction,
                AuthenticationContext = _authContext,
                FunctionParameter = serializedRequestData
            };

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    VyTaskResult<T> taskResult;
                    if (result.Error == null)
                    {
                        try
                        {
                            var azureResponse = JsonConvert.DeserializeObject<VyPlayFabAzureResponse<T>>(result.FunctionResult.ToString());
                            if (azureResponse is {Success: true})
                            {
                                taskResult = new VyTaskResult<T>(azureResponse.Data);
                            }
                            else
                            {
                                taskResult =
                                    new VyTaskResult<T>(
                                        new VyException(azureResponse.ErrorMessage));
                            }
                        }
                        catch (Exception ex)
                        {
                            taskResult = new VyTaskResult<T>(new VyException(ex));
                        }
                    }
                    else
                    {
                        taskResult =
                            new VyTaskResult<T>(new VyException($"[Playfab Azure Error] {result.Error.Message}"));
                    }

                    //Make sure the promise wasn't rejected before
                    taskNotifier.Notify(taskResult);

                },
                (error) =>
                {
                    taskNotifier.NotifyFail(new VyException($"[Playfab Azure Error] {error.ErrorMessage}"));
                });


            return taskNotifier.Task;
        }
    }
}