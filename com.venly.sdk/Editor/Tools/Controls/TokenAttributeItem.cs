using UnityEditor;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.Controls
{
    public class TokenAttributeItem : ControlBaseRW
    {
        private Button _btnRemoveItem;

        private EnumFieldRW _fieldType;
        private TextFieldRW _fieldName;
        private TextFieldRW _fieldValue;

        public new class UxmlFactory : UxmlFactory<TokenAttributeItem, UxmlTraits>
        {
        }

        public TokenAttributeItem()
        {
            var root = ToolUtils.GetControlUXML("TokenAttributeItem");
            root.CloneTree(this);

            _btnRemoveItem = this.Q<Button>("btn-item-remove");

            _fieldType = this.Q<EnumFieldRW>("field-type");
            _fieldName = this.Q<TextFieldRW>("field-name");
            _fieldValue = this.Q<TextFieldRW>("field-value");
        }

        public void BindProperty(SerializedProperty property)
        {
            _fieldType.BindProperty(property.FindPropertyRelative("Type"));
            _fieldName.BindProperty(property.FindPropertyRelative("Name"));
            _fieldValue.BindProperty(property.FindPropertyRelative("Value"));
        }

        public override void RefreshMode(eEditMode mode)
        {
            EditMode = mode;

            _fieldType.RefreshMode(mode);
            _fieldName.RefreshMode(mode);
            _fieldValue.RefreshMode(mode);

            _btnRemoveItem.ToggleElement(EditMode == eEditMode.Write);
        }
    }
}