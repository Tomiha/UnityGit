using Venly.Models.Market;
using Venly.Models.Wallet;

public class ApiExplorer_UserInfo
{
    public const string DATAKEY_PROVIDER_USERINFO = "provider-user-info";

    public VyWalletDto Wallet { get; set; }
    public VyUserProfileDto MarketUser { get; set; }
}