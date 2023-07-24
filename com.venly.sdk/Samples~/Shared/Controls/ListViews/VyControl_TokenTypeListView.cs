using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Nft;

//ITEM
public class VyControl_TokenTypeListItem : VyControl_ListViewItemBase<VyTokenTypeDto>
{
    //public VyControl_TokenTypeListItem() : base("VyControl_TokenTypeListItem") { }
    public VyControl_TokenTypeListItem() : base() { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-name", "Name");
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-fungible", "Fungible");
        AddLabel(root, "lbl-supply", "Supply");
    }

    public override void BindItem(VyTokenTypeDto sourceItem)
    {
        SetLabel("lbl-name", sourceItem.Name);
        SetLabel("lbl-id", sourceItem.Id.ToString());
        SetLabel("lbl-fungible", sourceItem.Fungible);
        SetLabel("lbl-supply", $"{sourceItem.CurrentSupply}/{sourceItem.MaxSupply}");
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-name", "MockTokenType");
        SetLabel("lbl-id", "1234");
        SetLabel("lbl-fungible", "NO");
        SetLabel("lbl-supply", "20/50");
    }
}

//LIST VIEW
public class VyControl_TokenTypeListView : VyControl_ListViewBase<VyTokenTypeDto, VyControl_TokenTypeListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_TokenTypeListView, UxmlTraits> { }
}
