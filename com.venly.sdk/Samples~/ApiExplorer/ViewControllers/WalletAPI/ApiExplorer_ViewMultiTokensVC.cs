using VenlySDK;
using VenlySDK.Core;
using VenlySDK.Models.Shared;

public class ApiExplorer_ViewMultiTokensVC : ApiExplorer_ViewTokensBaseVC<VyMultiTokenDto, VyControl_MultiTokenListView, VyControl_MultiTokenListItem>
{
    public ApiExplorer_ViewMultiTokensVC() : base(eApiExplorerViewId.WalletApi_ViewMultiTokens) { }

    protected override eApiExplorerViewId DetailViewId => eApiExplorerViewId.Shared_MultiTokenDetails;

    protected override VyTask<VyMultiTokenDto[]> GetTokenBalances(string walletId)
    {
        return Venly.WalletAPI.Client.GetMultiTokenBalances(walletId);
    }
}
