using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Models.Shared;

namespace VenlySDK.Models.Wallet
{
    public class VySignDto
    {
        public string Pincode { get; set; }
        public string Type { get; set; }
        public eVyChain Chain { get; set; }
        public string WalletId { get; set; }
        public JObject Data { get; set; }

        internal VySignDto_Internal ToInternalDTO()
        {
            return new VySignDto_Internal()
            {
                Pincode = Pincode,
                SignatureRequest = new VySignDto_Internal.VySignatureRequest()
                {
                    Type = Type,
                    Chain = Chain,
                    WalletId = WalletId,
                    Data = Data
                }
            };
        }
    }

    [Serializable]
    internal class VySignDto_Internal
    {
        [Serializable]
        internal class VySignatureRequest
        {
            [JsonProperty("type")] public string Type { get; set; }
            [JsonProperty("secretType")] public eVyChain Chain { get; set; }
            [JsonProperty("walletId")] public string WalletId { get; set; }
            [JsonProperty("data")] public JObject Data { get; set; }
        }

        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("signatureRequest")] public VySignatureRequest SignatureRequest { get; set; }
    }

}
