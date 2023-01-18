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

        BindButton("btn-view-wallets", onClick_ViewWallets);
        BindButton("btn-create-wallet", onClick_CreateWallet);
    }

    protected override void OnDeactivate()
    {
        
    }

    private void onClick_ViewWallets()
    {
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewWallets);
    }

    private void onClick_CreateWallet()
    {
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_CreateWallet);
    }
}
