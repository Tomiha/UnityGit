using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Nft
{
    [Serializable]
    public class VyUpdateTokenTypeMetadataDto
    {
        [JsonIgnore] public int ContractId { get; set; }
        [JsonIgnore] public int TokenTypeId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("attributes")] public VyTokenAttributeDto[] Atrributes { get; set; }
    }
}