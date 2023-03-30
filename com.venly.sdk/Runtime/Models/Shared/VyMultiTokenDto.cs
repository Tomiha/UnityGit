using System;
using System.Linq;
using Newtonsoft.Json;

namespace VenlySDK.Models.Shared
{
    /// <summary>
    /// ERC721 or ERC1155 Token
    /// ERC1155: True MultiToken, can be Fungible or Non-Fungible
    /// ERC721: Can only be Non-Fungible (So a 'MultiToken' but limited to Non-Fungible only)
    /// </summary>
    [Serializable]
    public class VyMultiTokenDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
        [JsonProperty("imagePreviewUrl")] public string ImagePreviewUrl { get; set; }
        [JsonProperty("imageThumbnailUrl")] public string ImageThumbnailUrl { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("fungible")] public bool Fungible { get; set; }
        [JsonProperty("contract")] public VyWalletTokenContractDto Contract { get; set; }
        [JsonProperty("attributes")] public VyTokenAttributeDto[] Attributes { get; set; }
        [JsonProperty("transferFees")] public bool TransferFees { get; set; }

        //Inconsistent between Wallet & NFT api
        [JsonProperty("image")] private string _image { set => ImageUrl = value; }
        [JsonProperty("imagePreview")] private string _imagePreview { set => ImagePreviewUrl = value; }
        [JsonProperty("imageThumbnail")] private string _imageThumbnail { set => ImageThumbnailUrl = value; }

        public bool HasAttribute(string name)
        {
            if (Attributes == null) return false;

            return Attributes.Any(att => att.Name.Equals(name));
        }

        public VyTokenAttributeDto GetAttribute(string name)
        {
            return Attributes.FirstOrDefault(att => att.Name.Equals(name));
        }

        public T GetAttributeValue<T>(string name, T defaultValue = default)
        {
            var att = Attributes.FirstOrDefault(att => att.Name.Equals(name));
            if (att == null) return defaultValue;

            return att.As<T>();
        }
    }
}