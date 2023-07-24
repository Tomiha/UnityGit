using UnityEngine.UIElements;
using Venly.Models.Market;
using Venly.Utils;

//ITEM
public class VyControl_SubUserListItem : VyControl_ListViewItemBase<VyUserProfileDto>
{
    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-nickname", "Nickname");
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-verification", "Verification");
    }

    public override void BindItem(VyUserProfileDto sourceItem)
    {
        SetLabel("lbl-nickname", sourceItem.Nickname);
        SetLabel("lbl-id", sourceItem.Id);
        SetLabel("lbl-verification", sourceItem.VerificationStatus.GetMemberName());
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-nickname", "Mock SubUser");
        SetLabel("lbl-id", "152eb3ac-999b-4eb7-b564-de0ae1faa60d");
        SetLabel("lbl-verification", "VERIFIED");
    }
}

//LIST VIEW
public class VyControl_SubUserListView : VyControl_ListViewBase<VyUserProfileDto, VyControl_SubUserListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_SubUserListView, UxmlTraits> { }
}
