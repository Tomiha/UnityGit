using System;
using System.Linq;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Nft
{
    [Serializable]
    public class VyTokenTypeDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string Image { get; set; }
        [JsonProperty("imageThumbnail")] public string ImageThumbnail { get; set; }
        [JsonProperty("imagePreview")] public string ImagePreview { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("youtubeUrl")] public string YoutubeUrl { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("fungible")] public bool Fungible { get; set; }
        [JsonProperty("burnable")] public bool Burnable { get; set; }
        [JsonProperty("maxSupply")] public long MaxSupply { get; set; }
        [JsonProperty("currentSupply")] public long CurrentSupply { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("attributes")] public VyTokenAttributeDto[] Attributes { get; set; }
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
        [JsonProperty("storage")] public VyStorage Storage { get; set; }
        [JsonProperty("confirmed")] public bool Confirmed { get; set; }

        public bool HasAttribute(string name)
        {
            return Attributes.Any(att => att.Name.Equals(name));
        }

        public bool HasAttribute<T>(string name, T value)
        {
            var att = Attributes.FirstOrDefault(att => att.Name.Equals(name));
            if (att == null) return false;

            return att.As<T>().Equals(value);
        }

        public VyTokenAttributeDto GetAttribute(string name)
        {
            return Attributes.FirstOrDefault(att => att.Name.Equals(name));
        }
    }

    [Serializable]
    public class VyTokenTypeMetadataDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string Image { get; set; }
        [JsonProperty("imageThumbnail")] public string ImageThumbnail { get; set; }
        [JsonProperty("imagePreview")] public string ImagePreview { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("attributes")] public VyTokenAttributeDto[] Attributes { get; set; }
    }
}