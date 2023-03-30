using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Models.Shared;
using VenlySDK.Utils;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    public abstract class VyImportWalletBaseDto
    {
        [JsonProperty("importWalletType")]
        private string ImportWalletTypeFull => $"{SourceChain.GetMemberName()}_{ImportType}";

        [JsonIgnore] public eVyChain SourceChain { get; set; }
        [JsonIgnore] private string ImportType { get; set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; set; }
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("clients")] public string[] Clients { get; set; }

        protected VyImportWalletBaseDto(string importType)
        {
            ImportType = importType;
        }
    }

    [Serializable]
    public class VyImportWalletKeystoreDto : VyImportWalletBaseDto
    {
        [JsonProperty("keystore")] public JObject Keystore { get; set; }
        [JsonProperty("password")] public string Password { get; set; }

        public VyImportWalletKeystoreDto() : base("KEYSTORE")
        {
        }
    }

    [Serializable]
    public class VyImportWalletPrivateKeyDto : VyImportWalletBaseDto
    {
        [JsonProperty("privateKey")] public string PrivateKey { get; set; }

        public VyImportWalletPrivateKeyDto() : base("PRIVATE_KEY")
        {
        }
    }

    [Serializable]
    public class VyImportWalletWifDto : VyImportWalletBaseDto
    {
        [JsonProperty("wif")] public string Wif { get; set; }

        public VyImportWalletWifDto() : base("WIF")
        {
        }
    }
}
