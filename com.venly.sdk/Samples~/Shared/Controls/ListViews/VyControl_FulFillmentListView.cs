using PlayFab.MultiplayerModels;
using UnityEngine.UIElements;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Utils;

//ITEM
public class VyControl_FulFillmentListItem : VyControl_ListViewItemBase<VyFulfillmentDto>
{
    private string _offerId;
    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-type", "Type");
        AddLabelWithButton(root, "lbl-offer-id", "Offer Id", "View", OnClick_ViewOffer);
        AddLabel(root, "lbl-amount", "Amount");
        AddLabel(root, "lbl-status", "Status");
        AddLabel(root, "lbl-creation-date", "Creation");
        AddLabel(root, "lbl-buyer-address", "Buyer Wallet");
    }

    private void OnClick_ViewOffer()
    {
        var mgr = SampleViewManager<eApiExplorerViewId>.Instance;
        mgr.SetViewBlackboardData(eApiExplorerViewId.MarketApi_OfferDetails, ApiExplorer_OfferDetailsVC.DATAKEY_OFFER_ID, _offerId);
        mgr.SwitchView(eApiExplorerViewId.MarketApi_OfferDetails);
    }

    public override void BindItem(VyFulfillmentDto sourceItem)
    {
        _offerId = sourceItem.OfferId;
        SetLabel("lbl-id", sourceItem.Id);
        SetLabel("lbl-type", sourceItem.Type.GetMemberName());
        SetLabel("lbl-offer-id", sourceItem.OfferId);
        SetLabel("lbl-amount", sourceItem.Amount);
        SetLabel("lbl-status", sourceItem.Status.GetMemberName());
        SetLabel("lbl-creation-date", sourceItem.CreationDate.ToString("f"));

        ToggleElement("lbl-buyer-address", sourceItem.Type == eVyFulfillmentType.Purchase);
        if(sourceItem.Type == eVyFulfillmentType.Purchase) SetLabel("lbl-buyer-address", sourceItem.BuyerWalletAddress);

    }

    public override void BindMockItem()
    {
        SetLabel("lbl-id", "Id");
        SetLabel("lbl-type", "Type");
        SetLabel("lbl-offer-id", "Offer Id");
        SetLabel("lbl-amount", "Amount");
        SetLabel("lbl-status", "Status");
        SetLabel("lbl-creation-date", "Creation");
        SetLabel("lbl-buyer-address", "Set if Purchase");
    }
}

//LIST VIEW
public class VyControl_FulFillmentListView : VyControl_ListViewBase<VyFulfillmentDto, VyControl_FulFillmentListItem>
{
    public VyControl_FulFillmentListView():base(false){}
    public new class UxmlFactory : UxmlFactory<VyControl_FulFillmentListView, UxmlTraits> { }
}
