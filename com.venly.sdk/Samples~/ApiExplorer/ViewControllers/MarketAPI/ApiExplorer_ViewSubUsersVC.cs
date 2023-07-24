using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Market;

public class ApiExplorer_ViewSubUsersVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_ViewSubUsersVC() :
        base(eApiExplorerViewId.MarketApi_ViewSubUsers) { }

    //UI
    [UIBind("lst-subUsers")] private VyControl_SubUserListView _lstUserProfiles;
    private List<VyUserProfileDto> _userProfileList = null;

    #region DATA & UI
    protected override void OnActivate()
    {
        _userProfileList = null;

        _lstUserProfiles.OnItemSelected += onClick_UserProfile;
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Sub-Users...");
        var result = await VenlyAPI.Market.GetUsers();
        ViewManager.Loader.Hide();

        if (result.Success) _userProfileList = result.Data.ToList();
        else
        {
            ViewManager.HandleException(result.Exception);
            return;
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_userProfileList, "UserProfile-list")) return;
        _lstUserProfiles.SetItemSource(_userProfileList);
    }
    #endregion

    #region EVENTS
    private void onClick_UserProfile(VyUserProfileDto user)
    {
        if (IsSelectionMode)
        {
            FinishSelection(user);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_SubUserDetails, ApiExplorer_SubUserDetailsVC.DATAKEY_USERPROFILE, user);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_SubUserDetails);
    }
    #endregion
}
