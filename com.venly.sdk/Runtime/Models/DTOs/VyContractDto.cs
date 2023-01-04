using System;
using Newtonsoft.Json;

namespace VenlySDK.Models
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

    //WALLET-API Contract (api v3 scheme)
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