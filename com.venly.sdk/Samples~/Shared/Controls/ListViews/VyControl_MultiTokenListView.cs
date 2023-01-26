using UnityEngine.UIElements;
using VenlySDK.Models;

//ITEM
public class VyControl_MultiTokenListItem : VyControl_ListViewItemBase<VyMultiTokenDto>
{
    public VyControl_MultiTokenListItem() : base()
    {
        var label = new Label();
        label.name = "lbl-token-details";
        Add(label);
    }

    public override void BindItem(VyMultiTokenDto sourceItem)
    {
        SetLabel("lbl-token-details", sourceItem.Name);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-token-details", "Token Details");
    }
}

//LIST VIEW
public class VyControl_MultiTokenListView : VyControl_ListViewBase<VyMultiTokenDto, VyControl_MultiTokenListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_MultiTokenListView, UxmlTraits> { }
}