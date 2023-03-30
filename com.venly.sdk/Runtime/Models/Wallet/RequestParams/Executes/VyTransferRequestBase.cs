using System;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyTransferRequestBase
    {
        [JsonProperty("walletId")] public string WalletId { get; set; }
        [JsonProperty("to")] public string ToAddress { get; set; }

        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        //todo: network + chainSpecificFields
        //[JsonProperty("chainSpecificFields")] public JObject ChainSpecificFields { get; set; }
    }
}