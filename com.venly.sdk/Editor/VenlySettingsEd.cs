using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.VersionControl;
using UnityEngine;
using Venly.Editor.Utils;
using Venly.Models;
using AssetDatabase = UnityEditor.AssetDatabase;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Venly.Editor
{
    internal class VenlySettingsEd
    {
        private static VenlySettingsEd _instance;
        public static VenlySettingsEd Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VenlySettingsEd();
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }
        private const string SettingsFilename = "VenlySettings";
        private const string EditorDataFilename = "VenlyEditorData";
        private const string _defaultResourceRoot = "Assets\\Resources\\";
        private const string _sdkPackageRoot = "Packages\\com.venly.sdk\\";
        private const string _managerPackageRoot = "Packages\\com.venly.sdkmanager\\";

        private VenlySettingsSO _settingsSO;
        public VenlySettingsSO Settings => _settingsSO;

        private VenlyEditorDataSO _editorDataSO;
        public VenlyEditorDataSO EditorData => _editorDataSO;

        private void Initialize()
        {
            LoadSettings();
            IsInitialized = true;
        }

        [InitializeOnLoadMethod]
        private static void InitializeStatic()
        {
            if (_instance == null)
            {
                _instance = new VenlySettingsEd();
                _instance.Initialize();
            }
        }

        public static void VerifyFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var splitFolders = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var parentFolder = splitFolders[0];
            for (var i = 1; i < splitFolders.Length; i++)
            {
                var childFolder = splitFolders[i];
                if (!AssetDatabase.IsValidFolder($"{parentFolder}\\{childFolder}"))
                    AssetDatabase.CreateFolder(parentFolder, childFolder);

                parentFolder += $"\\{childFolder}";
            }
        }
        
        //some comme
        public static T RetrieveOrCreateResource<T>(string soName, string path = null) where T : ScriptableObject
        {
            var allResources = Resources.LoadAll<T>("");
            if (allResources.Any()) //Settings Found
            {
                var resource = allResources[0];
                var p = AssetDatabase.GetAssetPath(resource);
                if (allResources.Length > 1) //Multiple Settings files
                {
                    Debug.LogWarning($"[Venly SDK] Multiple \'{typeof(T)}\' resources found. (removing all but one)");
                    foreach (var loadedSettings in allResources)
                    {
                        if (resource != loadedSettings)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(loadedSettings));
                        }
                    }
                }

                return resource;
            }

            if(path != null)
            {
                VerifyFolder(path);
                var resource = ScriptableObject.CreateInstance<T>();
                //resource.hideFlags = HideFlags.NotEditable;
                AssetDatabase.CreateAsset(resource, $"{path}{soName}.asset");
                return resource;
            }

            return null;
        }

        private void LoadSettings()
        {
            //Load Settings
            _editorDataSO = RetrieveOrCreateResource<VenlyEditorDataSO>(EditorDataFilename, null);
            if (_editorDataSO == null)
            {
                _editorDataSO = RetrieveOrCreateResource<VenlyEditorDataSO>(EditorDataFilename, _defaultResourceRoot);
                _editorDataSO.PublicResourceRoot = _defaultResourceRoot;
                _editorDataSO.SdkPackageRoot = _sdkPackageRoot;
                _editorDataSO.ManagerPackageRoot = _managerPackageRoot;
            }

            _settingsSO = RetrieveOrCreateResource<VenlySettingsSO>(SettingsFilename, _editorDataSO.PublicResourceRoot);

            //Verify Settings
            Settings.SdkPackageRoot = _editorDataSO.SdkPackageRoot;
            Settings.PublicResourceRoot = _editorDataSO.PublicResourceRoot;

            EditorData.SelectedBackend = Settings.BackendProvider;
            EditorData.PackageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
        }

        public async Task<bool> VerifyAuthSettings(bool forceVerify = true)
        {
            if (string.IsNullOrEmpty(Settings.ClientId) || string.IsNullOrEmpty(Settings.ClientSecret))
            {
                return false;
            }

            //Applist refresh needed?
            if (string.IsNullOrEmpty(EditorData.CurrentClientId) || !EditorData.CurrentClientId.Equals(Settings.ClientId))
            {
                EditorData.AvailableAppIds.Clear();
                EditorData.CurrentClientId = null;
            }

            //Verify Credentials (GetToken)
            var token = await VenlyEditorAPI.GetAccessToken(Settings.ClientId, Settings.ClientSecret);
            if (!token.IsValid)
            {
                EditorData.CurrentClientId = null;
                return false;
            }

            //Check Apps if necessary
            if (!EditorData.AvailableAppIds.Any())
            {
                RefreshAvailableApps();
            }

            //Swap current client id
            EditorData.CurrentClientId = Settings.ClientId;
            return true;
        }

        public async void RefreshAvailableApps()
        {
            var apps = await VenlyEditorAPI.GetApps();
            
            EditorData.AvailableAppIds.Clear();
            EditorData.AvailableAppIds.AddRange(apps.Select(app => app.Id));

            if (!EditorData.AvailableAppIds.Any())
            {
                Settings.ApplicationId = null;
                return;
            }

            if (!EditorData.AvailableAppIds.Contains(Settings.ApplicationId))
            {
                Settings.ApplicationId = EditorData.AvailableAppIds.First();
            }
        }

        public void ConfigureForBackend(eVyBackendProvider backend)
        {
            //Set Defines
            var buildTarget = NamedBuildTarget.Standalone;

            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out var currentDefines);

            //Clear Current Venly Defines
            var definesList = currentDefines.ToList();
            definesList.RemoveAll(define => define.Contains("_VENLY_"));

            //Populate with required Defines
            if (backend == eVyBackendProvider.PlayFab) definesList.Add("ENABLE_VENLY_PLAYFAB");

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, definesList.ToArray());

            //SET BACKEND
            Settings.BackendProvider = backend;
        }
    }
}