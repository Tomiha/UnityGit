using System;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.CloudScriptModels;
using Proto.Promises;
using Venly.Models;
using Venly.Utils;

namespace Venly.Backends.PlayFab
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
        protected override Promise<T> MakeRequest<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, HttpContent content, bool isWrapped)
        {
            if (!HasAuthContext)
            {
                return Promise<T>.Rejected(new Exception("[Playfab requester] AuthContext not set."));
            }

            var deferredPromise = Promise<T>.NewDeferred();

            var wrappedPayload = VyWrappedPayload.Create(method, uri, endpoint, content, isWrapped);

            ExecuteFunctionRequest request = new ExecuteFunctionRequest();
            request.FunctionName = VenlySettings.PlayFabBackendSettings.VenlyAzureFunction;
            request.AuthenticationContext = _authContext;
            request.FunctionParameter = wrappedPayload;

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    T payload = default;
                    if (typeof(T) == typeof(string))
                    {
                        payload = (T) result.FunctionResult;
                    }
                    else
                    {
                        try
                        {
                            payload = JsonConvert.DeserializeObject<T>(result.FunctionResult.ToString());
                        }
                        catch (Exception e)
                        {
                            throw VenlyUtils.WrapException($"Failed to parse the response.\nErrorMsg = {e.Message}\n\nResponseText = {result.FunctionResult}))");
                        }
                    }

                    //Make sure the promise wasn't rejected before
                    if (deferredPromise.IsValidAndPending)
                        deferredPromise.Resolve(payload);

                },
                (error) =>
                {
                    deferredPromise.Reject(new Exception($"Failed! \nPlayfab Error: {error.ErrorMessage}"));
                });


            return deferredPromise.Promise;
        }
    }
}