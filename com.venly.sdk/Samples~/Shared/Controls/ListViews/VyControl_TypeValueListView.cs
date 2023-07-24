using UnityEngine.UIElements;
using Venly.Models.Shared;

//ITEM
public class VyControl_TypeValueListItem : VyControl_ListViewItemBase<VyTypeValueDto>
{
    //public VyControl_TypeValueListItem() : base("VyControl_TypeValueListItem") { }
    public VyControl_TypeValueListItem() : base() { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-type", "Type");
        AddLabel(root, "lbl-value", "Value");
    }

    public override void BindItem(VyTypeValueDto sourceItem)
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
public class VyControl_TypeValueListView : VyControl_ListViewBase<VyTypeValueDto, VyControl_TypeValueListItem>
{
    public VyControl_TypeValueListView():base(false){}
    public new class UxmlFactory : UxmlFactory<VyControl_TypeValueListView, UxmlTraits> { }
}