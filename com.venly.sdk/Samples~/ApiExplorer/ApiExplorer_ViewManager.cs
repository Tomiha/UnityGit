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
            case eApiExplorerViewId.Auth_UserPortal: return "User Portal";

            case eApiExplorerViewId.Shared_MultiTokenDetails: return "Multi Token";
            case eApiExplorerViewId.Shared_CryptoTokenDetails: return "Crypto Token";

            case eApiExplorerViewId.Main_WalletApi: return "Wallet API";
            case eApiExplorerViewId.WalletApi_ViewWallets: return "View Wallets";
            case eApiExplorerViewId.WalletApi_CreateWallet: return "Create Wallet";
            case eApiExplorerViewId.WalletApi_WalletDetails: return "Wallet Details";
            case eApiExplorerViewId.WalletApi_ViewMultiTokens: return "Multi Tokens";
            case eApiExplorerViewId.WalletApi_ViewCryptoTokens: return "Crypto Tokens";
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

            case eApiExplorerViewId.Main_MarketApi: return "MARKET API";
            case eApiExplorerViewId.MarketApi_ViewSubUsers: return "View Sub-Users";
            case eApiExplorerViewId.MarketApi_CreateSubUser: return "Create Sub-User";
            case eApiExplorerViewId.MarketApi_SubUserDetails: return "Sub-User Details";
            case eApiExplorerViewId.MarketApi_CreateOffer: return "Create Offer";
            case eApiExplorerViewId.MarketApi_ViewOffers: return "View Offers";
            case eApiExplorerViewId.MarketApi_OfferDetails: return "Offer Details";
            case eApiExplorerViewId.MarketApi_CreateDepositAddress: return "Create Deposit Addr";
            case eApiExplorerViewId.MarketApi_BuyOffer: return "Buy Offer";
            case eApiExplorerViewId.MarketApi_PlaceBid: return "Place Bid";
            case eApiExplorerViewId.MarketApi_ViewFulfillments: return "View Fulfillments";

            default: return "Untitled";
        }
    }
}

//Sample View Id
public enum eApiExplorerViewId : uint
{
    Unknown = 0,

    Landing_DevMode = 1,
    Landing_RuntimeMode = 2,
    Landing_Auth = 3,

    Auth_Login = 4,
    Auth_Create = 5,
   
    Shared_MultiTokenDetails = 6,
    Shared_CryptoTokenDetails = 7,

    Main_WalletApi = 8,
    WalletApi_ViewWallets = 9,
    WalletApi_CreateWallet = 10,
    WalletApi_WalletDetails = 11,
    WalletApi_ViewMultiTokens = 12,
    WalletApi_ViewCryptoTokens = 13,
    WalletApi_ViewWalletEvents = 14,
    WalletApi_TransferNativeToken = 15,
    WalletApi_TransferCryptoToken = 16,
    WalletApi_TransferMultiToken = 17,
    WalletApi_TransactionDetails = 18,

    Main_NftApi = 19,
    NftApi_ViewContracts = 20,
    NftApi_ContractDetails = 21,
    NftApi_ViewTokenTypes = 22,
    NftApi_TokenTypeDetails = 23,
    NftApi_MintToken = 24,

    Main_MarketApi = 25,
    MarketApi_ViewSubUsers = 26,
    MarketApi_CreateSubUser = 27,
    MarketApi_SubUserDetails = 28,
    MarketApi_CreateOffer = 29,
    MarketApi_ViewOffers = 30,
    MarketApi_OfferDetails = 31,
    MarketApi_PlaceBid = 32,
    MarketApi_CreateDepositAddress = 33,
    MarketApi_BuyOffer = 34,
    MarketApi_ViewFulfillments = 35,

    Auth_UserPortal = 36,

    Count
}

public static class ApiExplorer_GlobalKeys
{
    public const string DATA_AllWalletsCached = "all-wallets-cache";
    public const string DATA_AllSubUsersCached = "all-subUsers-cache";
    public const string DATA_AllContractsCached = "all-contracts-cache";
    public const string DATA_Contract = "contract";
    public const string DATA_UserWallet = "user-wallet";
    public const string DATA_UserMarketProfile = "user-market-profile";
}
