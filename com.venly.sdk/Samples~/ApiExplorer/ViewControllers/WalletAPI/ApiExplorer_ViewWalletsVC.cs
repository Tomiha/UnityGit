using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Wallet;

public class ApiExplorer_ViewWalletsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA
    private List<VyWalletDto> _walletList = null;

    //UI
    [UIBind("lst-wallets")] private VyControl_WalletListView _lstWallets;

    public ApiExplorer_ViewWalletsVC() :
        base(eApiExplorerViewId.WalletApi_ViewWallets) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _walletList = null;
        _lstWallets.OnItemSelected += OnClick_Wallet;

        //Check for Cached Wallets
        if (TryGetBlackboardData(out VyWalletDto[] resultArr, globalKey: ApiExplorer_GlobalKeys.DATA_AllWalletsCached))
        {
            _walletList = resultArr.ToList();
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Wallets...");
        var query = VyQuery_GetWallets.Create().IncludeBalance(false);
        var result = await VenlyAPI.Wallet.GetWallets(query);
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            _walletList = result.Data.ToList();

            //Store to global
            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached, result.Data);
        }
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_walletList, "walletList")) return;

        _lstWallets.SetItemSource(_walletList);
    }
    #endregion

    #region EVENTS
    private void OnClick_Wallet(VyWalletDto wallet)
    {
        if (IsSelectionMode)
        {
            FinishSelection(wallet);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.DATAKEY_WALLET, wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails);
    }
    #endregion
}
