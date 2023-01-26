using UnityEngine.UIElements;
using VenlySDK.Models;
using VenlySDK.Utils;

//ITEM
public class VyControl_WalletListItem : VyControl_ListViewItemBase<VyWalletDto>
{
    public VyControl_WalletListItem() : base("VyControl_WalletListItem") { }

    public override void BindItem(VyWalletDto sourceItem)
    {
        SetLabel("lbl-wallet-chain", sourceItem.Chain.GetMemberName());
        SetLabel("lbl-wallet-id", sourceItem.Id);
        SetLabel("lbl-wallet-address", sourceItem.Address);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-wallet-chain", "MATIC");
        SetLabel("lbl-wallet-id", "152eb3ac-999b-4eb7-b564-de0ae1faa60d");
        SetLabel("lbl-wallet-address", "0x76d8a24d0Df9C2a0c0F9b831B0534b3b5811ac4b");
    }
}

//LIST VIEW
public class VyControl_WalletListView : VyControl_ListViewBase<VyWalletDto, VyControl_WalletListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_WalletListView, UxmlTraits> { }
}
