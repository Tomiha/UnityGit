using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Utils;

public class ApiExplorer_ViewOffersVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEY
    public const string DATAKEY_OFFERS = "offers";

    //DATA
    private List<VyOfferDto> _offerList;
    private VyQuery_GetOffers _offerQuery = null;

    private eVyVisibilityType _visibility = eVyVisibilityType.Public;

    //UI
    [UIBind("lst-offers")] private VyControl_OfferListView _lstOffers;
    [UIBind("selector-type")] private DropdownField _selectorType;
    [UIBind("selector-status")] private DropdownField _selectorStatus;
    [UIBind("selector-visibility")] private DropdownField _selectorVisibility;
    [UIBind("selector-chain")] private DropdownField _selectorChain;
    [UIBind("fld-filter")] private Foldout _fldFilter;

    public ApiExplorer_ViewOffersVC() :
        base(eApiExplorerViewId.MarketApi_ViewOffers) { }

    #region DATA & UI

    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-filter", OnClick_Filter);
    }

    protected override void OnActivate()
    {
        _offerList = null;
        _offerQuery = null;

        _lstOffers.OnItemSelected += onClick_Offer;

        //filter
        _selectorType.FromEnum(eVyOfferType.Sale);
        _selectorStatus.FromEnum(eVyOfferState.Ready);
        _selectorVisibility.FromEnum(eVyVisibilityType.Public);
        _selectorVisibility.choices.Remove(eVyVisibilityType.Unlisted.ToString());
        _selectorChain.FromEnum(eVyChain.Matic);
        _selectorVisibility.OnEnumChanged<eVyVisibilityType>(e =>
        {
            ToggleElement("container-filter-public", e == eVyVisibilityType.Public);
        });

        _fldFilter.value = false;
        ToggleElement("lbl-pagination", false);

        //Check if offer list is given
        if (TryGetBlackboardData(out VyOfferDto[] resultArr, localKey:DATAKEY_OFFERS))
        {
            _offerList = resultArr.ToList();
            ShowRefresh = false;
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        VyTaskResult<VyOfferDto[]> result;
        if (_visibility == eVyVisibilityType.Private)
        {
            //PRIVATE OFFERS
            ViewManager.Loader.Show("Retrieving Offers (Private)...");
            result = await VenlyAPI.Market.GetUserOffers();
            
            //Pagination (ignore)
            ToggleElement("lbl-pagination", false);

            ViewManager.Loader.Hide();
        }
        else
        {
            //PUBLIC OFFERS
            ViewManager.Loader.Show("Retrieving Offers (Public)...");
            result = await VenlyAPI.Market.GetOffers(_offerQuery);
            //Pagination
            ToggleElement("lbl-pagination", result.Pagination != null);
            if (result.Pagination != null)
            {
                SetLabel("lbl-pagination",
                    $"Page {result.Pagination.PageNumber} / {result.Pagination.NumberOfPages} ({result.Pagination.PageSize} items)\n{result.Pagination.NumberOfElements} items total");
            }

            ViewManager.Loader.Hide();
        }

        if (result.Success) _offerList = result.Data.ToList();
        else
        {
            ViewManager.HandleException(result.Exception);
            return;
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_offerList, "Offer-list")) return;
        _lstOffers.SetItemSource(_offerList);
    }

    protected override void OnClick_Refresh()
    {
        if (_visibility != eVyVisibilityType.Public) return;

        //Reset Filter
        SetToggleValue("toggle-seller-id", false);
        SetToggleValue("toggle-type", false);
        SetToggleValue("toggle-status", false);

        SetLabel("txt-page-number", "1");
        SetLabel("txt-page-size", "20");

        //Reset Query
        _offerQuery = null;
    }

    #endregion

    #region EVENTS
    private void onClick_Offer(VyOfferDto offer)
    {
        if (IsSelectionMode)
        {
            FinishSelection(offer);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.MarketApi_OfferDetails, ApiExplorer_OfferDetailsVC.DATAKEY_OFFER, offer);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_OfferDetails);
    }

    private void OnClick_Filter()
    {
        _offerQuery = null;
        _visibility = _selectorVisibility.GetValue<eVyVisibilityType>();

        if (_visibility == eVyVisibilityType.Public)
        {
            _offerQuery ??= VyQuery_GetOffers.Create();
            if (GetToggleValue("toggle-seller-id"))
            {
                if (!ValidateInput("txt-seller-id")) return;
                _offerQuery.SellerId(GetValue("txt-seller-id"));
            }

            if (GetToggleValue("toggle-chain")) 
                _offerQuery.Chain(_selectorChain.GetValue<eVyChain>());

            if (GetToggleValue("toggle-type"))
                _offerQuery.Type(new[] {_selectorType.GetValue<eVyOfferType>().GetMemberName()});
            if (GetToggleValue("toggle-status"))
                _offerQuery.Status(new[] {_selectorStatus.GetValue<eVyOfferState>().GetMemberName()});
            
            if (GetToggleValue("toggle-page-size"))
            {
                if (!ValidateInput<int>("txt-page-size")) return;
                _offerQuery.PageSize(GetValue<int>("txt-page-size"));
                if (!GetToggleValue("toggle-page-number"))
                    _offerQuery.PageNumber(1);
            }

            if (GetToggleValue("toggle-page-number"))
            {
                if (!ValidateInput<int>("txt-page-number")) return;
                _offerQuery.PageNumber(GetValue<int>("txt-page-number"));
            }
        }

        Refresh();
    }
    #endregion
}
