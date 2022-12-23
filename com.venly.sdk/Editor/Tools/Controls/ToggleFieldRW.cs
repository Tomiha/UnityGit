using UnityEngine.UIElements;

namespace VenlySDK.Editor.Tools.Controls
{
    public class ToggleFieldRW : ControlBaseRW<Toggle>
    {
        public new class UxmlFactory : UxmlFactory<ToggleFieldRW, UxmlTraits> { }

        public new class UxmlTraits : RWTraits
        {
            public override void InitRW(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
            }
        }

        public ToggleFieldRW() : base("ToggleFieldRW") { }

        protected override void RefreshControl()
        {
        }
    }
}
