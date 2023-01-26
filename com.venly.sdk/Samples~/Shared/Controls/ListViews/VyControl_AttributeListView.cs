using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using VenlySDK.Models;
using VenlySDK.Utils;

//ITEM
public class VyControl_AttributeListItem : VyControl_ListViewItemBase<VyTokenAttribute>
{
    public VyControl_AttributeListItem() : base()
    {
        var lbl = new Label();
        lbl.name = "lbl-item";
        Add(lbl);
    }

    public override void BindItem(VyTokenAttribute sourceItem)
    {
        SetLabel("lbl-item", $"Name: {sourceItem.Name}\nType: {sourceItem.Type}\nValue:{sourceItem.Value}");
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-item", "Name: someName\nType: someType\nValue: someValue");
    }
}

//LIST VIEW
public class VyControl_AttributeListView : VyControl_ListViewBase<VyTokenAttribute, VyControl_AttributeListItem>
{
    public VyControl_AttributeListView() : base(false) { }
    public new class UxmlFactory : UxmlFactory<VyControl_AttributeListView, UxmlTraits> { }
}
