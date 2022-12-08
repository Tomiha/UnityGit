using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Venly.Models
{
    [Serializable]
    public class VyTypeValuePair
    {
        [JsonProperty("type")] public string Type { get; internal set; }
        [JsonProperty("value")] public string Value { get; internal set; }
    }

    [Serializable]
    public class VyContractInput
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }

    [Serializable]
    public class VyContractOutput
    {
        [JsonProperty("type")] public string Type { get; private set; }
    }

    [Serializable]
    public class VyStorage
    {
        [JsonProperty("type")] public eVyStorageType Type { get; set; }
        [JsonProperty("location")] public string Location { get; set; }
    }

    [Serializable]
    public class VyTokenDestination
    {
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("amount")] public int Amount { get; set; }
    }
}