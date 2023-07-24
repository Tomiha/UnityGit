using System;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_CreateWalletVC : SampleViewBase<eApiExplorerViewId>
{
    [UIBind("selector-chain")] private DropdownField _selectorChains;

    public ApiExplorer_CreateWalletVC() : 
        base(eApiExplorerViewId.WalletApi_CreateWallet) { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", OnClick_CreateWallet);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;

        //Populate Selector
        _selectorChains.FromEnum(eVyChain.Matic);
        _selectorChains.choices.Remove(Enum.GetName(typeof(eVyChain), eVyChain.NotSupported));
    }
    #endregion

    #region EVENTS
    private void OnClick_CreateWallet()
    {
        if (!ValidateInput("txt-pincode")) return;

        var createParams = new VyCreateWalletRequest()
        {
            Chain = _selectorChains.GetValue<eVyChain>(),
            Description = GetValue("txt-description"),
            Identifier = GetValue("txt-identifier"),
            Pincode = GetValue("txt-pincode"),
            WalletType = GetToggleValue("toggle-recoverable") ? eVyWalletType.WhiteLabel : eVyWalletType.UnrecoverableWhiteLabel
        };

        ViewManager.Loader.Show("Creating Wallet..");
        VenlyAPI.Wallet.CreateWallet(createParams)
            .OnSuccess(wallet =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.DATAKEY_WALLET, wallet);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.Exception.Show)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
