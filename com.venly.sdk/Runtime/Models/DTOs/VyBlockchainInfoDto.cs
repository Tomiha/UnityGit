using System;
using Newtonsoft.Json;

namespace VenlySDK.Models
{
    [Serializable]
    public class VyBlockchainInfoDto
    {
        [JsonProperty("blockNumber")] public int BlockNumber { get; set; }

        [JsonProperty("requiredConfirmations")]
        public int RequiredConfirmations { get; set; }

        [JsonProperty("chainId")] public int ChainId { get; set; }
    }
}