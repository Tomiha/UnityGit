using System;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_CreateOfferVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_SOURCEWALLET = "source-wallet";
    public const string DATAKEY_SELLER = "seller";
    public const string DATAKEY_MULTITOKEN = "multi-token";

    //DATA
    private VyWalletDto _sourceWallet;
    private VyUserProfileDto _seller;
    private VyMultiTokenDto _token;
    private float _price;
    private float _minimumBid;
    private DateTime _closingDate;
    private eVyOfferType _offerType;

    //UI Elements
    [UIBind("txt-pincode")] private TextField _txtPincode;
    [UIBind("selector-visibility")] private DropdownField _selectorVisibility;
    [UIBind("selector-type")] private DropdownField _selectorType;
    [UIBind("selector-currency")] private DropdownField _selectorCurrency;
    [UIBind("btn-select-seller")] private Button _btnSelectSeller;
    [UIBind("btn-select-source-wallet")] private Button _btnSelectSourceWallet;
    [UIBind("btn-select-token")] private Button _btnSelectToken;

    public ApiExplorer_CreateOfferVC() : 
        base(eApiExplorerViewId.MarketApi_CreateOffer) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", OnClick_CreateOffer);
        BindButton("btn-select-source-wallet", OnSelect_Wallet);
        BindButton("btn-select-seller", OnSelect_Seller);
        BindButton("btn-select-token", OnSelect_Token);
    }

    protected override void OnActivate()
    {
        ShowNavigateHome = false;
        ShowRefresh = false;

        //Set Values
        _selectorVisibility.FromEnum(eVyVisibilityType.Public);
        _selectorCurrency.FromEnum(eVyCurrencyType.Usdc);
        _selectorType.FromEnum(eVyOfferType.Sale);
        _selectorType.OnEnumChanged<eVyOfferType>(updateOfferType);

        //Retrieve Blackboard Data
        TryGetBlackboardData(out _seller, DATAKEY_SELLER, ApiExplorer_GlobalKeys.DATA_UserMarketProfile);
        _btnSelectSeller.ToggleDisplay(_seller == null);

        TryGetBlackboardData(out _sourceWallet, DATAKEY_SOURCEWALLET, ApiExplorer_GlobalKeys.DATA_UserWallet);
        _btnSelectSourceWallet.ToggleDisplay(_sourceWallet == null);

        TryGetBlackboardData(out _token, localKey: DATAKEY_MULTITOKEN);
    }

    protected override void OnRefreshUI()
    {
        updateOfferType(_selectorType.GetValue<eVyOfferType>());
        SetLabel("txt-closing-date", DateTime.Now.AddMonths(3).ToString("g"));

        updateSeller(_seller);
        updateSourceWallet(_sourceWallet);
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();

        _seller = null;
        _sourceWallet = null;
        _token = null;
    }

    private void updateOfferType(eVyOfferType type)
    {
        _offerType = type;

        ToggleElement("details-sale", _offerType == eVyOfferType.Sale);
        ToggleElement("details-auction", _offerType == eVyOfferType.Auction);
    }

    private void updateSeller(VyUserProfileDto seller)
    {
        _seller = seller;

        if (_seller == null) SetLabel("lbl-seller", "select seller");
        else SetLabel("lbl-seller", seller.Nickname);
    }

    private void updateSourceWallet(VyWalletDto sourceWallet)
    {
        if (_sourceWallet != sourceWallet) //new wallet, reset token
        {
            _token = null;
        }

        _sourceWallet = sourceWallet;

        if (_sourceWallet == null)
        {
            SetLabel("lbl-source-wallet", "select wallet");
            SetLabel("lbl-token", "select wallet");
        }
        else
        {
            SetLabel("lbl-source-wallet", _sourceWallet.Id);
            updateToken(_token);
        }

        //Hide Select Token button if no wallet is selected yet
        _btnSelectToken.ToggleDisplay(_sourceWallet != null);
    }

    private void updateToken(VyMultiTokenDto token)
    {
        _token = token;

        if (_token == null) SetLabel("lbl-token", "select token");
        else SetLabel("lbl-token", token.Name);
    }
    #endregion

    #region EVENTS
    private void OnSelect_Wallet()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnSuccess(wallet =>
            {
                _sourceWallet = wallet as VyWalletDto;
                updateSourceWallet(_sourceWallet);
            });
    }

    private void OnSelect_Seller()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.MarketApi_ViewSubUsers, "Select Seller")
            .OnSuccess(user =>
            {
                _seller = user as VyUserProfileDto;
                updateSeller(_seller);
            });
    }

    private void OnSelect_Token()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, "sourceWallet", _sourceWallet);
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewMultiTokens, "Select Token")
            .OnSuccess(token =>
            {
                _token = token as VyMultiTokenDto;
                updateToken(_token);
            });
    }

    private bool Validate()
    {
        try
        {
            if (_seller == null) throw new ArgumentException("No seller selected");
            if (_sourceWallet == null) throw new ArgumentException("No wallet selected");
            if (_token == null) throw new ArgumentException("No token selected");
            if (string.IsNullOrEmpty(_txtPincode.text)) throw new ArgumentException("Pincode invalid");

            _closingDate = DateTime.Parse(GetValue("txt-closing-date"));

            if (_offerType == eVyOfferType.Sale)
            {
                _price = GetValue<float>("txt-price");
            }
            else if (_offerType == eVyOfferType.Auction)
            {
                _minimumBid = GetValue<float>("txt-minimum-bid");
            }
        }
        catch (Exception ex)
        {
            ViewManager.HandleException(ex);
            return false;
        }

        return true;
    }

    private async void OnClick_CreateOffer()
    {
        if (!Validate()) return;

        VyTaskResult<VyOfferDto> result;
        ViewManager.Loader.Show("Creating Offer...");
        if (_offerType == eVyOfferType.Sale) result = await CreateSaleOffer();
        else if (_offerType == eVyOfferType.Auction) result = await CreateAuctionOffer();
        else
        {
            ViewManager.Loader.Hide();
            ViewManager.Exception.Show($"Unknown Offer Type \'{_offerType.GetMemberName()}\'");
            return;
        }

        ViewManager.Loader.Hide();

        if (result.Success)
        {
            ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_OfferDetails, ApiExplorer_OfferDetailsVC.DATAKEY_OFFER, result.Data);
            ViewManager.SwitchView(eApiExplorerViewId.MarketApi_OfferDetails, new SwitchArgs<eApiExplorerViewId>
            {
                BackTargetArgs = new BackTargetArgs<eApiExplorerViewId>(this.BackTargetArgs.Target)
            });
        }
        else
        {
            ViewManager.HandleException(result.Exception);
        }
    }

    private VyTask<VyOfferDto> CreateSaleOffer()
    {
        var request = new VySaleOfferRequestExt
        {
            Pincode = _txtPincode.text,
            WalletId = _sourceWallet.Id,
            OfferRequest = new VySaleOfferRequest
            {
                SellerId = _seller.Id,
                SellerAddress = _sourceWallet.Address,
                Currency = _selectorCurrency.GetValue<eVyCurrencyType>(),
                Visibility = _selectorVisibility.GetValue<eVyVisibilityType>(),
                Price = _price,
                EndDate = _closingDate,
                Nft = new VyNftDto()
                {
                    TokenId = _token.Id,
                    Chain = _sourceWallet.Chain,
                    Address = _token.Contract.Address
                }
            }
        };

        
        return VenlyAPI.Market.CreateOfferExt(request);
    }

    private VyTask<VyOfferDto> CreateAuctionOffer()
    {
        var request = new VyAuctionOfferRequestExt()
        {
            Pincode = _txtPincode.text,
            WalletId = _sourceWallet.Id,
            OfferRequest = new VyAuctionOfferRequest()
            {
                SellerId = _seller.Id,
                SellerAddress = _sourceWallet.Address,
                Currency = _selectorCurrency.GetValue<eVyCurrencyType>(),
                Visibility = _selectorVisibility.GetValue<eVyVisibilityType>(),
                MinimumBid = _minimumBid,
                EndDate = _closingDate,
                Nft = new VyNftDto()
                {
                    TokenId = _token.Id,
                    Chain = _sourceWallet.Chain,
                    Address = _token.Contract.Address
                }
            }
        };

        return VenlyAPI.Market.CreateOfferExt(request);
    }
    #endregion
}
