using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Shared;

public class ApiExplorer_ViewContractsVC : SampleViewBase<eApiExplorerViewId>
{
    //UI
    [UIBind("lst-contracts")] private VyControl_ContractListView _lstContracts;
    private List<VyContractDto> _contractList = null;

    public ApiExplorer_ViewContractsVC() :
        base(eApiExplorerViewId.NftApi_ViewContracts) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _lstContracts.OnItemSelected += onClick_Contract;

        //Check for Cached Wallets
        if (TryGetBlackboardData(out VyContractDto[] resultArr, globalKey: ApiExplorer_GlobalKeys.DATA_AllContractsCached))
        {
            _contractList = resultArr.ToList();
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Contracts...");
        var result = await VenlyAPI.Nft.GetContracts();
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            _contractList = result.Data.ToList();
            _contractList.RemoveAll(c => !c.Confirmed);

            //Store to cache
            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllContractsCached, result.Data);
        }
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_contractList, "contractList")) return;
        _lstContracts.SetItemSource(_contractList);
    }
    #endregion

    #region EVENTS
    private void onClick_Contract(VyContractDto contract)
    {
        if (IsSelectionMode)
        {
            FinishSelection(contract);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_ContractDetails, ApiExplorer_ContractDetailsVC.DATAKEY_CONTRACT , contract);
        ViewManager.SwitchView(eApiExplorerViewId.NftApi_ContractDetails, ViewId, false);
    }
    #endregion
}
