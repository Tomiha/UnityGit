using VenlySDK;
using VenlySDK.Core;
using VenlySDK.Models;

public class ApiExplorer_ViewCryptoTokensVC : ApiExplorer_ViewTokensBaseVC<VyCryptoTokenDto, VyControl_CryptoTokenListView
    , VyControl_CryptoTokenListItem>
{
    public ApiExplorer_ViewCryptoTokensVC() : base(eApiExplorerViewId.WalletApi_ViewCryptoTokens) { }

    protected override eApiExplorerViewId DetailViewId => eApiExplorerViewId.Shared_CryptoTokenDetails;

    protected override VyTask<VyCryptoTokenDto[]> GetTokenBalances(string walletId)
    {
        return Venly.WalletAPI.Client.GetCryptoTokenBalances(walletId);
    }
}