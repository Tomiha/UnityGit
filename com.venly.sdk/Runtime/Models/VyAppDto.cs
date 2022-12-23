using System;
using Newtonsoft.Json;

namespace VenlySDK.Models
{
    [Serializable]
    public class VyAppDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("clientId")] public string ClientId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("rootURL")] public string RootUrl { get; set; }
        [JsonProperty("redirectURIs")] public string[] RedirectUris { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
    }
}