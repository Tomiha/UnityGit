using System;
using Newtonsoft.Json;

namespace VenlySDK.Models
{
    [Serializable]
    public class VyTransferInfoDto
    {
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
        [JsonProperty("transactionDetails")] public string Details { get; set; }
    }
}