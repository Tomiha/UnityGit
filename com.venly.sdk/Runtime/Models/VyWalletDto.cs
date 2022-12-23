using System;
using Newtonsoft.Json;

namespace VenlySDK.Models
{
    [Serializable]
    public class VyWalletValidationDto
    {
        [JsonProperty("valid")] public bool Valid { get; set; }
    }

    [Serializable]
    public class VyWalletExportDto
    {
        [JsonProperty("keystore")] public string Keystore { get; private set; }
    }

    [Serializable]
    public class VyWalletDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("archived")] public string Archived { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("primary")] public bool Primary { get; set; }
        [JsonProperty("hasCustomerPin")] public bool HasCustomerPin { get; set; }
        [JsonProperty("identifier")] public string Identifier { get; set; }
        [JsonProperty("balance")] public VyWalletBalanceDto Balance { get; set; }
    }

    [Serializable]
    public class VyWalletBalanceDto
    {
        [JsonProperty("available")] public bool Available { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("balance")] public float Balance { get; set; }
        [JsonProperty("gasBalance")] public float GasBalance { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("gasSymbol")] public string GasSymbol { get; set; }
        [JsonProperty("rawBalance")] public double RawBalance { get; set; }
        [JsonProperty("rawGasBalance")] public double RawGasBalance { get; set; }
        [JsonProperty("decimals")] public int Decimals { get; set; }
    }
}