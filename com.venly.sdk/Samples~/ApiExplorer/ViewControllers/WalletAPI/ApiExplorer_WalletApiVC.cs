using UnityEngine;

public class ApiExplorer_WalletApiVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_WalletApiVC() : 
        base(eApiExplorerViewId.Main_WalletApi) {}

    protected override void OnActivate()
    {
        ShowRefresh = false;

        BindButton_SwitchView("btn-view-wallets", eApiExplorerViewId.WalletApi_ViewWallets);
        BindButton_SwitchView("btn-create-wallet", eApiExplorerViewId.WalletApi_CreateWallet);
        BindButton_SwitchView("btn-transfer-nativetoken", eApiExplorerViewId.WalletApi_TransferNativeToken);
        BindButton_SwitchView("btn-transfer-multitoken", eApiExplorerViewId.WalletApi_TransferMultiToken);
        BindButton_SwitchView("btn-transfer-cryptotoken", eApiExplorerViewId.WalletApi_TransferCryptoToken);

#if ENABLE_VENLY_DEV_MODE
        if (!VenlySettings.HasWalletApiAccess)
        {
            if (!HasBlackboardData("realm-access-shown"))
            {
                SetBlackboardData("realm-access-shown", null);
                ViewManager.Info.Show($"The active credentials \'{VenlySettings.ClientId}\' do not have WALLET API realm access.\n\nSome features of the WALLET API samples will not work due to insufficient access (unauthorized).", new Color(0.9f, 0.4f,0.05f));
            }
        }
#endif
    }
}
