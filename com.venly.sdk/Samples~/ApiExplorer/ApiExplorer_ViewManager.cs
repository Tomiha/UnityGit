//Sample View Manager
public class ApiExplorer_ViewManager : SampleViewManager<eApiExplorerViewId>
{
    public override string GetTitle(eApiExplorerViewId viewId)
    {
        switch (viewId)
        {
            case eApiExplorerViewId.Unknown: return "Unknown";
            case eApiExplorerViewId.Count: return "Ignore";
            case eApiExplorerViewId.Landing_DevMode: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Landing_RuntimeMode: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Landing_Auth: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Auth_Login: return "Login User";
            case eApiExplorerViewId.Auth_Create: return "Create User";
            case eApiExplorerViewId.Main_WalletApi: return "Wallet API";
            case eApiExplorerViewId.WalletApi_ViewWallets: return "View Wallets";
            case eApiExplorerViewId.WalletApi_CreateWallet: return "Create Wallet";
            case eApiExplorerViewId.WalletApi_WalletDetails: return "Wallet Details";
            case eApiExplorerViewId.WalletApi_ViewMultiTokens: return "Multi Tokens";
            case eApiExplorerViewId.WalletApi_ViewCryptoTokens: return "Crypto Tokens";
            case eApiExplorerViewId.Shared_MultiTokenDetails: return "Multi Token";
            case eApiExplorerViewId.Shared_CryptoTokenDetails: return "Crypto Token";
            case eApiExplorerViewId.WalletApi_TransferNativeToken: return "Transfer Native Token";
            case eApiExplorerViewId.WalletApi_ViewWalletEvents: return "Wallet Events";
            case eApiExplorerViewId.WalletApi_TransferCryptoToken: return "Transfer CryptoToken";
            case eApiExplorerViewId.WalletApi_TransferMultiToken: return "Transfer MultiToken";
            case eApiExplorerViewId.WalletApi_TransactionDetails: return "Transaction Details";
            case eApiExplorerViewId.Main_NftApi: return "NFT API";
            case eApiExplorerViewId.NftApi_ViewContracts: return "View Contracts";
            case eApiExplorerViewId.NftApi_ContractDetails: return "Contract Details";
            case eApiExplorerViewId.NftApi_ViewTokenTypes: return "View Token Types";
            case eApiExplorerViewId.NftApi_TokenTypeDetails: return "Token Type Details";
            case eApiExplorerViewId.NftApi_MintToken: return "Mint Token";
            default: return "Untitled";
        }
    }
}

//Sample View Id
public enum eApiExplorerViewId : uint
{
    Unknown = 0,
    Landing_DevMode,
    Landing_RuntimeMode,
    Landing_Auth,
    Auth_Login,
    Auth_Create,
    Main_WalletApi,
    WalletApi_ViewWallets,
    WalletApi_CreateWallet,
    WalletApi_WalletDetails,
    Shared_MultiTokenDetails,
    Shared_CryptoTokenDetails,
    WalletApi_ViewMultiTokens,
    WalletApi_ViewCryptoTokens,
    WalletApi_ViewWalletEvents,
    WalletApi_TransferNativeToken,
    WalletApi_TransferCryptoToken,
    WalletApi_TransferMultiToken,
    WalletApi_TransactionDetails,

    Main_NftApi,
    NftApi_ViewContracts,
    NftApi_ContractDetails,
    NftApi_ViewTokenTypes,
    NftApi_TokenTypeDetails,
    NftApi_MintToken,

    Count
}

public static class ApiExplorer_GlobalKeys
{
    public const string DATA_AllWalletsCached = "all-wallets-cache";
    public const string DATA_AllContractsCached = "all-contracts-cache";
    public const string DATA_Contract = "contract";
    public const string DATA_Wallet = "wallet";
}
