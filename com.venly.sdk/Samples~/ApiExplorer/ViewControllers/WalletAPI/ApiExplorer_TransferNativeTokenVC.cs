using System.Globalization;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransferNativeTokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_WALLET = "wallet";

    //DATA
    private VyWalletDto _sourceWallet;

    public ApiExplorer_TransferNativeTokenVC() : 
        base(eApiExplorerViewId.WalletApi_TransferNativeToken) { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-select-source", OnClick_SelectSource);
        BindButton("btn-select-target", OnClick_SelectTarget);
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Check if Source Is Set
        TryGetBlackboardData(out _sourceWallet, DATAKEY_WALLET, ApiExplorer_GlobalKeys.DATA_UserWallet);
        ToggleElement("btn-select-source", _sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);
    }

    private void UpdateSourceWallet(VyWalletDto sourceWallet)
    {
        _sourceWallet = sourceWallet;

        SetLabel("lbl-source-wallet", _sourceWallet == null?"select wallet":_sourceWallet.Id);
        SetLabel("lbl-token", _sourceWallet == null?"select wallet":_sourceWallet.Balance.Symbol);
        SetLabel("lbl-balance", _sourceWallet == null?"select wallet":_sourceWallet.Balance.Balance.ToString(CultureInfo.InvariantCulture));
    }

    protected override void OnDeactivate()
    {
       ClearBlackboardData("sourceWallet");

       _sourceWallet = null;
    }
    #endregion

    #region EVENTS
    private async void OnClick_SelectSource()
    {
        var selection = await ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet");
        if(!selection.Success) return;

        //Update Wallet
        var selectedWallet = selection.Data as VyWalletDto; //Will have no Balance
        ViewManager.Loader.Show("Refreshing Wallet Details...");
        var result = await VenlyAPI.Wallet.GetWallet(selectedWallet.Id);
        ViewManager.Loader.Hide();

        if (result.Success) UpdateSourceWallet(result.Data);
        else ViewManager.HandleException(result.Exception);
    }

    private void OnClick_SelectTarget()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    SetLabel("txt-target-address", wallet.Address);
                }
            });
    }

    private void OnClick_Transfer()
    {
        if (!ValidateData(_sourceWallet, "sourceWallet")) return;
        if (!ValidateInput("txt-pincode")) return;
        if (!ValidateInput("txt-target-address")) return;
        if (!ValidateInput<double>("txt-amount")) return;

        var reqParams = new VyTransactionNativeTokenTransferRequest()
        {
            Chain = _sourceWallet?.Chain ?? eVyChain.NotSupported,
            WalletId = _sourceWallet?.Id,
            ToAddress = GetValue("txt-target-address"),
            Value = GetValue<double>("txt-amount"),
            Data = "Venly Api Explorer Transaction (Native Token)"

        };

        ViewManager.Loader.Show("Transferring...");
        VenlyAPI.Wallet.ExecuteNativeTokenTransfer(GetValue("txt-pincode"), reqParams)
            .OnSuccess(transferInfo =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXHASH, transferInfo.TransactionHash);
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXCHAIN, _sourceWallet.Chain);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
