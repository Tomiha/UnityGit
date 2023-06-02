using UnityEngine.UIElements;

namespace Venly.Editor.Tools.Controls
{
    public class LabelFieldRW : ControlBaseRW<Label>
    {
        public new class UxmlFactory : UxmlFactory<LabelFieldRW, UxmlTraits> { }

        public string Value { get; protected set; }

        public new class UxmlTraits : RWTraits
        {
            UxmlStringAttributeDescription m_Value = new() { name = "type", defaultValue = "text"};

            public override void InitRW(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                ((LabelFieldRW)ve).Value = m_Value.GetValueFromBag(bag, cc);
            }
        }

        public LabelFieldRW() : base("LabelFieldRW")
        { }

        protected override void RefreshControl()
        {
            _valueWrite.text = Value;
            _valueRead.text = Value;
        }
    }
}
