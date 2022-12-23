using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VenlySDK.Editor.Utils;

namespace VenlySDK.Editor.Tools.Controls
{
    public abstract class ControlBaseRW : VisualElement
    {
        public enum eEditMode
        {
            Hidden,
            Read,
            Write
        }

        public eEditMode EditMode { get; protected set; } = eEditMode.Write;
        public abstract void RefreshMode(eEditMode mode);
    }

    public abstract class ControlBaseRW<T> : ControlBaseRW where T : VisualElement, IBindable
    {
        protected Label _text;
        protected Label _valueRead;
        protected T _valueWrite;
        private bool _customReadBinding = false;

        public string Text { get; protected set; } = "Label";

        public string BindingPath { get; set; }

        public abstract class RWTraits : VisualElement.UxmlTraits
        {
            UxmlEnumAttributeDescription<eEditMode> m_EditMode = new()
                {name = "editmode", defaultValue = eEditMode.Write};

            UxmlStringAttributeDescription m_FieldName = new() {name = "text", defaultValue = "Label"};
            UxmlStringAttributeDescription m_BindingPath = new() {name = "binding-path"};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public abstract void InitRW(VisualElement ve, IUxmlAttributes bag, CreationContext cc);

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                ((ControlBaseRW<T>) ve).EditMode = m_EditMode.GetValueFromBag(bag, cc);
                ((ControlBaseRW<T>) ve).Text = m_FieldName.GetValueFromBag(bag, cc);
                ((ControlBaseRW<T>) ve).BindingPath = m_BindingPath.GetValueFromBag(bag, cc);

                InitRW(ve, bag, cc);

                ((ControlBaseRW<T>) ve).Refresh();
            }
        }

        public void BindProperty(SerializedProperty property)
        {
            _valueWrite.BindProperty(property);
            _valueRead.BindProperty(property);
        }

        public ControlBaseRW(string uxml, bool customReadBinding = false)
        {
            _customReadBinding = customReadBinding;

            var tree = VenlyEditorUtils.GetUXML_Controls(uxml);
            tree.CloneTree(this);

            _text = this.Q<Label>("name");
            _valueRead = this.Q<Label>("value-read");
            _valueWrite = this.Q<T>("value-write");
        }

        protected abstract void RefreshControl();

        public override void RefreshMode(eEditMode mode)
        {
            EditMode = mode;

            if (EditMode == eEditMode.Hidden)
            {
                this.HideElement();
            }
            else
            {
                this.ShowElement();
                _valueWrite.ToggleElement(EditMode == eEditMode.Write);
                _valueRead.ToggleElement(EditMode == eEditMode.Read);
            }
        }

        private void Refresh()
        {
            _text.text = Text;
            _valueWrite.bindingPath = BindingPath;

            if (!_customReadBinding)
                _valueRead.bindingPath = BindingPath;

            RefreshMode(EditMode);
            RefreshControl();
        }
    }
}