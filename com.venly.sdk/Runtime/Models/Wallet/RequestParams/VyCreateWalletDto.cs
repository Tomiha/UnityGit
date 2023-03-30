using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyCreateWalletDto
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("identifier")] public string Identifier { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; set; }
    }
}