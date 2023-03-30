using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VenlySDK.Models.Nft
{
    [Serializable]
    public class VyMintedTokenInfoDto
    {
        public class VyMintedTokenDestination
        {
            [JsonProperty("destination")] public string Destination { get; private set; }
            [JsonProperty("tokenId")] public int TokenId { get; private set; }
            [JsonProperty("txHash")] public string TransactionHash { get; private set; }
        }

        [JsonProperty("metadata")] public JObject Metadata { get; private set; } //todo: create meta class?
        [JsonProperty("mintedTokens")] public VyMintedTokenDestination[] MintedTokens { get; private set; }
    }
}