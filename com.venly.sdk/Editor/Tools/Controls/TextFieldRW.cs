using UnityEngine.UIElements;

namespace Venly.Editor.Tools.Controls
{
    public class TextFieldRW : ControlBaseRW<TextField>
    {
        public new class UxmlFactory : UxmlFactory<TextFieldRW, UxmlTraits> { }
        public new class UxmlTraits : RWTraits
        {
            UxmlBoolAttributeDescription m_Multiline = new() { name = "multiline", defaultValue = false };

            public override void InitRW(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                ((TextFieldRW)ve).Multiline = m_Multiline.GetValueFromBag(bag, cc);
            }
        }

        public bool Multiline { get; private set; } = false;

        public TextFieldRW() : base("TextFieldRW") { }

        protected override void RefreshControl()
        {
            _valueWrite.multiline = Multiline;
        }
    }
}
