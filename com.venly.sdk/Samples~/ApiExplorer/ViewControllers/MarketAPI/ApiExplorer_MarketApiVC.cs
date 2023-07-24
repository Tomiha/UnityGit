using UnityEngine;

public class ApiExplorer_MarketApiVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_MarketApiVC() : 
        base(eApiExplorerViewId.Main_MarketApi) {}

    protected override void OnActivate()
    {
        ShowRefresh = false;

        BindButton_SwitchView("btn-view-users", eApiExplorerViewId.MarketApi_ViewSubUsers, true);
        BindButton_SwitchView("btn-view-offers", eApiExplorerViewId.MarketApi_ViewOffers, true);
        BindButton_SwitchView("btn-create-sub-user", eApiExplorerViewId.MarketApi_CreateSubUser);
        BindButton_SwitchView("btn-create-offer", eApiExplorerViewId.MarketApi_CreateOffer);

#if ENABLE_VENLY_DEV_MODE
        if (!VenlySettings.HasMarketApiAccess)
        {
            if (!HasBlackboardData("realm-access-shown"))
            {
                SetBlackboardData("realm-access-shown", null);
                ViewManager.Info.Show($"The active credentials \'{VenlySettings.ClientId}\' do not have MARKET API realm access.\n\nSome features of the MARKET API samples will not work due to insufficient access (unauthorized).", new Color(0.9f, 0.4f, 0.05f));
            }
        }
#endif
    }
}
