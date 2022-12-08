using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManagerRoot : VisualElement
    {
        private SDKManagerSettings _panelManagerSettings;
        private SDKManagerMain _panelManagerMain;

        private VisualElement _containerContent;

        private VisualElement _menuPanel;
        private Label _lblHeader;

        private Button _btnSettings;
        private Button _btnMain;

        public SDKManagerRoot()
        {
            _panelManagerMain = new SDKManagerMain();
            _panelManagerSettings = new SDKManagerSettings();

            ToolUtils.GetSDKManagerUXML("SDKManagerRoot").CloneTree(this);

            _containerContent = this.Q<VisualElement>("container-content");

            _menuPanel = this.Q<VisualElement>("panel-menu");
            _lblHeader = this.Q<Label>("lbl-header");

            _btnSettings = this.Q<Button>("btn-settings");
            _btnSettings.clickable.clicked += ShowSettings;

            _btnMain = this.Q<Button>("btn-main");
            _btnMain.clickable.clicked += ShowMain;

            ShowMain();
        }

        private void ShowMain()
        {
            _containerContent.Clear();
            _containerContent.Add(_panelManagerMain);
        }

        private void ShowSettings()
        {
            _containerContent.Clear();
            _containerContent.Add(_panelManagerSettings);
        }
    }
}