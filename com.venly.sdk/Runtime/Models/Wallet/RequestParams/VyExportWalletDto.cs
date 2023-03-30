using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExportWalletDto
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("walletId")] public string WalletId { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
    }
}