using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Utils;

namespace VenlySDK.Models.Internal
{
    public class VyRequestData
    {
        [JsonProperty("uri")] public string Uri { get; private set; }
        [JsonProperty("method")] public HttpMethod Method { get; private set; }
        [JsonProperty("contentString")] public string ContentString { get; private set; }
        [JsonIgnore] public HttpContent Content { get; private set; }
        [JsonProperty("endpoint")] public eVyApiEndpoint Endpoint { get; private set; }
        [JsonProperty("environment")] public eVyEnvironment Environment { get; private set; }
        [JsonProperty("requiresWrapping")] public bool RequiresWrapping { get; private set; }

        [JsonProperty("selectPropertyName")] public string SelectPropertyName { get; private set; }
        [JsonIgnore] public bool MustSelectProperty => !string.IsNullOrEmpty(SelectPropertyName);

        [JsonIgnore] public string CallingOrigin { get; set; }
        [JsonIgnore] public StackTrace StackTrace { get; set; }

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
        public VyRequestData AddContent(byte[] content)
        {
            if (content == null) return this;

            Content = new ByteArrayContent(content);
            return this;
        }

        public VyRequestData AddJsonContent<T>(T content)
        {
            Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            return this;
        }

        public VyRequestData AddFormContent<T>(T content)
        {
            if (content is Dictionary<string, string> contentDictionary)
            {
                Content = new FormUrlEncodedContent(contentDictionary);
            }
            else
            {
                var data = new Dictionary<string, string>();
                foreach (var property in JObject.FromObject(content))
                {
                    if (property.Value != null)
                        data.Add(property.Key, property.Value.ToString());
                }

                Content = new FormUrlEncodedContent(data);
            }

            return this;
        }

        #endregion

        #region Misc

        public VyRequestData SelectProperty(string propertyName)
        {
            SelectPropertyName = propertyName;
            return this;
        }

        public byte[] Serialize()
        {
            if (Content != null)
            {
                var bytes = Content.ReadAsByteArrayAsync().Result;
                ContentString = Encoding.UTF8.GetString(bytes);
            }

            var serializedThis = JsonConvert.SerializeObject(this);
            var result =  Encoding.UTF8.GetBytes(serializedThis);

            return result;
        }

        public static VyRequestData Deserialize(byte[] data)
        {
            var dataString = Encoding.UTF8.GetString(data);
            var newData = JsonConvert.DeserializeObject<VyRequestData>(dataString);

            if(!string.IsNullOrEmpty(newData.ContentString))
                newData.Content = new StringContent(newData.ContentString, Encoding.UTF8, "application/json");

            return newData;
        }

        public T JsonContentTo<T>()
        {
            var contentStr = Content.ReadAsStringAsync().Result;
            //var json = Encoding.UTF8.GetString(Content);
            return JsonConvert.DeserializeObject<T>(contentStr);
        }
        #endregion
    }
}