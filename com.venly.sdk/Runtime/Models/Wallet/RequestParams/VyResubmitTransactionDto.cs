using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyResubmitTransactionDto
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
    }
}