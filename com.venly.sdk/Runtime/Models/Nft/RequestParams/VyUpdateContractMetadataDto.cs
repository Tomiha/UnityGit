using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Nft
{
    [Serializable]
    public class VyUpdateContractMetadataDto
    {
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
    }
}