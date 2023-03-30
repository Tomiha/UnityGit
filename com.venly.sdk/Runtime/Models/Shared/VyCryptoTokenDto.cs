using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Shared
{
    /// <summary>
    /// ERC20 Token
    /// </summary>
    [Serializable]
    public class VyCryptoTokenDto
    {
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("rawBalance")] public string RawBalance { get; set; }
        [JsonProperty("balance")] public double Balance { get; set; }
        [JsonProperty("decimals")] public Int32 Decimals { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("logo")] public string Logo { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("transferable")] public bool Transferable { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }
}