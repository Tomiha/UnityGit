using UnityEditor;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.Controls
{
    public class TypeValueItem : ControlBaseRW
    {
        private TextFieldRW _fieldType;
        private TextFieldRW _fieldValue;
        private Button _btnRemoveItem;

        public new class UxmlFactory : UxmlFactory<TypeValueItem, UxmlTraits>
        {
        }

        public TypeValueItem()
        {
            var root = VenlyEditorUtils.GetUXML_Controls("TypeValueItem");
            root.CloneTree(this);

            _fieldType = this.Q<TextFieldRW>("field-type");
            _fieldValue = this.Q<TextFieldRW>("field-value");
            _btnRemoveItem = this.Q<Button>("btn-item-remove");
        }

        public void BindProperty(SerializedProperty property)
        {
            _fieldType.BindProperty(property.FindPropertyRelative("Type"));
            _fieldValue.BindProperty(property.FindPropertyRelative("Value"));
        }

        public override void RefreshMode(eEditMode mode)
        {
            EditMode = mode;

            _fieldType.RefreshMode(mode);
            _fieldValue.RefreshMode(mode);

            _btnRemoveItem.ToggleElement(EditMode == eEditMode.Write);
        }
    }
}