using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public class VyExecuteMultiTokenTransferDto : VyExecuteBase<VyTransferMultiTokenRequest>
    {
    }

    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyTransferMultiTokenRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "NFT_TRANSFER";
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("from")] public string FromAddress { get; set; }
        [JsonProperty("amount")] public int? Amount { get; set; }
        [JsonProperty("tokenId")] public int TokenId { get; set; }
    }
}