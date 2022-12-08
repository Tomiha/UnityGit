using System;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public class VyBlockchainInfo
    {
        [JsonProperty("blockNumber")] public int BlockNumber { get; set; }
        [JsonProperty("requiredConfirmations")] public int RequiredConfirmations { get; set; }
        [JsonProperty("chainId")] public int ChainId { get; set; }
    }
}