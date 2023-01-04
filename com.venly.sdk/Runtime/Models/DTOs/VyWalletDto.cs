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
        [JsonProperty("id")] public string Id { get; private set; }
        [JsonProperty("address")] public string Address { get; private set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; private set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; private set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; private set; }
        [JsonProperty("archived")] public string Archived { get; private set; }
        [JsonProperty("description")] public string Description { get; private set; }
        [JsonProperty("primary")] public bool Primary { get; private set; }
        [JsonProperty("hasCustomerPin")] public bool HasCustomerPin { get; private set; }
        [JsonProperty("identifier")] public string Identifier { get; private set; }
        [JsonProperty("balance")] public VyWalletBalanceDto Balance {get; private set; }
    }

    [Serializable]
    public class VyWalletBalanceDto
    {
        [JsonProperty("available")] public bool Available { get; private set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; private set; }
        [JsonProperty("balance")] public float Balance { get; private set; }
        [JsonProperty("gasBalance")] public float GasBalance { get; private set; }
        [JsonProperty("symbol")] public string Symbol { get; private set; }
        [JsonProperty("gasSymbol")] public string GasSymbol { get; private set; }
        [JsonProperty("rawBalance")] public double RawBalance { get; private set; }
        [JsonProperty("rawGasBalance")] public double RawGasBalance { get; private set; }
        [JsonProperty("decimals")] public int Decimals {get; private set; }
    }
}