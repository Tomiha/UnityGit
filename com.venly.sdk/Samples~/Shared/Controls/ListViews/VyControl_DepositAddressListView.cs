using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Market;
using Venly.Models.Wallet;
using Venly.Utils;

//ITEM
public class VyControl_DepositAddressListItem : VyControl_ListViewItemBase<VyDepositAddressDto>
{
    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-chain", "Chain");
        AddLabel(root, "lbl-address", "Address");
        //AddLabel(root, "lbl-memo", "Memo");
    }

    public override void BindItem(VyDepositAddressDto sourceItem)
    {
        SetLabel("lbl-chain", sourceItem.Chain.GetMemberName());
        SetLabel("lbl-address", sourceItem.Address);
        //SetLabel("lbl-memo", sourceItem.Memo);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-chain", "Chain");
        SetLabel("lbl-address", "Address");
        //SetLabel("lbl-memo", "Memo");
    }
}

//LIST VIEW
public class VyControl_DepositAddressListView : VyControl_ListViewBase<VyDepositAddressDto, VyControl_DepositAddressListItem>
{
    public VyControl_DepositAddressListView():base(false){}
    public new class UxmlFactory : UxmlFactory<VyControl_DepositAddressListView, UxmlTraits> { }
}
