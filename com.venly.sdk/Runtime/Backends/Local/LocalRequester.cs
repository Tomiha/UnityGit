using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;
using VenlySDK.Utils;

namespace VenlySDK.Backends.Local
{
    public class LocalRequester : IVenlyRequester
    {
        private VyAccessTokenDto _accessToken;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public LocalRequester(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private VyTask Authenticate()
        {
#if UNITY_EDITOR
            var taskNotifier = VyTask.Create("Authenticate");
            //var deferred = VyDeferredTask<bool>.Create();

            Venly.AuthAPI.GetAccessToken(_clientId, _clientSecret)
                .OnSucces(token =>
                {
                    _accessToken = token;
                    taskNotifier.NotifySuccess();
                })
                .OnFail(ex => taskNotifier.NotifyFail(ex));

            return taskNotifier.Task;
#else
            return  VyTask.Failed(new Exception("[Venly SDK] Local Requester can only be used inside the Unity Editor."), "Authenticate Forbidden");
#endif
        }

        private VyTask ValidateAccessToken(bool skip = false)
        {
            if (skip || _accessToken is {IsValid: true}) return VyTask.Succeeded("ValidateAccessToken");

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