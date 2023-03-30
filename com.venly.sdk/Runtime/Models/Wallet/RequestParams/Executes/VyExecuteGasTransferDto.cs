using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExecuteGasTransferDto : VyExecuteBase<VyTransferGasRequest>
    {
    }

    [Serializable]
    public class VyTransferGasRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "GAS_TRANSFER";
        [JsonProperty("value")] public double Value { get; set; }
    }
}