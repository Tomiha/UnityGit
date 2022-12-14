using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK.Editor.Utils;
using VenlySDK.Models;

namespace VenlySDK.Editor.Tools.SDKManager
{
    public class SDKManagerSettings : VisualElement
    {
        private VisualElement _panelSettingsAuth;
        private VisualElement _panelSettingsMain;

        //Main Settings Elements
        private EnumField _selectorBackendProvider;
        private Button _btnApplySettings;

        private VisualElement _groupBackendSettings;
        private SerializedProperty _backendSettings = null;

        private VenlyEditorDataSO.SDKManagerData SdkManagerData => VenlyEditorSettings.Instance.EditorData.SDKManager;

        #region Cstr
        public new class UxmlFactory : UxmlFactory<SDKManagerSettings, UxmlTraits>
        {
        }

        public SDKManagerSettings()
        {
            this.style.flexGrow = new StyleFloat(1);

            _panelSettingsAuth = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerSettings_Auth").CloneTree().Children().First();
            _panelSettingsAuth.Bind(VenlyEditorSettings.Instance.SerializedSettings); //hm

            _panelSettingsAuth.Q<Button>("btn-save-auth").clickable.clicked += onSaveAuth_Clicked;

            //Main Settings Elements
            _panelSettingsMain = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerSettings_Main").CloneTree().Children().First();
            _panelSettingsMain.Bind(VenlyEditorSettings.Instance.SerializedSettings);

            _selectorBackendProvider = _panelSettingsMain.Q<EnumField>("selector-backendprovider");
            _selectorBackendProvider.RegisterValueChangedCallback(onBackendProvider_Changed);

            _panelSettingsMain.Q<Button>("btn-set-id").clickable.clicked += onSetId_Clicked;

            _btnApplySettings = _panelSettingsMain.Q<Button>("btn-apply");
            _btnApplySettings.clickable.clicked += onApplySettings_Clicked;
            _btnApplySettings.HideElement();

            _groupBackendSettings = _panelSettingsMain.Q<VisualElement>("group-backend-settings");

            SDKManager.Instance.OnAuthenticatedChanged += (_) => { RefreshView(); };
            RefreshView();
        }

        private void RefreshView(bool forceAuth = false)
        {
            Clear();
            if (!forceAuth && SDKManager.Instance.IsAuthenticated)
            {
                //MAIN SETTINGS
                Add(_panelSettingsMain);

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
        private async void onSaveAuth_Clicked()
        {
            var result = await SDKManager.Instance.Authenticate();
            if(!result.Success)
                Debug.LogException(result.Exception);
        }

        //MAIN SETTINGS EVENTS
        private void onBackendProvider_Changed(ChangeEvent<Enum> eventArgs)
        {
            SdkManagerData.SelectedBackend = (eVyBackendProvider)eventArgs.newValue;
            ValidateApplyVisibility();

            PopulateBackendSettings();
        }

        private void onSetId_Clicked()
        {
            RefreshView(true);
        }

        private void onApplySettings_Clicked()
        {
            SDKManager.Instance.ConfigureForBackend(SdkManagerData.SelectedBackend);
            ValidateApplyVisibility();
        }
        #endregion

        private void PopulateBackendSettings()
        {
            //Find BackendSettings
            var settingsName = $"{SdkManagerData.SelectedBackend}BackendSettings";
            var serializedSettings = VenlyEditorSettings.Instance.SerializedSettings;
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