using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using VenlySDK.Models;
using VenlySDK.Models.Internal;

namespace VenlySDK.Backends.PlayFab
{
    internal class VyPlayFabRequestData
    {
        [JsonProperty("uri")] public string Uri { get; private set; }
        [JsonProperty("http_method")] public string Method { get; private set; }
        [JsonProperty("content")] public byte[] Content { get; private set; }
        [JsonProperty("content_type")] public string ContentType { get; private set; }
        [JsonProperty("endpoint")] public eVyApiEndpoint Endpoint { get; private set; }
        [JsonProperty("environment")] public eVyEnvironment Environment { get; private set; }
        [JsonProperty("requires_wrap")] public bool RequiresWrap { get; private set; }

        public VyPlayFabRequestData(){}
        public VyPlayFabRequestData(VyRequestData requestData)
        {
            Uri = requestData.Uri;
            Method = requestData.Method.ToString();
            Content = requestData.Content?.ReadAsByteArrayAsync().Result??null;
            ContentType = requestData.Content?.Headers.ContentType.MediaType??string.Empty;
            Endpoint = requestData.Endpoint;
            Environment = requestData.Environment;
            RequiresWrap = requestData.RequiresWrapping;
        }

#if ENABLE_VENLY_AZURE
        public VyRequestData ToRequestData()
        {
            return VyRequestData.Create(new HttpMethod(Method), Uri, Endpoint, Environment)
                .AddContent(Content);
        }
#endif

        public byte[] ToBytes()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public T JsonContentTo<T>()
        {
            var json = Encoding.UTF8.GetString(Content);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
