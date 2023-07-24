using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Utils;

public class ApiExplorer_OfferDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_OFFER = "offer";
    public const string DATAKEY_OFFER_ID = "offer-id";

    //DATA
    private string _offerId;
    private VyOfferDto _offer;

    //UI
    [UIBind("img-token")] private VisualElement _imgToken;
    [UIBind("details-sale")] private VisualElement _detailsSale;
    [UIBind("details-auction")] private VisualElement _detailsAuction;
    [UIBind("actions-sale")] private VisualElement _actionsSale;
    [UIBind("actions-auction")] private VisualElement _actionsAuction;
    [UIBind("lst-bids")] private VyControl_BidListView _lstBids;
    [UIBind("fld-bids")] private Foldout _fldBids;


    public ApiExplorer_OfferDetailsVC() : 
        base(eApiExplorerViewId.MarketApi_OfferDetails) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-cancel", OnClick_CancelOffer);
        BindButton("btn-bid", OnClick_PlaceBid);
        BindButton("btn-buy", OnClick_BuyOffer);
    }

    protected override void OnActivate()
    {
        _offer = null;
        _offerId = null;

        if (TryGetBlackboardData(out _offer, localKey:DATAKEY_OFFER))
        {
            _offerId = _offer.Id;
        }
        else if (!TryGetBlackboardData(out _offerId, localKey:DATAKEY_OFFER_ID))
        {
            throw new Exception($"OfferDetailsVC >> BlackboardData \'{DATAKEY_OFFER}\' or \'{DATAKEY_OFFER_ID}\' not found...");
        }
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Offer...");
        var offerResult = await VenlyAPI.Market.GetOffer(_offerId);
        ViewManager.Loader.Hide();

        if (!offerResult.Success)
        {
            ViewManager.Exception.Show(offerResult.Exception);
            return;
        }

        _offer = offerResult.Data;
        _offerId = _offer.Id;
    }

    protected override void OnRefreshUI()
    {
        _fldBids.value = false;

        _detailsAuction.ToggleDisplay(_offer.Type == eVyOfferType.Auction);
        _actionsAuction.ToggleDisplay(_offer.Type == eVyOfferType.Auction && _offer.Status == eVyOfferState.Ready);
        _detailsSale.ToggleDisplay(_offer.Type == eVyOfferType.Sale);
        _actionsSale.ToggleDisplay(_offer.Type == eVyOfferType.Sale && _offer.Status == eVyOfferState.Ready);
        ToggleElement("btn-cancel", _offer.Status == eVyOfferState.Ready);

        //Update Offer UI
        SetLabel("lbl-name", _offer.Nft.Name);
        SetLabel("lbl-collection", _offer.Nft.Contract.Name);
        SetLabel("lbl-seller", _offer.SellerNickname);
        SetLabel("lbl-type", _offer.Type.GetMemberName());
        SetLabel("lbl-status", _offer.Status.GetMemberName());
        SetLabel("lbl-remaining", _offer.RemainingAmount);
        SetLabel("lbl-closing-time", _offer.EndDate.ToString("f"));

        if (_offer.Type == eVyOfferType.Sale)
        {
            SetLabel("lbl-price", $"{_offer.Price} {_offer.Currency.GetMemberName()}");
        }
        else if (_offer.Type == eVyOfferType.Auction)
        {
            SetLabel("lbl-min-bid", _offer.MinimumBid);
            if (_offer.HighestBid == null) SetLabel("lbl-highest-bid", "-");
            else SetLabel("lbl-highest-bid", $"{_offer.HighestBid.Amount} {_offer.Currency.GetMemberName()}");
            _fldBids.text = $"Bids ({_offer.Bids?.Length})";
            _lstBids.SetItemSource(_offer.Bids);
        }

        //image (try downloading)
        VenlyUnityUtils.DownloadImage(_offer.Nft.ImageUrl)
            .OnSuccess(tex =>
            {
                _imgToken.style.backgroundImage = new StyleBackground(tex);
            })
            .OnFail(ex => VenlyLog.Exception(ex));
    }
    #endregion

    #region EVENTS
    private void OnClick_BuyOffer()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_BuyOffer, ApiExplorer_PlaceBidVC.DATAKEY_OFFER, _offer);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_BuyOffer);
    }

    private void OnClick_PlaceBid()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_PlaceBid, ApiExplorer_PlaceBidVC.DATAKEY_OFFER, _offer);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_PlaceBid);
    }

    private void OnClick_CancelOffer()
    {
        var request = new VyCancelOfferRequest()
        {
            OfferId = _offer.Id,
            UserId = _offer.SellerId
        };

        ViewManager.Loader.Show("Cancelling Offer...");
        VenlyAPI.Market.CancelOffer(request)
            .OnSuccess(data => Refresh())
            .OnFail(ViewManager.Exception.Show)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
