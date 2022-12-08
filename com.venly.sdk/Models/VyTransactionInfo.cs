using System;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public class VyTransactionInfo
    {
        [JsonProperty("hash")] public string Hash { get; set; }
        [JsonProperty("status")] public eVyTransactionStatus Status { get; set; }
        [JsonProperty("confirmations")] public int Confirmations { get; set; }
        [JsonProperty("blockHash")] public string BlockHash { get; set; }
        [JsonProperty("blockNumber")] public int BlockNumber { get; set; }
        [JsonProperty("hasReachedFinality")] public bool HasReachedFinality { get; set; }
    }
}