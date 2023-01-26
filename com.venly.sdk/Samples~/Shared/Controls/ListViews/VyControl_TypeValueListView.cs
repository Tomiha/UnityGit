using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using VenlySDK.Models;

//ITEM
public class VyControl_TypeValueListItem : VyControl_ListViewItemBase<VyTypeValuePair>
{
    public VyControl_TypeValueListItem() : base()
    {
        var lbl = new Label();
        lbl.name = "lbl-item";
        Add(lbl);
    }

    public override void BindItem(VyTypeValuePair sourceItem)
    {
        SetLabel("lbl-item", $"Type: {sourceItem.Type}\nValue:{sourceItem.Value}");
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-item", $"Type: someType\nValue: someValue");
    }
}

//LIST VIEW
public class VyControl_TypeValueListView : VyControl_ListViewBase<VyTypeValuePair, VyControl_TypeValueListItem>
{
    public VyControl_TypeValueListView():base(false){}
    public new class UxmlFactory : UxmlFactory<VyControl_TypeValueListView, UxmlTraits> { }
}