using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Venly.Editor.Utils;
using Venly.Models;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManagerSettings : VisualElement
    {
        private VisualTreeAsset _templateSettingsAuth;
        private VisualTreeAsset _templateSettingsMain;

        private VisualElement _panelSettingsAuth;
        private VisualElement _panelSettingsMain;

        //Auth Settings Elements

        //Main Settings Elements
        private DropdownField _selectorAppId;
        private EnumField _selectorBackendProvider;
        private Button _btnApplySettings;

        private VisualElement _groupBackendSettings;
        private SerializedProperty _backendSettings = null;

        private VenlyEditorDataSO.SDKManagerData SdkManagerData => VenlySettingsEd.Instance.EditorData.SDKManager;

        #region Cstr
        public new class UxmlFactory : UxmlFactory<SDKManagerSettings, UxmlTraits>
        {
        }

        public SDKManagerSettings()
        {
            this.style.flexGrow = new StyleFloat(1);

            _panelSettingsAuth = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerSettings_Auth").CloneTree().Children().First();
            _panelSettingsAuth.Bind(new SerializedObject(VenlySettingsEd.Instance.Settings)); //hmm

            _panelSettingsAuth.Q<Button>("btn-save-auth").clickable.clicked += onSaveAuth_Clicked;

            //Main Settings Elements
            _panelSettingsMain = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerSettings_Main").CloneTree().Children().First();

            _selectorBackendProvider = _panelSettingsMain.Q<EnumField>("selector-backendprovider");
            _selectorBackendProvider.RegisterValueChangedCallback(onBackendProvider_Changed);

            _selectorAppId = _panelSettingsMain.Q<DropdownField>("selector-app-id");
            _selectorAppId.RegisterValueChangedCallback(onAppId_Changed);

            _panelSettingsMain.Q<Button>("btn-set-id").clickable.clicked += onSetId_Clicked;

            _btnApplySettings = _panelSettingsMain.Q<Button>("btn-apply");
            _btnApplySettings.clickable.clicked += onApplySettings_Clicked;
            _btnApplySettings.HideElement();

            _panelSettingsMain.Q<Button>("btn-refresh-apps").clickable.clicked += onRefreshApps_Clicked;

            _groupBackendSettings = _panelSettingsMain.Q<VisualElement>("group-backend-settings");


            this.Bind(new SerializedObject(VenlySettingsEd.Instance.Settings));

            RefreshPanels();
        }

        private async void RefreshPanels(bool forceAuth = false)
        {
            Clear();

            var authSuccess = await VenlySettingsEd.Instance.VerifyAuthSettings();
            if (!forceAuth && authSuccess)
            {
                //MAIN SETTINGS
                Add(_panelSettingsMain);
                _selectorAppId.choices = SdkManagerData.AvailableAppIds;
                _selectorAppId.value = VenlySettings.ApplicationId;
                if (!SdkManagerData.AvailableAppIds.Contains(VenlySettings.ApplicationId))
                {
                    _selectorAppId.index = 0;
                }

                _selectorBackendProvider.value = SdkManagerData.SelectedBackend;
                ValidateApplyVisibility();
                PopulateBackendSettings();
            }
            else
            {
                //AUTH SETTINGS
                Add(_panelSettingsAuth);
            }
        }
        #endregion

        private void ValidateApplyVisibility()
        {
            var applyVisible = false;

            //Check Backend Changed
            if (SdkManagerData.SelectedBackend != VenlySettings.BackendProvider)
            {
                applyVisible = true;
            }

            //...

            _btnApplySettings.ToggleElement(applyVisible);
            SdkManagerData.UnappliedSettings = applyVisible;
        }

        #region Events
        //AUTH EVENTS
        private void onSaveAuth_Clicked()
        {
            RefreshPanels(); //Refresh & Verify
        }

        //MAIN SETTINGS EVENTS
        private void onBackendProvider_Changed(ChangeEvent<Enum> eventArgs)
        {
            SdkManagerData.SelectedBackend = (eVyBackendProvider)eventArgs.newValue;
            ValidateApplyVisibility();

            PopulateBackendSettings();
        }

        private void onAppId_Changed(ChangeEvent<string> eventArgs)
        {
            VenlySettingsEd.Instance.Settings.ApplicationId = eventArgs.newValue;
        }

        private void onSetId_Clicked()
        {
            RefreshPanels(true);
        }

        private void onApplySettings_Clicked()
        {
            SDKManager.Instance.ConfigureForBackend(SdkManagerData.SelectedBackend);
            ValidateApplyVisibility();
        }

        private void onRefreshApps_Clicked()
        {
            VenlySettingsEd.Instance.RefreshAvailableApps();
            if (VenlySettings.ApplicationId != null)
            {
                _selectorAppId.value = VenlySettings.ApplicationId;
            }
        }
        #endregion

        private void PopulateBackendSettings()
        {
            //Find BackendSettings
            var settingsName = $"{SdkManagerData.SelectedBackend}BackendSettings";
            var serializedSettings = new SerializedObject(VenlySettingsEd.Instance.Settings);
            _backendSettings = serializedSettings.FindProperty(settingsName);

            if (_backendSettings == null)
            {
                _groupBackendSettings.HideElement();
                return;
            }

            if (!_backendSettings.hasVisibleChildren)
            {
                _groupBackendSettings.HideElement();
                return;
            }

            _groupBackendSettings.ShowElement();
            var iterProperty = _backendSettings.Copy();
            
            //Get Next Element
            SerializedProperty nextElement = null;
            if (iterProperty.NextVisible(false))
            {
                nextElement = iterProperty.Copy();
            }

            //Reset Iterator Property
            iterProperty = _backendSettings.Copy();

            iterProperty.NextVisible(true);

            var propertyContainer = _groupBackendSettings.Q<VisualElement>("container-backend-settings");
            propertyContainer.Clear();

            do
            {
                if (SerializedProperty.EqualContents(iterProperty, nextElement)) break;

                AddBackendSettingsItem(propertyContainer, iterProperty);
            } while (iterProperty.NextVisible(false));
        }

        private void AddBackendSettingsItem(VisualElement root, SerializedProperty property)
        {
            var propertyRoot = new PropertyField(property);
            propertyRoot.BindProperty(property);

            root.Add(propertyRoot);
        }
    }
}