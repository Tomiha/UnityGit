using UnityEngine.UIElements;
using VenlySDK.Models;

//ITEM
public class VyControl_WalletEventListItem : VyControl_ListViewItemBase<VyWalletEventDto>
{
    public VyControl_WalletEventListItem() : base()
    {
        //Temp
        var label = new Label();
        label.name = "lbl-event-type";
        Add(label);
    }

    public override void BindItem(VyWalletEventDto sourceItem)
    {
        SetLabel("lbl-event-type", sourceItem.EventType);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-event-type", "WALLET-EVENT");
    }
}

//LIST VIEW
public class VyControl_WalletEventListView : VyControl_ListViewBase<VyWalletEventDto, VyControl_WalletEventListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_WalletEventListView, UxmlTraits> { }
}