using System;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManagerDetails : VisualElement
    {
        private Label _lblVersion;
        private Label _lblUpdateText;
        private Button _btnUpdateSDK;
        private Button _btnCheckUpdate;

        private string _currentVersion;
        private string _latestVersion = null;

        public SDKManagerDetails()
        {
            VenlyEditorUtils.GetUXML_SDKManager("SDKManagerDetails").CloneTree(this);

            _lblVersion = this.Q<Label>("details-version");
            _lblUpdateText = this.Q<Label>("details-update-text");
            _btnUpdateSDK = this.Q<Button>("btn-update");
            _btnUpdateSDK.clickable.clicked += OnUpdateSDK_Clicked;

            _btnCheckUpdate = this.Q<Button>("btn-check-update");
            _btnCheckUpdate.clickable.clicked += RetrieveVersionList;

            _lblUpdateText.HideElement();
            _btnUpdateSDK.HideElement();

            _currentVersion = "v" + VenlySettingsEd.Instance.EditorData.PackageInfo.version;
            _lblVersion.text = $"SDK {_currentVersion}";

            RetrieveVersionList();
            RefreshDetails();
        }

        private void OnUpdateSDK_Clicked()
        {
            //var sdkManagerType = Type.GetType("VenlySDKManager,VenlySDK.Manager");
            //var instanceProp = sdkManagerType.GetProperty("Instance");
            //var sdkManagerInstance = instanceProp.GetValue(null);
            //var methodInfo = sdkManagerType?.GetMethod("UpdatePackages");

            var packages = new[]
            {
                $"git+https://github.com/TimCassell/ProtoPromise.git?path=ProtoPromise_Unity/Assets/Plugins/ProtoPromise#v2.3.0",
                $"git+https://github.com/Tomiha/UnityGit?path=com.venly.sdk#{_latestVersion}"
            };

            SDKManager.Instance.UpdatePackages(packages);

            //methodInfo.Invoke(sdkManagerInstance, new[] { packages , null});
        }

        private void RetrieveVersionList()
        {
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/Tomiha/UnityGit/main/versions.txt");
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest().completed += (op) =>
            {
                var versions = request.downloadHandler.text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                _latestVersion = versions.Last();

                RefreshDetails();
            };
        }

        private void RefreshDetails()
        {
            var canUpdate = !string.IsNullOrEmpty(_latestVersion) && !_currentVersion.Equals(_latestVersion);

            if (canUpdate)
            {
                _lblUpdateText.text = $"New Version Available!\n({_latestVersion})";
            }
            else
            {
                _lblUpdateText.text = "Latest version installed.";
            }

            _lblUpdateText.ToggleElement(true);
            _btnUpdateSDK.ToggleElement(canUpdate);
            //_btnCheckUpdate.ToggleElement(!canUpdate);
        }
    }
}