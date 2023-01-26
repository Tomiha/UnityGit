using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiExplorer_WalletApiVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_WalletApiVC() : 
        base(eApiExplorerViewId.Main_WalletApi) {}

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        BindButton_SwitchView("btn-view-wallets", eApiExplorerViewId.WalletApi_ViewWallets);
        BindButton_SwitchView("btn-create-wallet", eApiExplorerViewId.WalletApi_CreateWallet);
        BindButton_SwitchView("btn-transfer-nativetoken", eApiExplorerViewId.WalletApi_TransferNativeToken);
        BindButton_SwitchView("btn-transfer-multitoken", eApiExplorerViewId.WalletApi_TransferMultiToken);
        BindButton_SwitchView("btn-transfer-cryptotoken", eApiExplorerViewId.WalletApi_TransferCryptoToken);
    }

    protected override void OnDeactivate()
    {
        
    }
}
