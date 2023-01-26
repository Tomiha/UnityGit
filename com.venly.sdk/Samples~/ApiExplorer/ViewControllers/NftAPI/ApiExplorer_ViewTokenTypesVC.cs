using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;

public class ApiExplorer_ViewTokenTypesVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_ViewTokenTypesVC() :
        base(eApiExplorerViewId.NftApi_ViewTokenTypes) { }

    private VyControl_TokenTypeListView _tokenTypeListView;

    private List<VyTokenTypeDto> _tokenTypeList = null;
    private VyContractDto _sourceContract = null;
    private int? _lastContractId = null;

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _tokenTypeListView);
    }

    protected override void OnActivate()
    {
        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = false;

        _tokenTypeListView.OnItemSelected += onClick_TokenType;

        if (HasBlackboardData("tokenTypes"))
        {
            _tokenTypeList = GetBlackBoardData<VyTokenTypeDto[]>("tokenTypes").ToList();
        }
        
        if (HasBlackboardData("sourceContract"))
        {
            _sourceContract = GetBlackBoardData<VyContractDto>("sourceContract");
            ShowRefresh = true;
        }

        //Refresh (Fresh or Cache)
        RefreshTokenTypes();
    }

    protected override void OnClick_Refresh()
    {
        RefreshTokenTypes(true); //Force fresh reload
    }

    private void RefreshTokenTypes(bool force = false)
    {
        //Do we have a source contract?
        if (_sourceContract != null)
        {
            if (force || _sourceContract.Id != _lastContractId)
            {
                ViewManager.Loader.Show("Refreshing TokenTypes...");
                Venly.NftAPI.Client.GetTokenTypes(_sourceContract.Id)
                    .OnSuccess(tokens =>
                    {
                        _lastContractId = _sourceContract.Id;
                        _tokenTypeList = tokens.ToList();
                        _tokenTypeListView.SetItemSource(_tokenTypeList);
                    })
                    .OnFail(ViewManager.HandleException)
                    .Finally(ViewManager.Loader.Hide);

                return;
            }
        }
        else _lastContractId = null;

        _tokenTypeListView.SetItemSource(_tokenTypeList);
    }

    private void onClick_TokenType(VyTokenTypeDto tokenType)
    {
        if (IsSelectionMode)
        {
            FinishSelection(tokenType);
            return;
        }

        //Should come from a Contract Details View
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_TokenTypeDetails, "tokenType" , tokenType);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_TokenTypeDetails, "sourceContract" , _sourceContract);
        ViewManager.SwitchView(eApiExplorerViewId.NftApi_TokenTypeDetails);
    }
}
