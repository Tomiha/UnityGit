using System;
using Packages.com.venly.sdk.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManagerDetails : VisualElement
    {
        private Label _lblCurrVersion;
        private Label _lblUpdateText;
        private Label _lblNewVersion;
        private Button _btnUpdateSDK;
        private Button _btnCheckUpdate;

        private string _latestVersion = null;

        public SDKManagerDetails()
        {
            VenlyEditorUtils.GetUXML_SDKManager("SDKManagerContent_main").CloneTree(this);

            _lblCurrVersion = this.Q<Label>("lbl-current-version");
            _lblUpdateText = this.Q<Label>("lbl-update-notification");
            _lblNewVersion = this.Q<Label>("lbl-new-version");
            _btnUpdateSDK = this.Q<Button>("btn-update");
            _btnUpdateSDK.clickable.clicked += OnUpdateSDK_Clicked;

            _btnCheckUpdate = this.Q<Button>("btn-check-update");
            _btnCheckUpdate.clickable.clicked += () => RetrieveLatestVersion(true);

            _lblUpdateText.HideElement();
            _btnUpdateSDK.HideElement();

            //Buttons
            this.Q<Button>("btn-link-guide").clickable.clicked += () => { Application.OpenURL(SDKManager.URL_Guide); };
            //this.Q<Button>("btn-link-apiref").HideElement();
            this.Q<Button>("btn-link-bug-feature").clickable.clicked += () => { Application.OpenURL(SDKManager.URL_GitHubIssues); };
            this.Q<Button>("btn-link-changelog").clickable.clicked += () => { Application.OpenURL(SDKManager.URL_ChangeLog); };
            this.Q<Button>("btn-link-discord").clickable.clicked += () => { Application.OpenURL(SDKManager.URL_Discord); };

            RetrieveLatestVersion();
        }

        private void RetrieveLatestVersion(bool force = false)
        {
            _latestVersion = null;
            RefreshDetails();

            if (!force)
            {
                var lastUpdateCheck = PlayerPrefs.GetString("venly.lastUpdateCheck");
                if (!string.IsNullOrEmpty(lastUpdateCheck))
                {
                    var lastCheckTime = DateTime.Parse(lastUpdateCheck);
                    var nextCheckTime = lastCheckTime.AddHours(1);
                    if (nextCheckTime > DateTime.Now)
                    {
                        return;
                    }
                }
            }

            SDKManager.Instance.ShowLoader("Retrieving version data...");

            SDKManager.Instance.GetLatestVersion()
                .OnSuccess(latestVersion =>
                {
                    _latestVersion = latestVersion;
                    RefreshDetails();

                    PlayerPrefs.SetString("venly.lastUpdateCheck", DateTime.UtcNow.ToString("O"));
                })
                .OnFail((ex) =>
                {
                    Debug.LogWarning("[Venly SDK Manager] Failed to retrieve SDK release list.");
                })
                .Finally(() =>
                {
                    SDKManager.Instance.HideLoader();
                });
        }

        private void RefreshDetails()
        {
            var currentVersion = VyEditorData.EditorSettings.Version;
            _lblCurrVersion.text = $"SDK {currentVersion}";

            bool canUpdate = false;
            if (!string.IsNullOrEmpty(_latestVersion))
            {
                var currVersion = VenlyEditorUtils.ParseSemVer(currentVersion);
                var newVersion = VenlyEditorUtils.ParseSemVer(_latestVersion);

                canUpdate = newVersion > currVersion;
            }

            if (canUpdate)
            {
                _lblUpdateText.text = "New Version Available!";
                _lblNewVersion.ShowElement();
                _lblNewVersion.text = $"({_latestVersion})";
            }
            else
            {
                _latestVersion = null;
                _lblUpdateText.text = "Latest Version Installed!";

                _lblNewVersion.HideElement();
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