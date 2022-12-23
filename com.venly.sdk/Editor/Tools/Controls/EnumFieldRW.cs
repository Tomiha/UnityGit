using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VenlySDK.Utils;

namespace VenlySDK.Editor.Tools.Controls
{
    public class EnumFieldRW : ControlBaseRW<EnumField>
    {
        public new class UxmlFactory : UxmlFactory<EnumFieldRW, UxmlTraits>
        {
        }

        public string Type { get; protected set; }
        public string Value { get; protected set; }

        public new class UxmlTraits : RWTraits
        {
            UxmlStringAttributeDescription m_EnumType = new() {name = "type"};
            UxmlStringAttributeDescription m_ValueType = new() {name = "value"};

            public override void InitRW(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                ((EnumFieldRW) ve).Type = m_EnumType.GetValueFromBag(bag, cc);
                ((EnumFieldRW) ve).Value = m_ValueType.GetValueFromBag(bag, cc);
            }
        }

        public EnumFieldRW() : base("EnumFieldRW", false)
        {
            //_valueWrite.RegisterValueChangedCallback(OnValueChanged);
            //_valueRead.bindingPath = "";
        }

        protected override void RefreshControl()
        {
            if (!string.IsNullOrEmpty(Type))
            {
                var enumType = System.Type.GetType(Type);
                if (enumType != null)
                {
                    var enumNames = Enum.GetNames(enumType);
                    _valueWrite.Init(Enum.Parse(enumType, enumNames[0]) as Enum);

                    if (!string.IsNullOrEmpty(Value))
                    {
                        var enumVal = Enum.Parse(enumType, Value) as Enum;
                        if (enumVal != null)
                        {
                            _valueWrite.value = enumVal;
                        }
                    }
                }
            }
        }

        private void OnValueChanged(ChangeEvent<Enum> evtArgs)
        {
            _valueRead.text = evtArgs.newValue.GetMemberName();
        }
    }
}