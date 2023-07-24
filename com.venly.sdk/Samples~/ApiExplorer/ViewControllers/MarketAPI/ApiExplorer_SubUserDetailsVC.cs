using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Utils;

public class ApiExplorer_SubUserDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_USERPROFILE = "user-profile";
    public const string DATAKEY_USER_ID = "user-id";

    //DATA
    private string _userId;
    private VyUserProfileDto _userProfile;
    private VyOfferDto[] _offers;
    private VyFulfillmentDto[] _fulfillments;
    private VyDepositAddressDto[] _depositAddresses;
    private VyUserCreditBalanceDto _creditBalance;

    //UI
    [UIBind("fld-deposit-addresses")] private Foldout _fldDepositAddresses;
    [UIBind("lst-deposit-addresses")] private VyControl_DepositAddressListView _lstDepositAddresses;

    public ApiExplorer_SubUserDetailsVC() : 
        base(eApiExplorerViewId.MarketApi_SubUserDetails) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        //Bind Buttons
        BindButton("btn-view-offers", OnClick_ViewOffers);
        BindButton("btn-view-fulfillments", OnClick_ViewFulfillments);
        BindButton("btn-create-offer", OnClick_CreateOffer);
        BindButton("btn-create-deposit-address", OnClick_CreateDepositAddress);
        BindButton("btn-view-market-offers", OnClick_ViewMarketOffers);
    }

    protected override void OnActivate()
    {
        _userProfile = null;
        _userId = null;

        if (TryGetBlackboardData(out _userProfile, DATAKEY_USERPROFILE, ApiExplorer_GlobalKeys.DATA_UserMarketProfile))
        {
            _userId = _userProfile.Id;
        }
        else if (!TryGetBlackboardData(out _userId, localKey:DATAKEY_USER_ID))
        {
            throw new Exception($"SubUserDetailsVC >> BlackboardData \'{DATAKEY_USERPROFILE}\' or \'{DATAKEY_USER_ID}\' not found...");
        }
    }

    protected override async Task OnRefreshData()
    {
        //Retrieve Offers
        ViewManager.Loader.Show("Retrieving User Profile...");
        var query = VyQuery_GetUsers.Create().Id(new[] { _userId });
        var userResult = await VenlyAPI.Market.GetUsers(query);
        ViewManager.Loader.Hide();

        if (!userResult.Success)
        {
            ViewManager.Exception.Show(userResult.Exception);
            return;
        }

        _userProfile = userResult.Data.FirstOrDefault();

        //Retrieve Offers
        ViewManager.Loader.Show("Retrieving Sub-User Offers...");
        var offersQuery = VyQuery_GetOffers
            .Create()
            .SellerId(_userProfile.Id)
            .Status(new[]
            {
                eVyOfferState.New.GetMemberName(),
                eVyOfferState.Ready.GetMemberName(),
                eVyOfferState.Error.GetMemberName(),
                eVyOfferState.Closed.GetMemberName(),
                eVyOfferState.InitiatingOffer.GetMemberName(),
                eVyOfferState.Refused.GetMemberName(),
                eVyOfferState.Terminated.GetMemberName(),
                eVyOfferState.WaitingForAssociation.GetMemberName(),
            });

        var offerResult = await VenlyAPI.Market.GetOffers(offersQuery);
        ViewManager.Loader.Hide();

        if (!offerResult.Success)
        {
            ViewManager.Exception.Show(offerResult.Exception);
            return;
        }

        _offers = offerResult.Data;

        //Retrieve Fulfillments
        ViewManager.Loader.Show("Retrieving Sub-User Fulfillments...");
        var fulfillmentQuery = VyQuery_GetFulfillments
            .Create()
            .BuyerId(_userProfile.Id);
        var fulfillmentResult = await VenlyAPI.Market.GetFulfillments(fulfillmentQuery);
        ViewManager.Loader.Hide();

        if (!fulfillmentResult.Success)
        {
            ViewManager.Exception.Show(fulfillmentResult.Exception);
            return;
        }

        _fulfillments = fulfillmentResult.Data;

        //Retrieve Fulfillments
        ViewManager.Loader.Show("Retrieving Deposit Adresses...");
        var addrResult = await VenlyAPI.Market.GetDepositAddresses(_userProfile.Id);
        ViewManager.Loader.Hide();

        if (!addrResult.Success)
        {
            ViewManager.Exception.Show(addrResult.Exception);
            return;
        }

        _depositAddresses = addrResult.Data;


        //Retrieve Balance
        ViewManager.Loader.Show("Retrieving Credit Balance...");
        var creditQuery = VyQuery_GetCreditBalance.Create()
            .Currencies(new[] { eVyCurrencyType.Usdc.GetMemberName() });
        var balanceResult = await VenlyAPI.Market.GetCreditBalance(_userProfile.Id, creditQuery);
        ViewManager.Loader.Hide();

        if (!balanceResult.Success)
        {
            ViewManager.Exception.Show(balanceResult.Exception);
            return;
        }

        _creditBalance = balanceResult.Data[0];
    }

    protected override void OnRefreshUI()
    {
        //Update Sub-User UI
        SetLabel("lbl-nickname", _userProfile.Nickname);
        SetLabel("lbl-user-id", _userProfile.Id);
        SetLabel("lbl-verification", _userProfile.VerificationStatus.GetMemberName());
        SetLabel("lbl-balance", $"{_creditBalance.Balance} {_creditBalance.Currency.GetMemberName()}");

        _fldDepositAddresses.value = false;
        _fldDepositAddresses.text = $"{_depositAddresses.Length} Deposit Addresses";
        _lstDepositAddresses.SetItemSource(_depositAddresses);

        SetLabel("lbl-offers-amount", $"{_offers.Length} offers");
        SetLabel("lbl-fulfillments-amount", $"{_fulfillments.Length} fulfillments");
    }
    #endregion

    #region EVENTS
    private void OnClick_CreateDepositAddress()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_CreateDepositAddress, ApiExplorer_CreateDepositAddressVC.DATAKEY_USERPROFILE, _userProfile);
        ViewManager.SelectionMode(eApiExplorerViewId.MarketApi_CreateDepositAddress)
            .OnSuccess(item =>
            {
                Refresh();
            })
            .OnFail(ViewManager.HandleException);
    }

    private void OnClick_CreateOffer()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_CreateOffer, ApiExplorer_CreateOfferVC.DATAKEY_SELLER, _userProfile);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_CreateOffer);
    }

    private void OnClick_ViewOffers()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_ViewOffers, ApiExplorer_ViewOffersVC.DATAKEY_OFFERS, _offers);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_ViewOffers, new SwitchArgs<eApiExplorerViewId>
        {
            BackTargetArgs = new BackTargetArgs<eApiExplorerViewId>(this){RequiresRefresh = false}
        });
    }

    private void OnClick_ViewFulfillments()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_ViewFulfillments, ApiExplorer_ViewFulfillmentsVC.DATAKEY_FULFILLMENTS, _fulfillments);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_ViewFulfillments, new SwitchArgs<eApiExplorerViewId>
        {
            BackTargetArgs = new BackTargetArgs<eApiExplorerViewId>(this) { RequiresRefresh = false }
        });
    }

    private void OnClick_ViewMarketOffers()
    {
        ViewManager.ClearViewBlackboardData(eApiExplorerViewId.MarketApi_ViewOffers);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_ViewOffers, this, false);
    }
    #endregion
}
