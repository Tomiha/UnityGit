using Newtonsoft.Json;
using Proto.Promises;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Venly.Models;
using Venly.Utils;

namespace Venly.Backends.Local
{
    public class LocalRequester : IVenlyRequester
    {
        private VyAccessToken _accessToken;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public LocalRequester(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private Promise Authenticate()
        {
            var deferred = Promise.NewDeferred();

            VenlyAPI.Server.Authentication.GetAccessToken(_clientId, _clientSecret)
                .Then(token =>
                {
                    _accessToken = token;
                    deferred.Resolve();
                })
                .CatchAndForget(deferred);

            return deferred.Promise;
        }

        private Promise ValidateAccessToken(bool skip = false)
        {
            if (skip) return Promise.Resolved();
            if (_accessToken is {IsValid: true}) return Promise.Resolved();

            var result = Promise.NewDeferred();

            Authenticate()
                .Then(() =>
                {
                    result.Resolve();
                })
                .Catch((Exception ex) => result.Reject(ex))
                .Forget();

            return result.Promise;
        }

        //Make Request
        protected override Promise<T> MakeRequest<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, HttpContent content, bool isWrapped)
        {
            var result = Promise<T>.NewDeferred();

            //Check if we need to start another Thread
            if (Thread.CurrentThread.IsBackground) HttpRequestAction();
            else Task.Run(HttpRequestAction);

            //The Action (todo: put in utils)
            void HttpRequestAction()
            {
                //Check if Authorization is required
                bool requiresAuthorization = endpoint != eVyApiEndpoint.Auth;

                //Validate Token if required
                ValidateAccessToken(!requiresAuthorization)
                    .Then(async () =>
                    {
                        using HttpClient client = new();

                        //Build Request
                        //-------------
                        var request = new HttpRequestMessage(method, VenlyUtils.GetUrl(uri, endpoint)) {Content = content };

                        request.Headers.Add("Accept", "application/json");


                        if (requiresAuthorization)
                        {
                            request.Headers.Add("Authorization", $"Bearer {_accessToken.Token}");
                        }

                        //Send Request
                        //============
                        var response = await client.SendAsync(request);

                        //todo: check internal server error handling (if error message
                        //if (response.StatusCode < HttpStatusCode.InternalServerError)
                        {
                            var responsePayload = await response.Content.ReadAsStringAsync();
                            T payload = default;
                            if (typeof(T) == typeof(string)) //todo: remove string specialization (check first)
                            {
                                payload = (T) (responsePayload as object);
                                result.Resolve(payload);
                            }
                            else
                            {
                                try
                                {
                                    payload = JsonConvert.DeserializeObject<T>(responsePayload);
                                    result.Resolve(payload);
                                }
                                catch (Exception e)
                                {
                                    throw VenlyUtils.WrapException($"Failed to parse the response.\nErrorMsg = {e.Message}\n\nResponseText = {responsePayload}))");
                                }
                            }
                        }
                        //else
                        //{
                        //    var errorMessage = await response.Content.ReadAsStringAsync();
                        //    result.Reject(new Exception($"[Local-Requester] Failed to sent the request. ({response.ReasonPhrase})"));
                        //}
                    })
                    .CatchAndForget<T>(result);
            }

            return result.Promise;
        }
    }
}