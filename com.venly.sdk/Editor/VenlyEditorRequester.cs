using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Proto.Promises;
using VenlySDK.Backends;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;
using VenlySDK.Utils;

namespace VenlySDK.Editor
{
    internal class VenlyEditorRequester : IVenlyRequester
    {
        private VyAccessTokenDto _accessToken;

        private VyTask<bool> Authenticate()
        {

            var taskNotifier = VyTask<bool>.Create("Authenticate");
  
            VenlyEditorAPI.GetAccessToken(VenlySettings.ClientId, VenlySettings.ClientSecret)
                .OnSucces(token =>
                {
                    _accessToken = token;
                    taskNotifier.NotifySuccess(true);
                })
                .OnFail(ex => taskNotifier.NotifyFail(ex));

            return taskNotifier.Task;
        }

        private VyTask<bool> ValidateAccessToken(bool skip = false)
        {
            if (skip || _accessToken is { IsValid: true }) return VyTask<bool>.Succeeded(true, "ValidateAccessToken");

            return Authenticate();
        }
        
        public override VyTask<T> MakeRequest<T>(VyRequestData requestData)
        {
            //Create Task
            var taskNotifier = VyTask<T>.Create("MakeRequest");

            //Check if we need to start another Thread
            if (Thread.CurrentThread.IsBackground) HttpRequestAction();
            else Task.Run(HttpRequestAction);

            async void HttpRequestAction()
            {
                try
                {
                    //Check if Authorization is required  
                    bool requiresAuthorization = requestData.Endpoint != eVyApiEndpoint.Auth;

                    //Validate Token
                    //--------------
                    await ValidateAccessToken(!requiresAuthorization);

                    //Execute Request
                    //-------------
                    var requestMessage = requestData.ToRequestMessage(requiresAuthorization ? _accessToken : null);

                    using HttpClient client = new();
                    var response = await client.SendAsync(requestMessage);
                    var result = VenlyUtils.ProcessHttpResponse<T>(response, requestData);

                    //Little Hack
                    //var resultJson = JsonConvert.SerializeObject(result);
                    taskNotifier.Notify(result);
                }
                catch (Exception ex)
                {
                    taskNotifier.NotifyFail(ex);
                }
            }

            return taskNotifier.Task;
        }

    }
}
