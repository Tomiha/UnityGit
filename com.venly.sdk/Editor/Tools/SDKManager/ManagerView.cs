using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
public class ManagerView : EditorWindow
{
    private VisualElement _contentPanel;
    private VisualElement _managerRoot;
    private VisualElement _panelSetup;

    private VenlyManagerSO _sdkSettings;
    private string _rootPath;

    public void CreateGUI()
    {
        _rootPath = VenlySDKManager.Instance.Settings.ManagerPackageRoot;

        //Root Tree
        var rootTree = ToolUtils.GetSDKManagerUXML("ManagerView");
        rootTree.CloneTree(rootVisualElement);

        _contentPanel = rootVisualElement.Q<VisualElement>("panel-content");
        _panelSetup = ToolUtils.GetSDKManagerUXML("ManagerSetupPanel").CloneTree().Children().First();
        _panelSetup.Q<Button>("btn-install").clickable.clicked += () =>
        {
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/Tomiha/UnityGit/main/versions.txt");
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest().completed += (op) =>
            {
                var versions = request.downloadHandler.text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                VenlySDKManager.Instance.InstallSDK(versions.Last());
            };
        };

        RefreshPanels();
    }

    public void RefreshPanels()
    {
        _contentPanel.Clear();
        _sdkSettings = VenlySDKManager.Instance.Settings;

        //Sdk Installed?
        if (!_sdkSettings.IsSdkInstalled)
        {
            _contentPanel.Add(_panelSetup);
        }
        else
        {
            if (_managerRoot == null)
            {
                _managerRoot = new SDKManagerRoot();
                //       //SDK Manager Settings (dynamic)
                //       var managerRootType = Type.GetType("Venly.Editor.Tools.SDKManager.SDKManagerRoot, VenlySDK.Editor");
                //if (managerRootType != null)
                //{
                //    try
                //    {
                //        _managerRoot = (VisualElement) Activator.CreateInstance(managerRootType);
                //    }
                //    catch (Exception e)
                //    {
                //        Debug.LogException(new Exception("Failed to Instantiate SDK Manager Root", e));
                //        return;
                //    }
                //}
            }

            _contentPanel.Add(_managerRoot);
        }
    }
}
}