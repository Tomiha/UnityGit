using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Venly.Editor.Tools.Controls
{
    public class LongFieldRW : ControlBaseRW<LongField>
    {
        public new class UxmlFactory : UxmlFactory<LongFieldRW, UxmlTraits> { }
        public new class UxmlTraits : RWTraits
        {

            public override void InitRW(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
            }
        }

        public bool Multiline { get; private set; } = false;

        public LongFieldRW() : base("LongFieldRW") { }

        protected override void RefreshControl()
        {
        }
    }
}
