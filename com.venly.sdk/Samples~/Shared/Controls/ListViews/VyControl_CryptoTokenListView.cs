using System.Globalization;
using UnityEngine.UIElements;
using Venly.Models.Shared;

//ITEM
public class VyControl_CryptoTokenListItem : VyControl_ListViewItemBase<VyCryptoTokenDto>
{
    public VyControl_CryptoTokenListItem() : base("VyControl_CryptoTokenListItem") { }

    public override void BindItem(VyCryptoTokenDto sourceItem)
    {
        SetLabel("lbl-name", sourceItem.Name);
        SetLabel("lbl-symbol", sourceItem.Symbol);
        SetLabel("lbl-balance", sourceItem.Balance.ToString(CultureInfo.InvariantCulture));
        SetLabel("lbl-type", sourceItem.Type);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-name", "MockToken");
        SetLabel("lbl-symbol", "MTK");
        SetLabel("lbl-balance", "2.5454");
        SetLabel("lbl-type", "ERC 20");
    }
}

//LIST VIEW
public class VyControl_CryptoTokenListView : VyControl_ListViewBase<VyCryptoTokenDto, VyControl_CryptoTokenListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_CryptoTokenListView, UxmlTraits> { }
}