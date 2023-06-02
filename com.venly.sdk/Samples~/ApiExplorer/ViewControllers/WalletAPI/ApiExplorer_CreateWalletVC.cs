using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Toggle = UnityEngine.UIElements.Toggle;

public class ApiExplorer_CreateWalletVC : SampleViewBase<eApiExplorerViewId>
{
    private DropdownField _selectorChains;
    private TextField _txtIdentifier;
    private TextField _txtDescription;
    private TextField _txtPincode;
    private Toggle _toggleRecoverable;

    public ApiExplorer_CreateWalletVC() : 
        base(eApiExplorerViewId.WalletApi_CreateWallet) { }

    protected override void OnBindElements(VisualElement root)
    {
        _selectorChains = GetElement<DropdownField>("selector-chain");
        _txtIdentifier = GetElement<TextField>("txt-identifier");
        _txtDescription = GetElement<TextField>("txt-description");
        _txtPincode = GetElement<TextField>("txt-pincode");
        _toggleRecoverable = GetElement<Toggle>("toggle-recoverable");

        BindButton("btn-create", OnClick_Create);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        //Populate Selector
        _selectorChains.choices = Enum.GetNames(typeof(eVyChain)).ToList();
        _selectorChains.choices.Remove(Enum.GetName(typeof(eVyChain), eVyChain.NotSupported));
    }

    protected override void OnDeactivate()
    {
       
    }

    private void OnClick_Create()
    {
#if ENABLE_VENLY_DEV_MODE
        var selectedChain = Enum.Parse<eVyChain>(_selectorChains.value);

        var createParams = new VyCreateWalletRequest()
        {
            Chain = selectedChain,
            Description = _txtDescription.value,
            Identifier = _txtIdentifier.value,
            Pincode = _txtPincode.value,
            WalletType = _toggleRecoverable.value ? eVyWalletType.WhiteLabel : eVyWalletType.UnrecoverableWhiteLabel
        };

        ViewManager.Loader.Show("Creating Wallet..");
        VenlyAPI.Wallet.CreateWallet(createParams)
            .OnSuccess(wallet =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.DATAKEY_WALLET, wallet);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails);
            })
            .OnFail(Debug.LogException)
            .Finally(ViewManager.Loader.Hide);
#endif
    }
}
