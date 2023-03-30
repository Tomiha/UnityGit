using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Nft
{
    //NFT-API Contract (api v3 scheme)
    [Serializable]
    public class VyContractDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("confirmed")] public bool Confirmed { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("image")] public string Image { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
        [JsonProperty("owner")] public string Owner { get; set; }
        [JsonProperty("storage")] public VyStorage Storage { get; set; }
        [JsonProperty("external_link")] public string ExternalLink { get; set; }
    }

    [Serializable]
    public class VyContractMetadataDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
    }
}