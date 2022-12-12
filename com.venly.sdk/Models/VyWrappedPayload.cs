using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Venly;
using Venly.Models;

[Serializable]
public class VyWrappedPayload
{
    [JsonProperty("uri")] public string Uri { get; private set; }
    [JsonProperty("endpoint")] public eVyApiEndpoint Endpoint { get; private set; }
    [JsonProperty("environment")] public eVyEnvironment Environment { get; private set; }
    [JsonProperty("body")] public byte[] Body { get; private set; }
    [JsonProperty("contentType")] public string ContentType { get; private set; }
    [JsonProperty("requiresWrap")] public bool RequiresWrap { get; private set; }
    [JsonProperty("method")] public HttpMethod Method { get; private set; }

    public byte[] ToBytes()
    {
        var json = JsonConvert.SerializeObject(this);
        return Encoding.UTF8.GetBytes(json);
    }

    public T BodyAs<T>()
    {
        var json = Encoding.UTF8.GetString(Body);
        return JsonConvert.DeserializeObject<T>(json);
    }

#if !ENABLE_VENLY_AZURE
    public static VyWrappedPayload Create(HttpMethod method, string uri, eVyApiEndpoint endpoint, HttpContent content, bool requiresWrap)
    {
        byte[] body = null;
        string contentType = string.Empty;

        if (content != null)
        {
            contentType = content.Headers.ContentType.MediaType;
            body = content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        }

        return new VyWrappedPayload
        {
            Uri = uri,
            Endpoint = endpoint,
            ContentType = contentType,
            Body = body,
            Environment = VenlySettings.Environment,
            RequiresWrap = requiresWrap,
            Method = method
        };
    }
#endif
}
