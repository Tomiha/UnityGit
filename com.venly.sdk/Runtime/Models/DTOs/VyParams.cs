using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Utils;

namespace VenlySDK.Models
{
    [Serializable]
    public class VyGetSwapRateDto
    {
        [JsonProperty("fromSecretType")] public eVyChain FromChain { get; set; }
        [JsonProperty("toSecretType")] public eVyChain ToChain { get; set; }
        [JsonProperty("fromToken")] public string FromToken { get; set; }
        [JsonProperty("toToken")] public string ToToken { get; set; }
        [JsonProperty("amount")] public double Amount { get; set; }
        [JsonProperty("orderType")] public eVyOrderType OrderType { get; set; }
    }

    [Serializable]
    public class VyCreateWalletDto
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("identifier")] public string Identifier { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("walletType")] public eVyWalletType WalletType { get; set; }
    }

    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public abstract class VyExecuteBaseDto<T> where T : new()
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("transactionRequest")] public T Request { get; set; } = new T();
    }

    #region TransferRequests

    [Serializable]
    public class VyExecuteNativeTokenTransferDto : VyExecuteBaseDto<VyTransferNativeRequest>
    {
    }

    [Serializable]
    public class VyExecuteCryptoTokenTransferDto : VyExecuteBaseDto<VyTransferCryptoRequest>
    {
    }

    [Serializable]
    public class VyExecuteMultiTokenTransferDto : VyExecuteBaseDto<VyTransferMultiRequest>
    {
    }

    [Serializable]
    public class VyExecuteGasTransferDto : VyExecuteBaseDto<VyTransferGasRequest>
    {
    }

    [Serializable]
    public class VyExecuteContractDto : VyExecuteBaseDto<VyExecuteContractRequest>
    {
    }


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

    [Serializable]
    public class VyTransferNativeRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "TRANSFER";
        [JsonProperty("data")] public string Data { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
    }

    [Serializable]
    public class VyTransferCryptoRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "TOKEN_TRANSFER";
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
    }

    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyTransferMultiRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "NFT_TRANSFER";
        [JsonProperty("tokenAddress")] public string TokenAddress { get; set; }
        [JsonProperty("from")] public string FromAddress { get; set; }
        [JsonProperty("amount")] public int? Amount { get; set; }
        [JsonProperty("tokenId")] public int TokenId { get; set; }
    }

    [Serializable]
    public class VyTransferGasRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "GAS_TRANSFER";
        [JsonProperty("value")] public double Value { get; set; }
    }

    [Serializable]
    public class VyExecuteContractRequest : VyTransferRequestBase
    {
        [JsonProperty("type")] public string Type => "CONTRACT_EXECUTION";
        [JsonProperty("functionName")] public string FunctionName { get; set; }
        [JsonProperty("value")] public double Value { get; set; }
        [JsonProperty("inputs")] public VyContractInput[] Inputs { get; set; }
    }

    #endregion

    [Serializable]
    public class VyReadContractDto
    {
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("walletAddress")] public string WalletAddress { get; set; }
        [JsonProperty("contractAddress")] public string ContractAddress { get; set; }
        [JsonProperty("functionName")] public string FunctionName { get; set; }
        [JsonProperty("inputs")] public VyContractInput[] Inputs { get; set; }
        [JsonProperty("outputs")] public VyContractOutput[] Outputs { get; set; }
    }

    [Serializable]
    public class VyResubmitTransactionDto
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }
        [JsonProperty("transactionHash")] public string TransactionHash { get; set; }
    }

    [Serializable]
    public class VyExportWalletDto
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("walletId")] public string WalletId { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
    }

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


    [Serializable]
    public class VyUpdateWalletSecurityDto
    {
        [JsonIgnore] public string WalletId { get; set; }
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("newPincode")] public string NewPincode { get; set; }
        [JsonProperty("hasCustomPin")] public bool HasCustomPin { get; set; }
        [JsonProperty("recoverable")] public bool Recoverable { get; set; }
    }

    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class VyUpdateWalletMetadataDto
    {
        private VyUpdateWalletMetadataDto() { }
        public VyUpdateWalletMetadataDto(string walletId)
        { WalletId = walletId;}

        [JsonIgnore] public string WalletId { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("primary")] public bool? Primary { get; set; }
        [JsonProperty("archived")] public bool? Archived { get; set; }
    }

    //todo: check required/optional params (autoapproved...,storage)
    [Serializable]
    public class VyCreateContractDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
        [JsonProperty("owner")] public string Owner { get; set; }
        [JsonProperty("secretType")] public eVyChain Chain { get; set; }

        //[JsonProperty("autoApprovedAddressesLocked")] private bool _autoApprovedAddressesLocked = true;
        //[JsonProperty("storage")] public VyStorage Storage { get; set; }
    }

    //todo: check storage
    [Serializable]
    public class VyCreateTokenTypeDto
    {
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
    public class VyMintNonFungibleTokenDto
    {
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("typeId")] public int TokenId { get; set; }
        [JsonProperty("destinations")] public string[] Destinations { get; set; }
    }

    [Serializable]
    public class VyMintTokenDto
    {
        [JsonIgnore] public int ContractId { get; set; }
        [JsonIgnore] public int TokenId { get; set; }

        [JsonProperty("destinations")] public VyMintDestinationDto[] Destinations { get; set; }
    }

    [Serializable]
    public class VyMintDestinationDto
    {
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("amount")] public int Amount { get; set; }
    }

    [Serializable]
    public class VyMintFungibleTokenDto
    {
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("typeId")] public int TokenId { get; set; }
        [JsonProperty("destinations")] public string[] Destinations { get; set; }
        [JsonProperty("amounts")] public int[] Amounts { get; set; }
    }

    [Serializable]
    public class VyUpdateTokenTypeMetadataDto
    {
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
    public class VyUpdateContractMetadataDto
    {
        [JsonIgnore] public int ContractId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("symbol")] public string Symbol { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("image")] public string ImageUrl { get; set; }
        [JsonProperty("externalUrl")] public string ExternalUrl { get; set; }
        [JsonProperty("media")] public VyTypeValuePair[] Media { get; set; }
    }
}