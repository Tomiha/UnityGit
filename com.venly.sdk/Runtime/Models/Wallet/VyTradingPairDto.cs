using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyTradingPairDetailsDto
    {
        [JsonProperty("secretType")] public eVyChain Chain { get; private set; }
        [JsonProperty("symbol")] public string Symbol { get; private set; }
        [JsonProperty("tokenAddress")] public string TokenAddress { get; private set; }
    }

    [Serializable]
    public class VyTradingPairDto
    {
        [JsonProperty("from")] public VyTradingPairDetailsDto From { get; private set; }
        [JsonProperty("to")] public VyTradingPairDetailsDto To { get; private set; }
    }
}