using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyReadContractDto
    {
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("walletAddress")] public string WalletAddress { get; set; }
        [JsonProperty("contractAddress")] public string ContractAddress { get; set; }
        [JsonProperty("functionName")] public string FunctionName { get; set; }
        [JsonProperty("inputs")] public VyContractInput[] Inputs { get; set; }
        [JsonProperty("outputs")] public VyContractOutput[] Outputs { get; set; }
    }
}
