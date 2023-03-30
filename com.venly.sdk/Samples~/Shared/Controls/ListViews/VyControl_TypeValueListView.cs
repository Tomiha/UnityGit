using UnityEngine.UIElements;
using VenlySDK.Models.Shared;

//ITEM
public class VyControl_TypeValueListItem : VyControl_ListViewItemBase<VyTypeValuePair>
{
    public VyControl_TypeValueListItem() : base("VyControl_TypeValueListItem") { }

    public override void BindItem(VyTypeValuePair sourceItem)
    {
        SetLabel("lbl-type", sourceItem.Type);
        SetLabel("lbl-value", sourceItem.Value);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-type", "MockType");
        SetLabel("lbl-value", "MockValue");
    }
}

//LIST VIEW
public class VyControl_TypeValueListView : VyControl_ListViewBase<VyTypeValuePair, VyControl_TypeValueListItem>
{
    public VyControl_TypeValueListView():base(false){}
    public new class UxmlFactory : UxmlFactory<VyControl_TypeValueListView, UxmlTraits> { }
}