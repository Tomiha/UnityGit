using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Nft
{
    [Serializable]
    public class VyMintTokenDto
    {
        public class VyMintDestinationDto
        {
            [JsonProperty("address")] public string Address { get; set; }
            [JsonProperty("amount")] public int Amount { get; set; }
        }

        [JsonIgnore] public int ContractId { get; set; }
        [JsonIgnore] public int TokenId { get; set; }

        [JsonProperty("destinations")] public VyMintDestinationDto[] Destinations { get; set; }
    }
}