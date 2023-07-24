using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Utils;

public class ApiExplorer_PlaceBidVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_OFFER = "offer";
    public const string DATAKEY_USER = "user";

    //DATA
    private VyOfferDto _offer;
    private VyUserProfileDto _user;


    public ApiExplorer_PlaceBidVC() : 
        base(eApiExplorerViewId.MarketApi_PlaceBid) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-place-bid", OnClick_PlaceBid);
        BindButton("btn-select-user", OnClick_SelectUser);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;

        //Retrieve Blackboard Data
        TryGetBlackboardData(out _offer, localKey: DATAKEY_OFFER);
        if (TryGetBlackboardData(out _user, DATAKEY_USER, ApiExplorer_GlobalKeys.DATA_UserMarketProfile))
        {
            ToggleElement("btn-select-user", false);
            SetLabel("txt-user", _user.Nickname);
        }

        if (_offer == null)
        {
            ViewManager.Exception.Show($"PlaceBid >> DATA-KEY \'{DATAKEY_OFFER}\' not set!");
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_offer, DATAKEY_OFFER)) return;

        SetLabel("txt-offer-name", _offer.Nft.Name);
        SetLabel("txt-offer-id", _offer.Id);

        if (_offer.HighestBid != null) SetLabel("txt-highest-bid", $"{_offer.HighestBid.Amount} {_offer.Currency.GetMemberName()}");
        else SetLabel("txt-highest-bid", $"{_offer.MinimumBid} {_offer.Currency.GetMemberName()}");
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();

        _offer = null;
        _user = null;
    }
    #endregion

    #region EVENTS
    private void OnClick_PlaceBid()
    {
        //Validate
        if (!ValidateData(_user, DATAKEY_USER)) return;
        if (!ValidateInput<double>("txt-amount")) return;

        var request = new VyBidRequest
        {
            Amount = GetValue<double>("txt-amount"),
            OfferId = _offer.Id,
            UserId = _user.Id
        };

        ViewManager.Loader.Show("Placing Bid...");
        VenlyAPI.Market.BidOnOffer(_offer.Id, request)
            .OnSuccess(offer =>
            {
                ViewManager.Info.Show("Bid Successful!");
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_OfferDetails, ApiExplorer_OfferDetailsVC.DATAKEY_OFFER, offer);
                var backTarget = new BackTargetArgs<eApiExplorerViewId>(ViewManager.GetView(eApiExplorerViewId.MarketApi_SubUserDetails));
                backTarget.SetValue(ApiExplorer_SubUserDetailsVC.DATAKEY_USERPROFILE, _user);
                ViewManager.SwitchView(eApiExplorerViewId.MarketApi_OfferDetails, new SwitchArgs<eApiExplorerViewId>
                {
                    BackTargetArgs = backTarget
                });
            })
            .OnFail(ViewManager.Exception.Show)
            .Finally(ViewManager.Loader.Hide);
    }

    private void OnClick_SelectUser()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.MarketApi_ViewSubUsers, "Select User")
            .OnSuccess(user =>
            {
                _user = user as VyUserProfileDto;
                SetLabel("txt-user", _user.Nickname);
            });
    }
    #endregion
}
