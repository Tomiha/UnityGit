using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Wallet;
using Venly.Utils;

//ITEM
public class VyControl_WalletListItem : VyControl_ListViewItemBase<VyWalletDto>
{
    //public VyControl_WalletListItem() : base("VyControl_WalletListItem") { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-chain", "Chain");
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-address", "Address");
        AddLabel(root, "lbl-description", "Description");
    }

    public override void BindItem(VyWalletDto sourceItem)
    {
        SetLabel("lbl-chain", sourceItem.Chain.GetMemberName());
        SetLabel("lbl-id", sourceItem.Id);
        SetLabel("lbl-address", sourceItem.Address);
        SetLabel("lbl-description", sourceItem.Description);
        //ToggleElement("lbl-archived", sourceItem.Archived);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-chain", "MATIC");
        SetLabel("lbl-id", "152eb3ac-999b-4eb7-b564-de0ae1faa60d");
        SetLabel("lbl-address", "0x76d8a24d0Df9C2a0c0F9b831B0534b3b5811ac4b");
        SetLabel("lbl-description", "lorem ipsum doloret...");
    }
}

//LIST VIEW
public class VyControl_WalletListView : VyControl_ListViewBase<VyWalletDto, VyControl_WalletListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_WalletListView, UxmlTraits> { }
}
