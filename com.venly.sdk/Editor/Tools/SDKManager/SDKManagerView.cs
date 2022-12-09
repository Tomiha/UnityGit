using UnityEditor;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
public class SDKManagerView : EditorWindow
{
    private VisualElement _containerContent;

    private SDKManagerDetails _panelDetails;
    private SDKManagerSettings _panelSettings;

    private Label _lblHeader;
    private VisualElement _panelHeader;

    private bool _detailsActive = true;

    public void CreateGUI()
    {
        //Root Tree
        var rootTree = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerView");
        rootTree.CloneTree(rootVisualElement);

        _containerContent = rootVisualElement.Q<VisualElement>("container-content");

        _panelHeader = rootVisualElement.Q<VisualElement>("panel-header");
        _lblHeader = _panelHeader.Q<Label>("lbl-header");

        rootVisualElement.Q<Button>("btn-details").clickable.clicked += ShowDetails;
        rootVisualElement.Q<Button>("btn-settings").clickable.clicked += ShowSettings;

        HideHeader();
        RefreshView();
    }

    public void RefreshView()
    {
        _panelSettings = new SDKManagerSettings();
        _panelDetails = new SDKManagerDetails();

        if(_detailsActive)ShowDetails();
        else ShowSettings();
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