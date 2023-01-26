using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiExplorer_NftApiVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_NftApiVC() : 
        base(eApiExplorerViewId.Main_NftApi) {}

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        BindButton_SwitchView("btn-view-contracts", eApiExplorerViewId.NftApi_ViewContracts);
        BindButton_SwitchView("btn-mint-token", eApiExplorerViewId.NftApi_MintToken);
    }
}
