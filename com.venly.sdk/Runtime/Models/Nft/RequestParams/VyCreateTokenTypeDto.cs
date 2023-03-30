using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Nft
{
    //todo: check storage
    [Serializable]
    public class VyCreateTokenTypeDto
    {
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("fungible")] public bool Fungible { get; set; }
        [JsonProperty("burnable")] public bool Burnable { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("maxSupply")] public int MaxSupply { get; set; }
        [JsonProperty("attributes")] public VyTokenAttributeDto[] Attributes { get; set; }

        [JsonProperty("destinations")] public VyTokenDestination[] Destinations { get; set; }
        //[JsonProperty("storage")] public VyStorage Storage { get; set; }
    }
}