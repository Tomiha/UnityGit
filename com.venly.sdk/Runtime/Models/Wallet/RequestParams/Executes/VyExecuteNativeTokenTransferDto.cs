using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExecuteNativeTokenTransferDto : VyExecuteBase<VyTransferNativeRequest>
    {
    }

    [Serializable]
    public class VyTransferNativeRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "TRANSFER";
        [JsonProperty("data")] public string Data { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
    }
}