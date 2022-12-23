using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VenlySDK.Models
{
    //NFT-API NFT-TOKEN (API v3 scheme)
    [Serializable]
    public class VyTokenTypeDto
    {
        [JsonProperty("id")] public long Id { get; set; }
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
        [JsonProperty("attributes")] public VyTokenAttribute[] Attributes { get; set; }
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

        public VyTokenAttribute GetAttribute(string name)
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
        [JsonProperty("attributes")] public VyTokenAttribute[] Attributes { get; set; }
    }

    //WALLET-API NFT-TOKEN (API v3 scheme)
    [Serializable]
    public class VyNonFungibleTokenDto
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
        [JsonProperty("attributes")] public VyTokenAttribute[] Attributes { get; set; }
        [JsonProperty("transferFees")] public bool TransferFees { get; set; }

        public bool HasAttribute(string name)
        {
            return Attributes.Any(att => att.Name.Equals(name));
        }

        public VyTokenAttribute GetAttribute(string name)
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

    //WALLET-API TOKEN (API v3 scheme)
    [Serializable]
    public class VyFungibleTokenDto
    {
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("rawBalance")] public string RawBalance { get; set; }
        [JsonProperty("balance")] public long Balance { get; set; }
        [JsonProperty("decimals")] public long Decimals { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("logo")] public string Logo { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("transferable")] public bool Transferable { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }


    [Serializable]
    public class VyTokenAttribute
    {
        [JsonProperty("type")] public eVyTokenAttributeType Type { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("value")] public object Value { get; set; }

        public T As<T>()
        {
            return (T) Convert.ChangeType(Value, typeof(T));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }


    [Serializable]
    public class VyMintedTokenInfoDto
    {
        public class VyMintedTokenDestination
        {
            [JsonProperty("destination")] public string Destination { get; private set; }
            [JsonProperty("tokenId")] public int TokenId { get; private set; }
            [JsonProperty("txHash")] public string TransactionHash { get; private set; }
        }

        [JsonProperty("metadata")] public JObject Metadata { get; private set; } //todo: create meta class?
        [JsonProperty("mintedTokens")] public VyMintedTokenDestination[] MintedTokens { get; private set; }
    }
}