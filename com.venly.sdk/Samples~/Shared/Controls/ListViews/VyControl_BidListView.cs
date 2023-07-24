using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Market;
using Venly.Models.Wallet;
using Venly.Utils;

//ITEM
public class VyControl_BidListItem : VyControl_ListViewItemBase<VyBidDto>
{
    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-user", "User");
        AddLabel(root, "lbl-amount", "Amount");
        AddLabel(root, "lbl-created", "Created");
    }

    public override void BindItem(VyBidDto sourceItem)
    {
        SetLabel("lbl-user", sourceItem.User.Nickname);
        SetLabel("lbl-amount", sourceItem.Amount);
        SetLabel("lbl-created", sourceItem.CreatedOn.ToString("f"));
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-user", "User");
        SetLabel("lbl-amount", "Amount");
        SetLabel("lbl-created", "Created");
    }
}

//LIST VIEW
public class VyControl_BidListView : VyControl_ListViewBase<VyBidDto, VyControl_BidListItem>
{
    public VyControl_BidListView():base(false){}
    public new class UxmlFactory : UxmlFactory<VyControl_BidListView, UxmlTraits> { }
}
