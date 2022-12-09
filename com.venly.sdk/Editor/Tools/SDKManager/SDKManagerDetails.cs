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

        private string _latestVersion = null;

        public SDKManagerDetails()
        {
            VenlyEditorUtils.GetUXML_SDKManager("SDKManagerDetails").CloneTree(this);

            _lblVersion = this.Q<Label>("details-version");
            _lblUpdateText = this.Q<Label>("details-update-text");
            _btnUpdateSDK = this.Q<Button>("btn-update");
            _btnUpdateSDK.clickable.clicked += OnUpdateSDK_Clicked;

            _btnCheckUpdate = this.Q<Button>("btn-check-update");
            _btnCheckUpdate.clickable.clicked += RetrieveLatestVersion;

            _lblUpdateText.HideElement();
            _btnUpdateSDK.HideElement();

            RetrieveLatestVersion();
        }

        private void RetrieveLatestVersion()
        {
            _latestVersion = null;
            RefreshDetails();

            SDKManager.Instance.GetLatestVersion()
                .Then(latestVersion =>
                {
                    _latestVersion = latestVersion;
                    RefreshDetails();
                }).Forget();
        }

        private void RefreshDetails()
        {
            var currentVersion = VenlySettingsEd.Instance.EditorData.Version;
            _lblVersion.text = $"SDK {currentVersion}";

            bool canUpdate = false;
            if (!string.IsNullOrEmpty(_latestVersion))
            {
                var currVersion = VenlyEditorUtils.ParseSemVer(currentVersion);
                var newVersion = VenlyEditorUtils.ParseSemVer(_latestVersion);

                canUpdate = newVersion > currVersion;
            }

            if (canUpdate)
            {
                _lblUpdateText.text = $"New Version Available!\n({_latestVersion})";
            }
            else
            {
                _latestVersion = null;
                _lblUpdateText.text = "Latest version installed.";
            }

            _lblUpdateText.ToggleElement(true);
            _btnUpdateSDK.ToggleElement(canUpdate);
        }

        private void OnUpdateSDK_Clicked()
        {
            if (string.IsNullOrEmpty(_latestVersion))
            {
                RefreshDetails();
                return;
            }

            SDKManager.Instance.UpdateSDK(_latestVersion);
        }
    }
}