using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VySignatureDto
    {
        [JsonProperty("type")] public string Type { get; private set; }
        [JsonProperty("r")] public string R { get; private set; }
        [JsonProperty("s")] public string S { get; private set; }
        [JsonProperty("v")] public string V { get; private set; }
        [JsonProperty("signature")] public string Signature { get; private set; }
    }
}
