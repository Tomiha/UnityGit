using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Proto.Promises;
using Venly.Models;
using Venly.Utils;

namespace Venly.Backends
{
    public abstract class IVenlyRequester
    {
        protected abstract Promise<T> MakeRequest<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint,
            HttpContent content, bool isWrapped);

        #region Request Wrapping
        protected virtual Promise<T> MakeRequest_Wrap<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, HttpContent content, bool wrap)
        {
            //Wrap inside a Default Response
            if (wrap)
            {
                var result = Promise<T>.NewDeferred();

                MakeRequest<VyResponse<T>>(method, uri, endpoint, content, true)
                    .Then(response =>
                    {
                        if (response.Success) result.Resolve(response.Data);
                        else result.Reject(response.ToException(uri));
                    })
                    .Catch((Exception ex) => result.Reject(ex))
                    .Forget();

                return result.Promise;
            }

            return MakeRequest<T>(method, uri, endpoint, content, false);
        }
        #endregion

        #region Request Interfaces

        public Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, bool wrap = true)
        {
            return MakeRequest_Wrap<T>(method, uri, endpoint, null!, wrap);
        }

        public Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, Dictionary<string, string> formData, bool wrap = true)
        {
            FormUrlEncodedContent formContent = new(formData);
            return MakeRequest_Wrap<T>(method, uri, endpoint, formContent, wrap);
        }

        public Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, Dictionary<string, object> jsonData, bool wrap = true)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(jsonData), Encoding.UTF8, "application/json");
            return MakeRequest_Wrap<T>(method, uri, endpoint, jsonContent, wrap);
        }

        public Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, string jsonData, bool wrap = true)
        {
            var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            return MakeRequest_Wrap<T>(method, uri, endpoint, jsonContent, wrap);
        }

        public Promise<T> Request_FORM<T, TBody>(HttpMethod method, string uri, eVyApiEndpoint endpoint, TBody body, bool wrap = true)
        {
            FormUrlEncodedContent formContent;
            if (body is Dictionary<string, string>)
            {
                formContent = new FormUrlEncodedContent(body as Dictionary<string,string>);
            }
            else
            {
                formContent = VenlyUtils.ConvertToFormContent(body);
            }

            return MakeRequest_Wrap<T>(method, uri, endpoint, formContent, wrap);
        }

        public Promise<T> Request_JSON<T, TBody>(HttpMethod method, string uri, eVyApiEndpoint endpoint, TBody body, bool wrap = true)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            return MakeRequest_Wrap<T>(method, uri, endpoint, jsonContent, wrap);
        }
        #endregion

        #region Requester Data
        protected Dictionary<string, object> _requesterData = new();
        protected virtual object GetData(string key)
        {
            if (_requesterData.ContainsKey(key)) return _requesterData[key];
            return null;
        }

        protected bool HasData(string key)
        {
            return _requesterData.ContainsKey(key);
        }

        public virtual bool RemoveData(string key)
        {
            if (_requesterData.ContainsKey(key))
            {
                _requesterData.Remove(key);
                return true;
            }

            return false;
        }

        public virtual void SetData(string key, object data)
        {
            _requesterData.Add(key, data);
        }
        #endregion
    }
}