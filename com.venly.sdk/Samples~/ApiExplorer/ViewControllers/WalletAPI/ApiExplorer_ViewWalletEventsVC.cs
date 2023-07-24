using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Venly.Models.Wallet;

public class ApiExplorer_ViewWalletEventsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_EVENTS = "eventList";

    //DATA
    private List<VyWalletEventDto> _events = null;

    //UI
    [UIBind("lst-events")] private VyControl_WalletEventListView _eventListView;

    public ApiExplorer_ViewWalletEventsVC() :
        base(eApiExplorerViewId.WalletApi_ViewWalletEvents) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        ShowRefresh = false;
        _eventListView.selectionType = SelectionType.None;

        if (TryGetBlackboardData(out VyWalletEventDto[] resultArr, localKey: DATAKEY_EVENTS))
        {
            _events = resultArr.ToList();
        }
    }

    protected override void OnRefreshUI()
    {
        _eventListView.SetItemSource(_events);
    }
    #endregion
}
