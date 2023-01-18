using System;
using UnityEngine.UIElements;
using VenlySDK.Models;

public class ApiExplorer_CryptoTokenDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    private VyCryptoToken _token;

    public ApiExplorer_CryptoTokenDetailsVC() : 
        base(eApiExplorerViewId.Shared_CryptoTokenDetails)
    {
    }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        _token = GetBlackBoardData<VyCryptoToken>("token");

        //Set Data
        SetLabel("lbl-token-name", _token.Name);
        SetLabel("lbl-token-symbol", _token.Symbol);
        SetLabel("lbl-token-balance", _token.Balance.ToString());
        SetLabel("lbl-token-decimals", _token.Decimals.ToString());
        SetLabel("lbl-token-transferable", _token.Transferable?"YES":"NO");
        SetLabel("lbl-token-address", _token.TokenAddress);
    }

    protected override void OnDeactivate()
    {
    }

    private void OnClick_Transfer()
    {

    }
}
