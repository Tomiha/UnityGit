using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Nft
{
    //todo: check required/optional params (autoapproved...,storage)
    [Serializable]
    public class VyCreateContractDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
        [JsonProperty("owner")] public string Owner { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }

        //[JsonProperty("autoApprovedAddressesLocked")] private bool _autoApprovedAddressesLocked = true;
        //[JsonProperty("storage")] public VyStorage Storage { get; set; }
    }
}