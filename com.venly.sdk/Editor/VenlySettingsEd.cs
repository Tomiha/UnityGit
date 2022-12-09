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
        private const string _defaultResourceRoot = "Assets\\Resources\\";
        private const string _sdkPackageRoot = "Packages\\com.venly.sdk\\";

        private VenlySettingsSO _settingsSO;
        public VenlySettingsSO Settings => _settingsSO;

        private VenlyEditorDataSO _editorDataSO;
        public VenlyEditorDataSO EditorData => _editorDataSO;

        private void Initialize()
        {
            if (IsInitialized) return;

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

        public void RefreshSettings()
        {
            if(string.IsNullOrEmpty(_editorDataSO.SdkPackageRoot)) 
                LoadSettings();
        }
        
        private void LoadSettings()
        {
            //Load EditorData
            _editorDataSO = Resources.Load<VenlyEditorDataSO>("");
            if (_editorDataSO == null) //First Creation
            {
                _editorDataSO = RetrieveOrCreateResource<VenlyEditorDataSO>("VenlyEditorData",$"{_sdkPackageRoot}Resources\\");
            }
            VenlyEditorUtils.RestoreBackup(_editorDataSO);


            //Load VenlySettings
            _settingsSO = Resources.Load<VenlySettingsSO>("");
            if (_settingsSO == null) //First Creation
            {
                _settingsSO = RetrieveOrCreateResource<VenlySettingsSO>("VenlySettings", $"{_sdkPackageRoot}Resources\\");
                _settingsSO.PublicResourceRoot = _defaultResourceRoot;
            }
            VenlyEditorUtils.RestoreBackup(_settingsSO);


            //Sync Settings
            _editorDataSO.PublicResourceRoot = _settingsSO.PublicResourceRoot;
            _editorDataSO.SDKManager.SelectedBackend = _settingsSO.BackendProvider;
            _editorDataSO.SdkPackageRoot = _sdkPackageRoot;
            _editorDataSO.PackageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            _editorDataSO.Version = $"v{_editorDataSO.PackageInfo.version}";

            _editorDataSO.SDKManager.GitReleaseURL = @"https://api.github.com/repos/Tomiha/UnityGit/releases";
            _editorDataSO.SDKManager.GitSdkURL = @"git+https://github.com/Tomiha/UnityGit.git?path=com.venly.sdk";

            _settingsSO.SdkPackageRoot = _sdkPackageRoot;

            EditorUtility.SetDirty(_editorDataSO);
            AssetDatabase.SaveAssetIfDirty(_editorDataSO);

            EditorUtility.SetDirty(_settingsSO);
            AssetDatabase.SaveAssetIfDirty(_settingsSO);

            Debug.Log("Venly Settings Loaded!");
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

        public async Task<bool> VerifyAuthSettings(bool forceVerify = true)
        {
            if (string.IsNullOrEmpty(Settings.ClientId) || string.IsNullOrEmpty(Settings.ClientSecret))
            {
                return false;
            }

            //Applist refresh needed?
            if (string.IsNullOrEmpty(EditorData.SDKManager.CurrentClientId) || !EditorData.SDKManager.CurrentClientId.Equals(Settings.ClientId))
            {
                EditorData.SDKManager.AvailableAppIds.Clear();
                EditorData.SDKManager.CurrentClientId = null;
            }

            //Verify Credentials (GetToken)
            var token = await VenlyEditorAPI.GetAccessToken(Settings.ClientId, Settings.ClientSecret);
            if (!token.IsValid)
            {
                EditorData.SDKManager.CurrentClientId = null;
                return false;
            }

            //Check Apps if necessary
            if (!EditorData.SDKManager.AvailableAppIds.Any())
            {
                RefreshAvailableApps();
            }

            //Swap current client id
            EditorData.SDKManager.CurrentClientId = Settings.ClientId;
            return true;
        }

        public async void RefreshAvailableApps()
        {
            var apps = await VenlyEditorAPI.GetApps();
            
            EditorData.SDKManager.AvailableAppIds.Clear();
            EditorData.SDKManager.AvailableAppIds.AddRange(apps.Select(app => app.Id));

            if (!EditorData.SDKManager.AvailableAppIds.Any())
            {
                Settings.ApplicationId = null;
                return;
            }

            if (!EditorData.SDKManager.AvailableAppIds.Contains(Settings.ApplicationId))
            {
                Settings.ApplicationId = EditorData.SDKManager.AvailableAppIds.First();
            }
        }
    }
}