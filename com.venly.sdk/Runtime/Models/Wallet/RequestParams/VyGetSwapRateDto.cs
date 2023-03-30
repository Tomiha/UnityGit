using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyGetSwapRateDto
    {
        [JsonProperty("fromSecretType")] public eVyChain FromChain { get; set; }
        [JsonProperty("toSecretType")] public eVyChain ToChain { get; set; }
        [JsonProperty("fromToken")] public string FromToken { get; set; }
        [JsonProperty("toToken")] public string ToToken { get; set; }
        [JsonProperty("amount")] public double Amount { get; set; }
        [JsonProperty("orderType")] public eVyOrderType OrderType { get; set; }
    }
}