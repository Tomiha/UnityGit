using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VenlySDK.Editor.Utils;

namespace VenlySDK.Editor.Tools.SDKManager
{
public class SDKManagerView : EditorWindow
{
    private VisualElement _containerContent;

    private SDKManagerDetails _panelDetails;
    private SDKManagerSettings _panelSettings;

    private Label _lblHeader;
    private VisualElement _panelHeader;
    private VisualElement _panelMenu;

    private bool _detailsActive = true;

    public void CreateGUI()
    {
        if (SDKManager.Instance.SettingsLoaded)
        {
            rootVisualElement.Bind(VenlyEditorSettings.Instance.SerializedSettings);
        }

        //Root Tree
        var rootTree = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerView");
        rootTree.CloneTree(rootVisualElement);

        _containerContent = rootVisualElement.Q<VisualElement>("container-content");

        _panelMenu = rootVisualElement.Q<VisualElement>("panel-menu");

        _panelHeader = rootVisualElement.Q<VisualElement>("panel-header");
        _lblHeader = _panelHeader.Q<Label>("lbl-header");
        _lblHeader.text = "SDK Not Yet Initialized...";

        rootVisualElement.Q<Button>("btn-details").clickable.clicked += ShowDetails;
        rootVisualElement.Q<Button>("btn-settings").clickable.clicked += ShowSettings;

        SDKManager.Instance.OnSettingsLoaded += RefreshView;
        SDKManager.Instance.OnInitialized += RefreshView;
        SDKManager.Instance.OnAuthenticatedChanged += (_)=>
        {
            RefreshView();
        };

        RefreshView();
    }

    public void RefreshView()
    {
        HideHeader();
        _containerContent.Clear();

        if (!SDKManager.Instance.IsInitialized)
        {
            ShowHeader("SDK Manager not yet initialized...");
            _panelMenu.HideElement();
            return;
        }

        if (!SDKManager.Instance.IsInitialized)
        {
            ShowHeader("Settings not loaded...");
            _panelMenu.HideElement();
            return;
        }

        //SDK manager initialized =>
        if (!SDKManager.Instance.IsAuthenticated)
        {
            ShowHeader("Not Authenticated...");
        }

        _panelMenu.ShowElement();

        _panelSettings = new SDKManagerSettings();
        _panelDetails = new SDKManagerDetails();
        BindData();

        if(_detailsActive)ShowDetails();
        else ShowSettings();
    }

    private void BindData()
    {
        if (SDKManager.Instance.SettingsLoaded)
        {
            rootVisualElement.Bind(VenlyEditorSettings.Instance.SerializedSettings);
            _panelSettings.Bind(VenlyEditorSettings.Instance.SerializedSettings);
        }
    }

        public void ShowHeader(string text)
    {
        _lblHeader.text = text;
        _panelHeader.ShowElement();
    }

    public void HideHeader()
    {
        _panelHeader.HideElement();
    }

    private void ShowDetails()
    {
        _detailsActive = true;
        _containerContent.Clear();
        _containerContent.Add(_panelDetails);
    }

    private void ShowSettings()
    {
        _detailsActive = false;
        _containerContent.Clear();
        _containerContent.Add(_panelSettings);
    }
}
}