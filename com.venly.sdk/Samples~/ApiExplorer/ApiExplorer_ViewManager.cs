//Sample View Manager
public class ApiExplorer_ViewManager : SampleViewManager<eApiExplorerViewId>
{
    public override string GetTitle(eApiExplorerViewId viewId)
    {
        switch (viewId)
        {
            case eApiExplorerViewId.Landing_DevMode: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Landing_RuntimeMode: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Landing_Auth: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Auth_Login: return "Login User";
            case eApiExplorerViewId.Auth_Create: return "Create User";
            case eApiExplorerViewId.Main_WalletApi: return "Wallet API";
            case eApiExplorerViewId.WalletApi_ViewWallets: return "View Wallets";
            case eApiExplorerViewId.WalletApi_CreateWallet: return "Create Wallet";
            case eApiExplorerViewId.WalletApi_WalletDetails: return "Wallet Details";
            case eApiExplorerViewId.Shared_MultiTokenDetails: return "Multi Token";
            case eApiExplorerViewId.Shared_CryptoTokenDetails: return "Crypto Token";
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

    Count
}

public static class ApiExplorer_GlobalKeys
{
    public const string DATA_AllWalletsCached = "all-wallets-cache";
}
