using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using VenlySDK.Core;
using VenlySDK.Models;

public abstract class ApiExplorer_ViewTokensBaseVC<T, TListView, TListItem> : SampleViewBase<eApiExplorerViewId> 
    where TListView : VyControl_ListViewBase<T, TListItem>
    where TListItem : VyControl_ListViewItemBase<T>
{
    protected ApiExplorer_ViewTokensBaseVC(eApiExplorerViewId viewId) :
        base(viewId)
    { }

    protected abstract eApiExplorerViewId DetailViewId { get; }

    protected TListView _tokenListView;
    protected List<T> _tokenList = null;

    protected VyWalletDto _sourceWallet = null;
    protected string _lastWalletId = null;

    protected override void OnBindElements(VisualElement root)
    {
        _tokenListView = GetElement<TListView>();
    }

    protected override void OnActivate()
    {
        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = false; //No Refresh

        _tokenListView.OnItemSelected += onClick_Token;

        if (HasBlackboardData("tokenList"))
        {
            _tokenList = GetBlackBoardData<T[]>("tokenList").ToList();
        }
        
        if (HasBlackboardData("sourceWallet"))
        {
            _sourceWallet = GetBlackBoardData<VyWalletDto>("sourceWallet");
            ShowRefresh = true; //Wallet present, refresh is possible
        }

        RefreshTokens();
    }

    protected override void OnDeactivate()
    {
        _sourceWallet = null;
        
        ClearBlackboardData("sourceWallet");
        ClearBlackboardData("tokenList");
    }

    protected abstract VyTask<T[]> GetTokenBalances(string walletId);

    private void RefreshTokens(bool force = false)
    {
        //Do we have a wallet?
        if (_sourceWallet != null)
        {
            if (force || _sourceWallet.Id != _lastWalletId)
            {
                ViewManager.Loader.Show("Refreshing Tokens...");
                GetTokenBalances(_sourceWallet.Id)
                    .OnSuccess(tokens =>
                    {
                        _lastWalletId = _sourceWallet.Id;
                        _tokenList = tokens.ToList();
                        _tokenListView.SetItemSource(_tokenList);
                    })
                    .OnFail(ViewManager.HandleException)
                    .Finally(ViewManager.Loader.Hide);
            }
        }
        else
        {
            _lastWalletId = null;
            _tokenListView.SetItemSource(_tokenList);
        }
    }

    protected override void OnClick_Refresh()
    {
        RefreshTokens(true);
    }

    private void onClick_Token(T token)
    {
        if (IsSelectionMode)
        {
            FinishSelection(token);
            return;
        }

        ViewManager.SetViewBlackboardData(DetailViewId, "token", token);

        var wallet = GetBlackBoardData<VyWalletDto>("sourceWallet");
        ViewManager.SetViewBlackboardData(DetailViewId, "sourceWallet", wallet);

        ViewManager.SwitchView(DetailViewId);
    }
}