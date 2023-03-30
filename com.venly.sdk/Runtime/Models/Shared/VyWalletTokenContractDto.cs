using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Shared
{
    [Serializable]
    public class VyWalletTokenContractDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("verified")] public bool Verified { get; set; }
        [JsonProperty("premium")] public bool Premium { get; set; }
        [JsonProperty("categories")] public string[] Categories { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
    }
}