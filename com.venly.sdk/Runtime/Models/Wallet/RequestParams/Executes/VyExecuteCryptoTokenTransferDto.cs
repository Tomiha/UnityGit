using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExecuteCryptoTokenTransferDto : VyExecuteBase<VyTransferCryptoRequest>
    {
    }

    [Serializable]
    public class VyTransferCryptoRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "TOKEN_TRANSFER";
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
    }
}