using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Market;

public class ApiExplorer_ViewFulfillmentsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEY
    public const string DATAKEY_FULFILLMENTS = "fulfillments";

    //DATA
    private List<VyFulfillmentDto> _fulfillmentList;
    private VyUserProfileDto _userProfile;

    //UI
    [UIBind("lst-fulfillments")] private VyControl_FulFillmentListView _lstFulfillment;

    public ApiExplorer_ViewFulfillmentsVC() :
        base(eApiExplorerViewId.MarketApi_ViewFulfillments) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _fulfillmentList = null;
        _userProfile = null;

        if (TryGetBlackboardData(out _userProfile, globalKey: ApiExplorer_GlobalKeys.DATA_UserMarketProfile))
        {
            ShowRefresh = true;
            NoDataRefresh = false;
        }
        else if (TryGetBlackboardData(out VyFulfillmentDto[] resultArr, localKey:DATAKEY_FULFILLMENTS))
        {
            _fulfillmentList = resultArr.ToList();
            ShowRefresh = false;
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Fulfillments...");
        VyQuery_GetFulfillments query =
            _userProfile == null ? null : VyQuery_GetFulfillments.Create().BuyerId(_userProfile.Id);
        var result = await VenlyAPI.Market.GetFulfillments(query);
        ViewManager.Loader.Hide();

        if (result.Success) _fulfillmentList = result.Data.ToList();
        else
        {
            ViewManager.HandleException(result.Exception);
            return;
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_fulfillmentList, "Fulfillment-list")) return;
        _lstFulfillment.SetItemSource(_fulfillmentList);
    }

    protected override void OnDeactivate()
    {
        //ClearBlackboardData();
        _fulfillmentList = null;
    }
    #endregion
}
