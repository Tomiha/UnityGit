using System;
using Newtonsoft.Json;

namespace Venly.Models
{
    [Serializable]
    public class VyWalletEvent
    {
        [JsonProperty("eventTimestamp")] public DateTime Timestamp { get; set; }
        [JsonProperty("client")] public string Client { get; set; }
        [JsonProperty("apiToken")] public string ApiToken { get; set; }
        [JsonProperty("type")] public string EventType { get; set; }
        [JsonProperty("metadata")] public string Metadata { get; set; }
        [JsonProperty("walletId")] public string walletId { get; set; }
    }
}