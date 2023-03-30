using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyUpdateWalletMetadataDto
    {
        private VyUpdateWalletMetadataDto() { }
        public VyUpdateWalletMetadataDto(string walletId)
        { WalletId = walletId; }

        [JsonIgnore] public string WalletId { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("primary")] public bool? Primary { get; set; }
        [JsonProperty("archived")] public bool? Archived { get; set; }
    }
}