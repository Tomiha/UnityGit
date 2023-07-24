using UnityEngine.UIElements;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_CryptoTokenDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA
    private VyCryptoTokenDto _token;

    public ApiExplorer_CryptoTokenDetailsVC() : 
        base(eApiExplorerViewId.Shared_CryptoTokenDetails)
    {
    }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        TryGetBlackboardData(out _token, localKey: ApiExplorer_TokenDetailsDataKeys.DATAKEY_TOKEN);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_token, "token")) return;

        SetLabel("lbl-token-name", _token.Name);
        SetLabel("lbl-token-symbol", _token.Symbol);
        SetLabel("lbl-token-balance", _token.Balance.ToString());
        SetLabel("lbl-token-decimals", _token.Decimals.ToString());
        SetLabel("lbl-token-transferable", _token.Transferable ? "YES" : "NO");
        SetLabel("lbl-token-address", _token.TokenAddress);
    }
    #endregion

    #region EVENTS
    private void OnClick_Transfer()
    {
        var wallet = GetBlackBoardData<VyWalletDto>(ApiExplorer_TokenDetailsDataKeys.DATAKEY_WALLET);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferCryptoToken, ApiExplorer_TransferCryptoTokenVC.DATAKEY_WALLET, wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferCryptoToken, ApiExplorer_TransferCryptoTokenVC.DATAKEY_TOKEN, _token);

        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferCryptoToken);
    }
    #endregion
}
