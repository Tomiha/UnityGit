using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyUpdateWalletSecurityDto
    {
        [JsonIgnore] public string WalletId { get; set; }
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("newPincode")] public string NewPincode { get; set; }
        [JsonProperty("hasCustomPin")] public bool HasCustomPin { get; set; }
        [JsonProperty("recoverable")] public bool Recoverable { get; set; }
    }
}
