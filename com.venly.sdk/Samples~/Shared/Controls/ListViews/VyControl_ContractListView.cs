using UnityEngine.UIElements;
using VenlySDK.Models;
using VenlySDK.Utils;

//ITEM
public class VyControl_ContractListItem : VyControl_ListViewItemBase<VyContractDto>
{
    public VyControl_ContractListItem() : base()
    {
        var lbl = new Label();
        lbl.name = "lbl-contract-name";
        Add(lbl);
    }

    public override void BindItem(VyContractDto sourceItem)
    {
        SetLabel("lbl-contract-name", sourceItem.Name);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-contract-name", "MOCK Contract");
    }
}

//LIST VIEW
public class VyControl_ContractListView : VyControl_ListViewBase<VyContractDto, VyControl_ContractListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_ContractListView, UxmlTraits> { }
}
