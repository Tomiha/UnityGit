using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExecuteContractDto : VyExecuteBase<VyExecuteContractRequest>
    {
    }

    [Serializable]
    public class VyExecuteContractRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "CONTRACT_EXECUTION";
        [JsonProperty("functionName")] public string FunctionName { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
        [JsonProperty("inputs")] public VyContractInput[] Inputs { get; set; }
    }
}