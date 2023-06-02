using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Venly;
using Venly.Backends;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Utils;

namespace Venly.Backends
{
    public class VyProvider_DevMode : VyProviderBase
    {
        private VyAccessTokenDto _accessToken;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private VyJwtInfo _jwtInfo;

        public VyProvider_DevMode() : base(eVyBackendProvider.DevMode.GetMemberName())
        {
#if UNITY_EDITOR
            _clientId = VenlySettings.ClientId;
            _clientSecret = VenlySettings.ClientSecret;
#endif
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        private static void RegisterProvider()
        {
            VenlyAPI.RegisterProvider(new VyProvider_DevMode());
        }
#endif

        private VyTask Authenticate()
        {
#if UNITY_EDITOR
            var taskNotifier = VyTask.Create("Authenticate");
            //var deferred = VyDeferredTask<bool>.Create();

            VenlyAPI.Auth.GetAccessToken(_clientId, _clientSecret)
                .OnSuccess(token =>
                {
                    _accessToken = token;
                    _jwtInfo = _accessToken.JwtInfo;

                    taskNotifier.NotifySuccess();
                })
                .OnFail(ex => taskNotifier.NotifyFail(ex));

            return taskNotifier.Task;
#else
            return VyTask.Failed(new Exception("[Venly SDK] DevMode Requester can only be used inside the Unity Editor."), "Authenticate Forbidden");
#endif
        }

        private VyTask ValidateAccessToken(bool skip = false)
        {
            if (skip || _accessToken is { IsValid: true }) return VyTask.Succeeded("ValidateAccessToken");

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
