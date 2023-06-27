#if VENLY_ENABLE_GAMESPARKS
using System;
using System.Net;
using Amazon.GameSparks.Unity.DotNet;
using Amazon.GameSparks.Unity.EngineIntegration;
using Newtonsoft.Json;
using UnityEngine;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Utils;

namespace Venly.Backends.GameSparks
{
    public class VyProvider_GameSparks : VyProviderBase
    {
        //CloudScript Vars
        //private PlayFabAuthenticationContext _authContext = null;
        //public const string AuthContextDataKey = "PlayFab-AuthContext";
        //public bool HasAuthContext => _authContext != null;

        public VyProvider_GameSparks() : base(eVyBackendProvider.GameSparks.GetMemberName())
        {
            ExtensionsSupported = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            VenlyAPI.RegisterProvider(new VyProvider_GameSparks());
        }

        //protected override void OnSetData(string key, object data)
        //{
        //    if (key.Equals(AuthContextDataKey))
        //    {
        //        _authContext = data as PlayFabAuthenticationContext;
        //    }
        //}

        public override VyTask<T> MakeRequest<T>(VyRequestData requestData)
        {
            

            return VyTask<T>.Failed(new NotImplementedException("GameSparks provider not yet implemented!"));

            //if (!HasAuthContext)
            //{
            //    return VyTask<T>.Failed(new Exception("[Playfab Provider] AuthContext not set."));
            //}

            //if (string.IsNullOrEmpty(VenlySettings.PlayFabBackendSettings.VenlyAzureFunction))
            //{
            //    return VyTask<T>.Failed(new Exception("[Playfab Provider] Venly Azure Function cannot be NULL or empty. (SDK Settings)"));
            //}

            //var taskNotifier = VyTask<T>.Create();

            //ExecuteFunctionRequest request = new ExecuteFunctionRequest
            //{
            //    FunctionName = VenlySettings.PlayFabBackendSettings.VenlyAzureFunction,
            //    AuthenticationContext = _authContext,
            //    FunctionParameter = requestData
            //};

            //PlayFabCloudScriptAPI.ExecuteFunction(request,
            //    (result) =>
            //    {
            //        if (result.Error == null)
            //        {
            //            try
            //            {
            //                var serverResponse = JsonConvert.DeserializeObject<VyServerResponseDto>(result.FunctionResult.ToString());
            //                VyTaskResult<T> taskResult;

            //                if (serverResponse is { Success: true })
            //                {
            //                    if (serverResponse.IsRawResponse) //Requires Response Parsing
            //                    {
            //                        //Small trick to safely unescape the JSON encoded JSON string...
            //                        object decodedJsonString = JsonConvert.DeserializeObject(serverResponse.Data);
            //                        taskResult = VenlyUtils.ProcessApiResponse<T>(decodedJsonString?.ToString(), (HttpStatusCode)serverResponse.StatusCode, requestData);
            //                        taskNotifier.Notify(taskResult);
            //                        return;
            //                    }

            //                    //Response contains the requested object, no parsing required
            //                    T data = JsonConvert.DeserializeObject<T>(serverResponse.Data);
            //                    taskResult = new VyTaskResult<T>(data);
            //                    taskNotifier.Notify(taskResult);
            //                    return;
            //                }

            //                taskResult = new VyTaskResult<T>(new VyException(serverResponse.ErrorMessage));
            //                taskNotifier.Notify(taskResult);
            //                return;
            //            }
            //            catch (Exception ex)
            //            {
            //                taskNotifier.NotifyFail(new VyException(ex));
            //                return;
            //            }
            //        }

            //        //PlayFay Result contains Error
            //        taskNotifier.NotifyFail(new VyException($"[Playfab Azure Error] {result.Error.Message}"));
            //    },
            //    (error) =>
            //    {
            //        //PlayFab Error
            //        taskNotifier.NotifyFail(new VyException($"[Playfab Azure Error] {error.ErrorMessage}"));
            //    });


            //return taskNotifier.Task;
        }
    }
}
#endif