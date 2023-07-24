using UnityEngine.UIElements;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Utils;

//ITEM
public class VyControl_OfferListItem : VyControl_ListViewItemBase<VyOfferDto>
{
    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-seller", "Seller");
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-type", "Type");
        AddLabel(root, "lbl-state", "Status");
        AddLabel(root, "lbl-name", "Name");
        AddLabel(root, "lbl-chain", "Chain");
        AddLabel(root, "lbl-price", "Price");
    }

    public override void BindItem(VyOfferDto sourceItem)
    {
        SetLabel("lbl-seller", sourceItem.SellerNickname);
        SetLabel("lbl-id", sourceItem.Id);
        SetLabel("lbl-type", sourceItem.Type.GetMemberName());
        SetLabel("lbl-state", sourceItem.Status.GetMemberName());
        SetLabel("lbl-name", sourceItem.Nft.Name);
        SetLabel("lbl-chain", sourceItem.Nft.Chain.GetMemberName());

        if (sourceItem.Type == eVyOfferType.Sale)
        {
            SetLabel("lbl-price", $"{sourceItem.Price} {sourceItem.Currency.GetMemberName()}");
            SetLabelName("lbl-price", "Price");
        }
        else if (sourceItem.Type == eVyOfferType.Auction)
        {
            SetLabel("lbl-price", $"{sourceItem.HighestBid?.Amount??sourceItem.MinimumBid} {sourceItem.Currency.GetMemberName()}");
            SetLabelName("lbl-price", "Bid");
        }
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-seller", "Seller");
        SetLabel("lbl-id", "Id");
        SetLabel("lbl-type", "Type");
        SetLabel("lbl-state", "Status");
        SetLabel("lbl-name", "Name");
        SetLabel("lbl-price", "Price");
        SetLabel("lbl-chain", "Chain");
    }
}

//LIST VIEW
public class VyControl_OfferListView : VyControl_ListViewBase<VyOfferDto, VyControl_OfferListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_OfferListView, UxmlTraits> { }
}
