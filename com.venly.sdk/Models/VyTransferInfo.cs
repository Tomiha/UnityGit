using System;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public class VyTransferInfo
    {
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
        [JsonProperty("transactionDetails")] public string Details { get; set; }
    }
}