using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using VenlySDK.Models;

namespace VenlySDK.Core
{
    public class VyRequestData
    {
        public enum eVyRequestContentType
        {
            None,
            Json,
            UrlEncoded,
            Binary,
            String
        }

        [JsonProperty("uri")] public string Uri { get; private set; }
        [JsonProperty("method")] public HttpMethod Method { get; private set; }
        [JsonProperty("endpoint")] public eVyApiEndpoint Endpoint { get; private set; }
        [JsonProperty("environment")] public eVyEnvironment Environment { get; private set; }
        [JsonProperty("requiresWrapping")] public bool RequiresWrapping { get; private set; }

        [JsonProperty("contentType")] public eVyRequestContentType ContentType { get; private set; } = eVyRequestContentType.None;
        [JsonProperty("contentStr")] public string ContentStr { get; private set; }

        [JsonProperty("selectPropertyName")] public string SelectPropertyName { get; private set; }
        [JsonIgnore] public bool MustSelectProperty => !string.IsNullOrEmpty(SelectPropertyName);

        [JsonIgnore] public bool MustProcessResponse { get; set; } = true;
        //[JsonIgnore] public string CallingOrigin { get; set; }
        //[JsonIgnore] public StackTrace StackTrace { get; set; }

        private VyRequestData() {}

        #region Creation

        internal static bool CheckWrapRequirement(eVyApiEndpoint endpoint)
        {
            switch (endpoint)
            {
                case eVyApiEndpoint.Nft: return false;
                case eVyApiEndpoint.Auth: return false;
                case eVyApiEndpoint.Extension: return false;
                default: return true;
            }
        }

        internal static VyRequestData Create(HttpMethod method, string uri, eVyApiEndpoint endpoint, eVyEnvironment environment)
        {
            return new VyRequestData
            {
                Method = method,
                Uri = uri,
                Endpoint = endpoint,
                Environment = environment,
                RequiresWrapping = CheckWrapRequirement(endpoint)
            };
        }

        public static VyRequestData Get(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Get, uri, endpoint, Venly.CurrentEnvironement);
        }

        public static VyRequestData Patch(string uri, eVyApiEndpoint endpoint)
        {
            return Create(new HttpMethod("PATCH"), uri, endpoint, Venly.CurrentEnvironement);
        }

        public static VyRequestData Post(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Post, uri, endpoint, Venly.CurrentEnvironement);
        }

        public static VyRequestData Put(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Put, uri, endpoint, Venly.CurrentEnvironement);
        }

        public static VyRequestData Delete(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Delete, uri, endpoint, Venly.CurrentEnvironement);
        }

        #endregion

        #region Content
        public VyRequestData AddBinaryContent(byte[] content)
        {
            if (content == null) return this;

            ContentType = eVyRequestContentType.Binary;
            ContentStr = Encoding.UTF8.GetString(content);
            return this;

            //Content = new ByteArrayContent(content);
            //return this;
        }

        public VyRequestData AddJsonContent<T>(T content)
        {
            ContentType = eVyRequestContentType.Json;
            ContentStr = JsonConvert.SerializeObject(content);
            return this;

            //Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            //return this;
        }

        public VyRequestData AddFormContent<T>(T content)
        {
            ContentType = eVyRequestContentType.UrlEncoded;
            ContentStr = JsonConvert.SerializeObject(content);
            return this;


            //if (content is Dictionary<string, string> contentDictionary)
            //{
            //    Content = new FormUrlEncodedContent(contentDictionary);
            //}
            //else
            //{
            //    var data = new Dictionary<string, string>();
            //    foreach (var property in JObject.FromObject(content))
            //    {
            //        if (property.Value != null)
            //            data.Add(property.Key, property.Value.ToString());
            //    }

            //    Content = new FormUrlEncodedContent(data);
            //}

            //return this;
        }

        public T GetJsonContent<T>()
        {
            if (ContentType != eVyRequestContentType.Json)
                throw new Exception($"VyRequestData::GetJsonContent > ContentType is not JSON (type={ContentType})");

            return JsonConvert.DeserializeObject<T>(ContentStr);
        }

        public Dictionary<string, string> GetFormContent()
        {
            if (ContentType != eVyRequestContentType.UrlEncoded)
                throw new Exception($"VyRequestData::GetJsonContent > ContentType is not FormUrlEncoded (type={ContentType})");

            return JsonConvert.DeserializeObject<Dictionary<string,string>>(ContentStr);
        }

        public byte[] GetBinaryContent()
        {
            if (ContentType != eVyRequestContentType.UrlEncoded)
                throw new Exception($"VyRequestData::GetJsonContent > ContentType is not FormUrlEncoded (type={ContentType})");

            return Encoding.UTF8.GetBytes(ContentStr);
        }

        public string GetContent()
        {
            return ContentStr;
        }

        public HttpContent GetHttpContent()
        {
            switch (ContentType)
            {
                case eVyRequestContentType.Json: 
                    return new StringContent(ContentStr, Encoding.UTF8, "application/json");
                case eVyRequestContentType.UrlEncoded:
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(ContentStr);
                    return new FormUrlEncodedContent(dict);
                case eVyRequestContentType.Binary:
                    var bytes = Encoding.UTF8.GetBytes(ContentStr);
                    return new ByteArrayContent(bytes);
                case eVyRequestContentType.String:
                    return new StringContent(ContentStr, Encoding.UTF8);
            }

            return null;
        }

        #endregion

        #region Misc
        public VyRequestData SelectProperty(string propertyName)
        {
            SelectPropertyName = propertyName;
            return this;
        }
        #endregion
    }
}