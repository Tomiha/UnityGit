using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Venly.Utils;

namespace Venly.Models
{
    [Serializable]
    public class VyParam_GetSwapRate
    {
        [JsonProperty("fromSecretType")] public eVySecretType FromSecretType { get; set; }
        [JsonProperty("toSecretType")] public eVySecretType ToSecretType { get; set; }
        [JsonProperty("fromToken")] public string FromToken { get; set; }
        [JsonProperty("toToken")] public string ToToken { get; set; }
        [JsonProperty("amount")] public double Amount { get; set; }
        [JsonProperty("orderType")] public eVyOrderType OrderType { get; set; }
    }

    [Serializable]
    public class VyParam_CreateWallet
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("identifier")] public string Identifier { get; set; }
        [JsonProperty("secretType")] public eVySecretType SecretType { get; set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; set; }
    }

    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyParam_Execute<T> where T : new()
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("transactionRequest")] public T Request { get; set; } = new T();
    }

    #region TransferRequests
    [Serializable] public class VyParam_ExecuteTransfer:VyParam_Execute<VyTransferRequest>{}
    [Serializable] public class VyParam_ExecuteTokenTransfer:VyParam_Execute<VyTokenTransferRequest>{}
    [Serializable] public class VyParam_ExecuteNftTransfer:VyParam_Execute<VyNftTransferRequest>{}
    [Serializable] public class VyParam_ExecuteGasTransfer:VyParam_Execute<VyGasTransferRequest>{}
    [Serializable] public class VyParam_ExecuteContract:VyParam_Execute<VyContractExecutionRequest>{}


    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyTransferRequestBase
    {
        [JsonProperty("walletId")] public string WalletId { get; set; }
        [JsonProperty("to")] public string ToAddress { get; set; }
        [JsonProperty("secretType")] public eVySecretType SecretType { get; set; }
        //todo: network + chainSpecificFields
        //[JsonProperty("chainSpecificFields")] public JObject ChainSpecificFields { get; set; }
    }

    [Serializable]
    public class VyTransferRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "TRANSFER";
        [JsonProperty("data")] public string Data { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
    }

    [Serializable]
    public class VyTokenTransferRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "TOKEN_TRANSFER";
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
        [JsonProperty("tokenId")] public Int64 TokenId { get; set; }
    }

    [Serializable]
    public class VyNftTransferRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "NFT_TRANSFER";
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("from")] public string FromAddress { get; set; }
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("tokenId")] public int TokenId { get; set; }
    }

    [Serializable]
    public class VyGasTransferRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "GAS_TRANSFER";
        [JsonProperty("value")] public double Value { get; set; }
    }

    [Serializable]
    public class VyContractExecutionRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "CONTRACT_EXECUTION";
        [JsonProperty("functionName")] public string FunctionName { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
        [JsonProperty("inputs")] public VyContractInput[] Inputs { get; set; }
    }
    #endregion

    [Serializable]
    public class VyParam_ReadContract
    {
        [JsonProperty("secretType")] public eVySecretType SecretType { get; set; }
        [JsonProperty("walletAddress")] public string WalletAddress { get; set; }
        [JsonProperty("contractAddress")] public string ContractAddress { get; set; }
        [JsonProperty("functionName")] public string FunctionName { get; set; }
        [JsonProperty("inputs")] public VyContractInput[] Inputs { get; set; }
        [JsonProperty("outputs")] public VyContractOutput[] Outputs { get; set; }
    }

    [Serializable]
    public class VyParam_ResubmitTransaction
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("secretType")] public eVySecretType SecretType { get; set; }
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
    }

    [Serializable]
    public class VyParam_ExportWallet
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("walletId")] public string WalletId { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
    }

    [Serializable]
    public abstract class VyParam_ImportWallet
    {
        [JsonProperty("importWalletType")] private string ImportWalletTypeFull => $"{SourceChain.GetMemberName()}_{ImportType}";
        [JsonIgnore] public eVySecretType SourceChain { get; set; }
        [JsonIgnore] private string ImportType { get; set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; set; }
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("clients")] public string[] Clients { get; set; }

        public VyParam_ImportWallet(string importType)
        {
            ImportType = importType;
        }
    }

    [Serializable]
    public class VyParamImportWalletKeystore : VyParam_ImportWallet
    {
        [JsonProperty("keystore")] public JObject Keystore { get; set; }
        [JsonProperty("password")] public string Password { get; set; }

        public VyParamImportWalletKeystore() : base("KEYSTORE") {}
    }

    [Serializable]
    public class VyParamImportWalletPrivateKey : VyParam_ImportWallet
    {
        [JsonProperty("privateKey")] public string PrivateKey { get; set; }

        public VyParamImportWalletPrivateKey() : base("PRIVATE_KEY") {}
    }

    [Serializable]
    public class VyParamImportWalletWif : VyParam_ImportWallet
    {
        [JsonProperty("wif")] public string Wif { get; set; }

        public VyParamImportWalletWif() : base("WIF") {}
    }


    [Serializable]
    public class VyParam_UpdateWalletSecurity
    {
        [JsonIgnore]public string WalletId { get; set; }
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("newPincode")] public string NewPincode { get; set; }
        [JsonProperty("hasCustomPin")] public bool HasCustomPin { get; set; }
        [JsonProperty("recoverable")] public bool Recoverable { get; set; }
    }

    //todo: check required/optional params (autoapproved...,storage)
    [Serializable]
    public class VyParam_CreateContract
    {
        [JsonIgnore] public string ApplicationId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
        [JsonProperty("owner")] public string Owner { get; set; }
        [JsonProperty("chain")] public eVySecretType Chain { get; set; }

        //[JsonProperty("autoApprovedAddressesLocked")] private bool _autoApprovedAddressesLocked = true;
        //[JsonProperty("storage")] public VyStorage Storage { get; set; }
    }

    //todo: check storage
    [Serializable]
    public class VyParam_CreateTokenType
    {
        [JsonIgnore] public string ApplicationId { get; set; }
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("fungible")] public bool Fungible { get; set; }
        [JsonProperty("burnable")] public bool Burnable { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("maxSupply")] public int MaxSupply { get; set; }
        [JsonProperty("attributes")] public VyTokenAttribute[] Attributes { get; set; }
        [JsonProperty("destinations")] public VyTokenDestination[] Destinations { get; set; }
        //[JsonProperty("storage")] public VyStorage Storage { get; set; }
    }

    [Serializable]
    public class VyParam_MintNFT
    {
        [JsonIgnore] public string ApplicationId { get; set; }
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("typeId")] public int TokenId { get; set; }
        [JsonProperty("destinations")] public string[] Destinations { get; set; }
    }

    [Serializable]
    public class VyParam_MintFT
    {
        [JsonIgnore] public string ApplicationId { get; set; }
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("typeId")] public int TokenId { get; set; }
        [JsonProperty("destinations")] public string[] Destinations { get; set; }
        [JsonProperty("amounts")] public int[] Amounts { get; set; }
    }

    [Serializable]
    public class VyParam_UpdateTokenTypeMetadata
    {
        [JsonIgnore] public string ApplicationId { get; set; }
        [JsonIgnore] public int ContractId { get; set; }
        [JsonIgnore] public int TokenTypeId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonProperty("animationUrls")] public VyTypeValuePair[] AnimationUrls { get; set; }
        [JsonProperty("attributes")] public VyTokenAttribute[] Atrributes { get; set; }
    }

    [Serializable]
    public class VyParam_UpdateContractMetadata
    {
        [JsonIgnore] public string ApplicationId { get; set; }
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
    }
}
