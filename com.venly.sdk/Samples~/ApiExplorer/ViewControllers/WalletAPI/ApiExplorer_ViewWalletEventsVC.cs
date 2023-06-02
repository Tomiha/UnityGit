using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Wallet;

public class ApiExplorer_ViewWalletEventsVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_ViewWalletEventsVC() :
        base(eApiExplorerViewId.WalletApi_ViewWalletEvents) { }

    private VyControl_WalletEventListView _eventListView;
    private List<VyWalletEventDto> _events = null;

    protected override void OnBindElements(VisualElement root)
    {
        _eventListView = GetElement<VyControl_WalletEventListView>(null);
    }

    protected override void OnActivate()
    {
        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = false;

        _eventListView.selectionType = SelectionType.None;

        //Check for Cached Wallets
        _events = GetBlackBoardData<VyWalletEventDto[]>("eventList").ToList();

        //Refresh List
        _eventListView.SetItemSource(_events);
    }

    protected override void OnDeactivate()
    {
        
    }
}
