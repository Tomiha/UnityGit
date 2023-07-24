using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly.Core;
using Venly.Models.Wallet;

public abstract class ApiExplorer_ViewTokensBaseVC<T, TListView, TListItem> : SampleViewBase<eApiExplorerViewId> 
    where TListView : VyControl_ListViewBase<T, TListItem>
    where TListItem : VyControl_ListViewItemBase<T>
{
    protected ApiExplorer_ViewTokensBaseVC(eApiExplorerViewId viewId) :
        base(viewId)
    { }

    //DATA-KEYS
    public const string DATAKEY_TOKENLIST = "tokenList";
    public const string DATAKEY_WALLET = "sourceWallet";

    //DATA
    protected TListView _lstTokens;
    protected List<T> _tokenList = null;
    protected VyWalletDto _sourceWallet = null;

    protected abstract eApiExplorerViewId DetailViewId { get; }
    protected abstract VyTask<T[]> GetTokenBalances(string walletId);

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        _lstTokens = GetElement<TListView>();
    }

    protected override void OnActivate()
    {
        _sourceWallet = null;
        _tokenList = null;

        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = false; //No Refresh

        _lstTokens.OnItemSelected += onClick_Token;

        if (TryGetBlackboardData(out T[] resultArr, localKey:DATAKEY_TOKENLIST))
        {
            _tokenList = resultArr.ToList();
            NoDataRefresh = true;
        } 
        else if (TryGetBlackboardData(out _sourceWallet, DATAKEY_WALLET, ApiExplorer_GlobalKeys.DATA_UserWallet))
        {
            ShowRefresh = true; //Wallet present, refresh is possible
        }
    }

    protected override void OnDeactivate()
    {
        //ClearBlackboardData();
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Refreshing Tokens...");
        var result = await GetTokenBalances(_sourceWallet.Id);
        ViewManager.Loader.Hide();

        if(result.Success) _tokenList = result.Data.ToList();
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_tokenList, "tokenList")) return;

        _lstTokens.SetItemSource(_tokenList);
    }
    #endregion

    #region EVENTS
    private void onClick_Token(T token)
    {
        if (IsSelectionMode)
        {
            FinishSelection(token);
            return;
        }

        ViewManager.SetViewBlackboardData(DetailViewId, ApiExplorer_TokenDetailsDataKeys.DATAKEY_TOKEN, token);

        var wallet = GetBlackBoardData<VyWalletDto>(DATAKEY_WALLET);
        ViewManager.SetViewBlackboardData(DetailViewId, ApiExplorer_TokenDetailsDataKeys.DATAKEY_WALLET, wallet);

        ViewManager.SwitchView(DetailViewId, ViewId, false);
    }
    #endregion
}