using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Models.Shared;

public class ApiExplorer_CreateSubUserVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_CreateSubUserVC() : 
        base(eApiExplorerViewId.MarketApi_CreateSubUser) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", OnClick_CreateUser);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;
    }
    #endregion

    #region EVENTS
    private void OnClick_CreateUser()
    {
        //Validate Input
        if (!ValidateInput("txt-nickname")) return;

        //Execute
        var request = new VyCreateSubUserRequest()
        {
            Nickname = GetValue("txt-nickname"),
            Type = eVyUserType.SubUser
        };

        ViewManager.Loader.Show("Creating Sub-User...");
        VenlyAPI.Market.CreateSubUser(request)
            .OnSuccess(user =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_SubUserDetails, ApiExplorer_SubUserDetailsVC.DATAKEY_USER_ID, user.Id);
                ViewManager.ClearViewBlackboardData(eApiExplorerViewId.MarketApi_SubUserDetails, ApiExplorer_SubUserDetailsVC.DATAKEY_USERPROFILE);
                ViewManager.SwitchView(eApiExplorerViewId.MarketApi_SubUserDetails);
            })
            .OnFail(ViewManager.Exception.Show)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
