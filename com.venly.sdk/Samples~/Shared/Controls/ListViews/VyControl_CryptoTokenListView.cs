using UnityEngine.UIElements;
using VenlySDK.Models;

//ITEM
public class VyControl_CryptoTokenListItem : VyControl_ListViewItemBase<VyCryptoTokenDto>
{
    public VyControl_CryptoTokenListItem() : base()
    {
        var label = new Label();
        label.name = "lbl-token-details";
        Add(label);
    }

    public override void BindItem(VyCryptoTokenDto sourceItem)
    {
        SetLabel("lbl-token-details", sourceItem.Name);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-token-details", "Token Details");
    }
}

//LIST VIEW
public class VyControl_CryptoTokenListView : VyControl_ListViewBase<VyCryptoTokenDto, VyControl_CryptoTokenListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_CryptoTokenListView, UxmlTraits> { }
}