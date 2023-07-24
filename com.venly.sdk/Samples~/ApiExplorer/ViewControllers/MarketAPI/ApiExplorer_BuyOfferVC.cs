using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_BuyOfferVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_OFFER = "offer";
    public const string DATAKEY_USER = "user";
    public const string DATAKEY_WALLET = "wallet";

    //DATA
    private VyOfferDto _offer;
    private VyUserProfileDto _user;
    private VyWalletDto _wallet;


    public ApiExplorer_BuyOfferVC() : 
        base(eApiExplorerViewId.MarketApi_BuyOffer) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-buy", OnClick_Buy);
        BindButton("btn-select-user", OnClick_SelectUser);
        BindButton("btn-select-wallet", OnClick_SelectWallet);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Retrieve Blackboard Data
        TryGetBlackboardData(out _offer, localKey: DATAKEY_OFFER);

        if(TryGetBlackboardData(out _user, DATAKEY_USER, ApiExplorer_GlobalKeys.DATA_UserMarketProfile))
        {
            ToggleElement("btn-select-user", false);
            SetLabel("txt-user", _user.Nickname);
        }

        if (TryGetBlackboardData(out _wallet, DATAKEY_WALLET, ApiExplorer_GlobalKeys.DATA_UserWallet))
        {
            ToggleElement("btn-select-wallet", false);
            SetLabel("txt-target-wallet", _wallet.Address);
            GetElement<TextField>("txt-target-wallet").SetReadOnly(true);
        }

        if (_offer == null)
        {
            ViewManager.Exception.Show($"BuyOffer >> DATA-KEY \'{DATAKEY_OFFER}\' not set!");
        }
    }

    protected override void OnRefreshUI()
    {
        //DATA Validation
        if (!ValidateData(_offer, DATAKEY_OFFER)) return;

        SetLabel("txt-offer-name", _offer.Nft.Name);
        SetLabel("txt-offer-id", _offer.Id);
        SetLabel("txt-offer-chain", _offer.Nft.Chain.GetMemberName());
        SetLabel("txt-price", $"{_offer.Price} {_offer.Currency.GetMemberName()}");
    }

    //reset state
    protected override void OnDeactivate()
    {
        ClearBlackboardData();

        _offer = null;
        _user = null;
        _wallet = null;
    }
    #endregion

    #region EVENTS
    private void OnClick_Buy()
    {
        if (_user == null || _wallet == null)
        {
            ViewManager.Exception.Show("Make sure to select a User and Target-Wallet.");
            return;
        }

        if (_wallet.Chain != _offer.Nft.Chain)
        {
            ViewManager.Exception.Show($"Selected wallet's chain type must be \'{_offer.Nft.Chain.GetMemberName()}\'.\n(wallet chain = {_wallet.Chain.GetMemberName()})");
            return;
        }

        var request = new VyPurchaseOfferRequest()
        {
            OfferId = _offer.Id,
            UserId = _user.Id,
            WalletAddress = GetValue("txt-target-wallet"),
            Amount = 1
        };

        ViewManager.Loader.Show("Buying Offer...");
        VenlyAPI.Market.PurchaseOffer(request)
            .OnSuccess(offer =>
            {
                //Todo: create Purchase Detail page
                ViewManager.Info.Show("Purchase Successful!");
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_SubUserDetails, ApiExplorer_SubUserDetailsVC.DATAKEY_USERPROFILE, _user);
                ViewManager.SwitchView(eApiExplorerViewId.MarketApi_SubUserDetails, false);
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

    private void OnClick_SelectWallet()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnSuccess(wallet =>
            {
                _wallet = wallet as VyWalletDto;
                SetLabel("txt-target-wallet", _wallet.Address);
            });
    }
    #endregion
}
