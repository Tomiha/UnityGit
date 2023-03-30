using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;
using VenlySDK.Models.Wallet;

public class ApiExplorer_ViewWalletsVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_ViewWalletsVC() :
        base(eApiExplorerViewId.WalletApi_ViewWallets) { }

    private VyControl_WalletListView _walletListView;
    private List<VyWalletDto> _walletList = null;

    protected override void OnBindElements(VisualElement root)
    {
        _walletListView = GetElement<VyControl_WalletListView>(null);
    }

    protected override void OnActivate()
    {
        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = true;

        _walletListView.OnItemSelected += onClick_Wallet;

        //Check for Cached Wallets
        if (ViewManager.HasGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached))
            _walletList = ViewManager.GetGlobalBlackBoardData<VyWalletDto[]>(ApiExplorer_GlobalKeys.DATA_AllWalletsCached).ToList();

        //Refresh Wallets (Fresh or Cache)
        RefreshWalletList();
    }

    protected override void OnDeactivate()
    {
        
    }

    protected override void OnClick_Refresh()
    {
        RefreshWalletList(true); //Force fresh reload
    }

    private void RefreshWalletList(bool forceFreshLoad = false)
    {
        if (forceFreshLoad || _walletList == null)
        {
            ViewManager.Loader.Show("Retrieving Wallets...");
            Venly.WalletAPI.Client.GetWallets()
                .OnSuccess(wallets =>
                {
                    ViewManager.Loader.SetLoaderText("Populating List...");
                    _walletList = wallets.ToList();
                    _walletListView.SetItemSource(wallets);

                    //Store to cache
                    ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached, wallets);
                })
                .OnFail(ViewManager.HandleException)
                .Finally(ViewManager.Loader.Hide);
        }
        else
        {
            ViewManager.Loader.Show("Populating List...");
            _walletListView.SetItemSource(_walletList);
            ViewManager.Loader.Hide();
        }
    }

    private void onClick_Wallet(VyWalletDto wallet)
    {
        if (IsSelectionMode)
        {
            FinishSelection(wallet);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.DATAKEY_WALLET, wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails);
    }
}
