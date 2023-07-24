using UnityEngine;

public class ApiExplorer_NftApiVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_NftApiVC() : 
        base(eApiExplorerViewId.Main_NftApi) {}

    protected override void OnActivate()
    {
        ShowRefresh = false;

        BindButton_SwitchView("btn-view-contracts", eApiExplorerViewId.NftApi_ViewContracts);
        BindButton_SwitchView("btn-mint-token", eApiExplorerViewId.NftApi_MintToken);

#if ENABLE_VENLY_DEV_MODE
        if (!VenlySettings.HasNftApiAccess)
        {
            if (!HasBlackboardData("realm-access-shown"))
            {
                SetBlackboardData("realm-access-shown", null);
                ViewManager.Info.Show($"The active credentials \'{VenlySettings.ClientId}\' do not have NFT API realm access.\n\nSome features of the NFT API samples will not work due to insufficient access (unauthorized).", new Color(0.9f, 0.4f, 0.05f));
            }
        }
#endif
    }
}
