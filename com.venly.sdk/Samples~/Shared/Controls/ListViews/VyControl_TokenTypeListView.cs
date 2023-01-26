using UnityEngine.UIElements;
using VenlySDK.Models;

//ITEM
public class VyControl_TokenTypeListItem : VyControl_ListViewItemBase<VyTokenTypeDto>
{
    public VyControl_TokenTypeListItem() : base()
    {
        var lbl = new Label();
        lbl.name = "lbl-tokentype-name";
        Add(lbl);
    }

    public override void BindItem(VyTokenTypeDto sourceItem)
    {
        SetLabel("lbl-tokentype-name", sourceItem.Name);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-tokentype-name", "MOCK TokenType");
    }
}

//LIST VIEW
public class VyControl_TokenTypeListView : VyControl_ListViewBase<VyTokenTypeDto, VyControl_TokenTypeListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_TokenTypeListView, UxmlTraits> { }
}
