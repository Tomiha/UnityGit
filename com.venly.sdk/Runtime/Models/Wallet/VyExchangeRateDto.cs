using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExchangeRateResultDto
    {
        [JsonProperty("exchangeRates")] public VyExchangeRateDto[] ExchangeRates { get; private set; }
        [JsonProperty("bestRate")] public VyExchangeRateDto BestRate { get; private set; }
    }

    [Serializable]
    public class VyExchangeRateDto
    {
        [JsonProperty("exchange")] public eVyExchange Exchange { get; private set; }
        [JsonProperty("orderType")] public eVyOrderType OrderType { get; private set; }
        [JsonProperty("inputAmount")] public double InputAmount { get; private set; }
        [JsonProperty("outputAmount")] public double OutputAmount { get; private set; }
        [JsonProperty("slippage")] public double Slippage { get; private set; }
        [JsonProperty("fee")] public double Fee { get; private set; }
    }
}