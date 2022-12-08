using System;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public class VyTradingPairDetails
    {
        [JsonProperty("secretType")] public eVySecretType SecretType { get; private set; }
        [JsonProperty("symbol")] public string Symbol { get; private set; }
        [JsonProperty("tokenAddress")] public string TokenAddress { get; private set; }
    }

    [Serializable]
    public class VyTradingPair
    {
        [JsonProperty("from")] public VyTradingPairDetails From { get; private set; }
        [JsonProperty("to")] public VyTradingPairDetails To { get; private set; }
    }
}