using Packages.com.venly.sdk.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManagerView : EditorWindow
    {
        private VisualElement _containerContent;

        private SDKManagerDetails _panelDetails;
        private SDKManagerSettings _panelSettings;

        private Label _lblNotification;
        private VisualElement _containerNotification;
        private VisualElement _containerMenu;
        private VisualElement _containerLoader;
        private VisualElement _loaderImage;
        private Label _loaderMsg;
        private bool _loaderActive;
        private float _loaderAngle;
        private float _loaderNextUpdate = 0.0f;
        private double _lastEditorTime = 0.0f;
        private readonly float _loaderUpdateThreshold = 0.1f;

        private bool _detailsActive = true;

        public void CreateGUI()
        {
            if (VyEditorData.VerifyIsLoaded())
            {
                rootVisualElement.Bind(VyEditorData.SerializedRuntimeSettings);
            }

            //Root Tree (SDKManagerRoot)
            var rootTree = VenlyEditorUtils.GetUXML_SDKManager("SDKManagerRoot");
            rootTree.CloneTree(rootVisualElement);

            //Content Container (Details/Auth/Settings)
            _containerContent = rootVisualElement.Q<VisualElement>("container-content");

            //Menu Container
            _containerMenu = rootVisualElement.Q<VisualElement>("container-menu");
            _containerMenu.Q<Button>("btnMenu_SDK").clickable.clicked += ShowDetails;
            _containerMenu.Q<Button>("btnMenu_Settings").clickable.clicked += ShowSettings;

            //Notification Container
            _containerNotification = rootVisualElement.Q<VisualElement>("container-notification");
            _lblNotification = _containerNotification.Q<Label>("lbl-notification");
            _lblNotification.text = "SDK Not Yet Initialized...";

            //Loader Container
            _containerLoader = rootVisualElement.Q<VisualElement>("overlay-loader");
            _loaderImage = _containerLoader.Q<VisualElement>("loader");
            _loaderMsg = _containerLoader.Q<Label>("lbl-loader-msg");
            HideLoader(true);

            //Refresh
            VyEditorData.OnLoaded += RefreshView;
            SDKManager.Instance.OnInitialized += RefreshView;
            SDKManager.Instance.OnAuthenticatedChanged += (_) =>
            {
                RefreshView();
            };

            RefreshView();
        }

        public void ShowLoader(string msg = null)
        {
            //set message
            _loaderMsg.ToggleElement(msg != null);
            _loaderMsg.text = msg;

            if (_loaderActive)
                return;

            _loaderActive = true;
            _loaderAngle = 0.0f;
            _lastEditorTime = EditorApplication.timeSinceStartup;

            _containerLoader.ShowElement();
            EditorApplication.update += updateLoader;

            //_containerLoader.MarkDirtyRepaint();
        }

        private void updateLoader()
        {
            float deltaTime = (float)(EditorApplication.timeSinceStartup - _lastEditorTime);
            _lastEditorTime = EditorApplication.timeSinceStartup;
            _loaderNextUpdate -= deltaTime;

            if (_loaderNextUpdate <= 0.0f)
            {
                _loaderAngle += 360.0f / 8.0f;
                if(_loaderAngle >= 360.0f) _loaderAngle = 0;
                _loaderImage.transform.rotation = Quaternion.AngleAxis(_loaderAngle, Vector3.back);
                _loaderImage.MarkDirtyRepaint();

                _loaderNextUpdate = _loaderUpdateThreshold;
            }
        }

        public void HideLoader(bool force = false)
        {
            if (!_loaderActive && !force)
                return;

            EditorApplication.update -= updateLoader;

            _containerLoader.HideElement();
            _loaderActive = false;
        }

        public void RefreshView()
        {
            HideHeader();
            _containerContent.Clear();

            //if (!SDKManager.Instance.IsInitialized)
            //{
            //    SDKManager.Instance.Initialize();
            //}

            if (!SDKManager.Instance.IsInitialized)
            {
                ShowHeader("SDK Manager not initialized...");
                _containerMenu.HideElement();
                return;
            }

            if (!VyEditorData.IsLoaded)
            {
                ShowHeader("Settings not loaded...");
                _containerMenu.HideElement();
                return;
            }

            //SDK manager initialized =>
            if (!SDKManager.Instance.VerifyAuthentication())
            {
                ShowHeader("Not Authenticated...");
            }

            _containerMenu.ShowElement();

            _panelSettings = new SDKManagerSettings();
            _panelDetails = new SDKManagerDetails();
            BindData();

            if (_detailsActive) ShowDetails();
            else ShowSettings();
        }

        private void BindData()
        {
            if (VyEditorData.IsLoaded)
            {
                rootVisualElement.Bind(VyEditorData.SerializedRuntimeSettings);
                _panelSettings.Bind(VyEditorData.SerializedRuntimeSettings);
            }
        }

        public void ShowHeader(string text)
        {
            _lblNotification.text = text;
            _containerNotification.ShowElement();
        }

        public void HideHeader()
        {
            _containerNotification.HideElement();
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