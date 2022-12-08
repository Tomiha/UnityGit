using System;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public class VyExchangeRateResult
    {
        [JsonProperty("exchangeRates")] public VyExchangeRate[] ExchangeRates { get; private set; }
        [JsonProperty("bestRate")] public VyExchangeRate BestRate { get; private set; }
    }

    [Serializable]
    public class VyExchangeRate
    {
        [JsonProperty("exchange")] public eVyExchange Exchange { get; private set; }
        [JsonProperty("orderType")] public eVyOrderType OrderType { get; private set; }
        [JsonProperty("inputAmount")] public double InputAmount { get; private set; }
        [JsonProperty("outputAmount")] public double OutputAmount { get; private set; }
        [JsonProperty("slippage")] public double Slippage { get; private set; }
        [JsonProperty("fee")] public double Fee { get; private set; }
    }
}