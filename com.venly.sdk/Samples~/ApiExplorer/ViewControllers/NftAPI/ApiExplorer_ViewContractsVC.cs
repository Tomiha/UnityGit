using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;

public class ApiExplorer_ViewContractsVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_ViewContractsVC() :
        base(eApiExplorerViewId.NftApi_ViewContracts) { }

    private VyControl_ContractListView _contractListView;
    private List<VyContractDto> _contractList = null;

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _contractListView);
    }

    protected override void OnActivate()
    {
        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = true;

        _contractListView.OnItemSelected += onClick_Contract;

        //Check for Cached Wallets
        if (ViewManager.HasGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllContractsCached))
            _contractList = ViewManager.GetGlobalBlackBoardData<List<VyContractDto>>(ApiExplorer_GlobalKeys.DATA_AllContractsCached);

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
        if (forceFreshLoad || _contractList == null)
        {
            ViewManager.Loader.Show("Retrieving Contracts...");
            VenlyAPI.Nft.GetContracts()
                .OnSuccess(contracts =>
                {
                    ViewManager.Loader.SetLoaderText("Populating List...");
                    _contractList = contracts.ToList();

                    //Remove Unconfirmed Contracts?
                    _contractList.RemoveAll(c => !c.Confirmed);

                    _contractListView.SetItemSource(_contractList);

                    //Store to cache
                    ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllContractsCached, _contractList);
                })
                .OnFail(ViewManager.HandleException)
                .Finally(ViewManager.Loader.Hide);
        }
        else
        {
            ViewManager.Loader.Show("Populating List...");
            _contractListView.SetItemSource(_contractList);
            ViewManager.Loader.Hide();
        }
    }

    private void onClick_Contract(VyContractDto contract)
    {
        if (IsSelectionMode)
        {
            FinishSelection(contract);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_ContractDetails, ApiExplorer_ContractDetailsVC.DATAKEY_CONTRACT , contract);
        ViewManager.SwitchView(eApiExplorerViewId.NftApi_ContractDetails);
    }
}
