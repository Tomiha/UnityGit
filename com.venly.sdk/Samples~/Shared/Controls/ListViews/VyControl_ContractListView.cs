using UnityEngine.UIElements;
using Venly.Models.Shared;
using Venly.Utils;

//ITEM
public class VyControl_ContractListItem : VyControl_ListViewItemBase<VyContractDto>
{
    //public VyControl_ContractListItem() : base("VyControl_ContractListItem") { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-name", "Name");
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-chain", "Chain");
        AddLabel(root, "lbl-symbol", "Symbol");
    }

    public override void BindItem(VyContractDto sourceItem)
    {
        SetLabel("lbl-name", sourceItem.Name);
        SetLabel("lbl-id", sourceItem.Id.ToString());
        SetLabel("lbl-chain", sourceItem.Chain.GetMemberName());
        SetLabel("lbl-symbol", sourceItem.Symbol);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-name", "MockContract");
        SetLabel("lbl-id", "123");
        SetLabel("lbl-chain", "MATIC");
        SetLabel("lbl-symbol", "MCT");
    }
}

//LIST VIEW
public class VyControl_ContractListView : VyControl_ListViewBase<VyContractDto, VyControl_ContractListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_ContractListView, UxmlTraits> { }
}
