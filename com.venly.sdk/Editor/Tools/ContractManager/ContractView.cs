using UnityEngine.UIElements;
using Venly.Data;
using Venly.Editor.Tools.Controls;

namespace Venly.Editor.Tools.ContractManager
{
    public class ContractView : ItemViewBase
    {
        private TypeValueList _mediaList;

        private Label _headerTitle;
        private Label _headerSubTitle;

        #region Cstr
        public new class UxmlFactory : UxmlFactory<ContractView, UxmlTraits> { }

        public ContractView() : base("ContractView")
        {
            _mediaList = this.Q<TypeValueList>("media-list");

            _headerSubTitle = this.Q<Label>("header-sub-title");
            _headerTitle = this.Q<Label>("header-title");
        }
        #endregion

        private void UpdateHeader()
        {
            var contract = _item.AsContract();
            _headerTitle.text = contract.Name;
            _headerSubTitle.text = $"Contract ({contract.Chain})";
        }

        protected override void OnBindingUpdate(VyItemSO item)
        {
            _mediaList.BindItemSource(item);

            UpdateHeader();
        }
    }
}