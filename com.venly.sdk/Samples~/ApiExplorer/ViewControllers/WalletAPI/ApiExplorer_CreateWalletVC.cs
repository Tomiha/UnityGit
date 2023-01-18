using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Core;
using VenlySDK.Models;
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
#if ENABLE_VENLY_DEVMODE
        var selectedChain = Enum.Parse<eVyChain>(_selectorChains.value);

        VyCreateWalletDto createParams = new VyCreateWalletDto
        {
            Chain = selectedChain,
            Description = _txtDescription.value,
            Identifier = _txtIdentifier.value,
            Pincode = _txtPincode.value,
            WalletType = _toggleRecoverable.value ? eVyWalletType.WhiteLabel : eVyWalletType.WhiteLabelUnrecoverable
        };

        ViewManager.Loader.Show("Creating Wallet...");
        Venly.WalletAPI.Server.CreateWallet(createParams)
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
