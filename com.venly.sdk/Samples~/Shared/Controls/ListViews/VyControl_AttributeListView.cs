using UnityEngine.UIElements;
using VenlySDK.Models.Shared;
using VenlySDK.Utils;

//ITEM
public class VyControl_AttributeListItem : VyControl_ListViewItemBase<VyTokenAttributeDto>
{
    public VyControl_AttributeListItem() : base("VyControl_AttributeListItem") { }

    public override void BindItem(VyTokenAttributeDto sourceItem)
    {
        SetLabel("lbl-type", sourceItem.Type.GetMemberName());
        SetLabel("lbl-name", sourceItem.Name);
        SetLabel("lbl-value", sourceItem.Value.ToString());
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-type", "MockType");
        SetLabel("lbl-name", "MockName");
        SetLabel("lbl-value", "MockValue");
    }
}

//LIST VIEW
public class VyControl_AttributeListView : VyControl_ListViewBase<VyTokenAttributeDto, VyControl_AttributeListItem>
{
    public VyControl_AttributeListView() : base(false) { }
    public new class UxmlFactory : UxmlFactory<VyControl_AttributeListView, UxmlTraits> { }
}
