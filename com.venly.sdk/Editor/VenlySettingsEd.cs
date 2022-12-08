using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Venly.Editor.Utils;
using Venly.Models;
using AssetDatabase = UnityEditor.AssetDatabase;

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

        public string SdkRootPath { get; private set; }
        public string SdkResourcePath { get; private set; }

        private string _sdkEditorPath { get; set; }
        private string _sdkEditorResourcePath { get; set; }

        private const string SdkPackageRoot = "Packages\\com.venly.sdk\\";
        //private const string SDKFolderName = "VenlySDK";
        private const string SettingsFilename = "VenlySettings";
        private const string EditorDataFilename = "VenlyEditorData";

        private VenlySettingsSO _settingsSO;
        public VenlySettingsSO Settings => _settingsSO;

        private VenlyEditorDataSO _editorDataSO;
        public VenlyEditorDataSO EditorData => _editorDataSO;

        private void Initialize()
        {
            if (!FindSDKRoot()) return;
            LoadEditorData();
            LoadSettings();
            VerifySettings();

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

        private bool FindSDKRoot()
        {
            //SDK Root & Resources 
            SdkRootPath = SdkPackageRoot;
            //SdkRootPath = ToolUtils.FindAssetFolder(SDKFolderName);

            if (SdkRootPath != null)
            {
                //Verify or Create Resources root
                if (!AssetDatabase.IsValidFolder($"{SdkRootPath}Resources"))
                {
                    AssetDatabase.CreateFolder(SdkRootPath, "Resources");
                }

                SdkResourcePath = $"{SdkRootPath}Resources\\";
            }
            else
            {
                Debug.LogWarning("[Venly SDK] Root Path not found...");
                return false;
            }

            //SDK Editor Resources
            if (!AssetDatabase.IsValidFolder($"{SdkRootPath}Editor")) return false;
            _sdkEditorPath = $"{SdkRootPath}Editor\\";

            if (!AssetDatabase.IsValidFolder($"{_sdkEditorPath}Resources"))
            {
                AssetDatabase.CreateFolder(_sdkEditorPath, "Resources");
            }

            _sdkEditorResourcePath = $"{_sdkEditorPath}Resources\\";

            return true;
        }

        private void LoadSettings()
        {
            var allSettings = Resources.LoadAll<VenlySettingsSO>("");
            if (allSettings.Any()) //Settings Found
            {
                _settingsSO = allSettings[0];
                if (allSettings.Length > 1) //Multiple Settings files
                {
                    Debug.LogWarning("[Venly SDK Manager] Multiple settings files found... (removing all but one)");
                    foreach (var loadedSettings in allSettings)
                    {
                        if (Settings != loadedSettings)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(loadedSettings));
                        }
                    }
                }
            }
            else
            {
                _settingsSO = ScriptableObject.CreateInstance<VenlySettingsSO>();
                //newSettings.hideFlags = HideFlags.NotEditable;
                AssetDatabase.CreateAsset(Settings, $"{SdkRootPath}{SettingsFilename}.asset");
            }
        }

        private void LoadEditorData()
        {
            var allSettings = Resources.LoadAll<VenlyEditorDataSO>("");
            if (allSettings.Any()) //Settings Found
            {
                _editorDataSO = allSettings[0];
                if (allSettings.Length > 1) //Multiple Settings files
                {
                    Debug.LogWarning("[Venly SDK Manager] Multiple editor data files found... (removing all but one)");
                    foreach (var loadedSettings in allSettings)
                    {
                        if (Settings != loadedSettings)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(loadedSettings));
                        }
                    }
                }
            }
            else
            {
                _editorDataSO = ScriptableObject.CreateInstance<VenlyEditorDataSO>();
                //newSettings.hideFlags = HideFlags.NotEditable;
                AssetDatabase.CreateAsset(_editorDataSO, $"{_sdkEditorResourcePath}{EditorDataFilename}.asset");
            }
        }

        private void VerifySettings()
        {
            //Set SDK Resource Path (Editor Only)
            Settings.SdkResourcePath = SdkResourcePath;

            //Editor Data
            EditorData.SdkRootPath = SdkRootPath;
            EditorData.SdkResourcesPath = SdkResourcePath;
            EditorData.SdkEditorRootPath = _sdkEditorPath;
            EditorData.SdkEditorResourcesPath = _sdkEditorResourcePath;

            //Sync Settings
            EditorData.SelectedBackend = Settings.BackendProvider;
        }

        //[MenuItem("Venly/Create Shared Settings", true)]
        //private static bool ValidateSharedSettings()
        //{
        //    return _settings == null;
        //}

        //[MenuItem("Venly/Create Shared Settings")]
        //private static void CreateSharedSettings()
        //{
        //    VenlySettingsSO asset = ScriptableObject.CreateInstance<VenlySettingsSO>();

        //    //todo: wrap in utitily function
        //    var lastSeparatorIndex = 0;
        //    var prevPath = "Assets";
        //    var appendPath = $"VenlySDK/{RelativeSettingsPath}";

        //    while (true)
        //    {
        //        var nextSeperatorIndex = appendPath.IndexOf('/', lastSeparatorIndex);
        //        if (nextSeperatorIndex == -1) break;

        //        var subStr = appendPath.Substring(lastSeparatorIndex, nextSeperatorIndex - lastSeparatorIndex);
        //        lastSeparatorIndex = nextSeperatorIndex + 1;

        //        if (!AssetDatabase.IsValidFolder($"{prevPath}/{subStr}"))
        //        {
        //            if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(prevPath, subStr)))
        //            {
        //                Debug.LogWarning($"Failed to create VenlySettings. (CreateFolder fail | {prevPath}/{subStr})");
        //            }
        //        }

        //        prevPath += $"/{subStr}";
        //    }

        //    //todo: fix path
        //    AssetDatabase.CreateAsset(asset, $"Assets/VenlySDK/{RelativeSettingsPath}{SettingsFilename}.asset");
        //    AssetDatabase.SaveAssets();

        //    EditorUtility.FocusProjectWindow();

        //    Selection.activeObject = asset;

        //    //Set current setting instance
        //    _settingsInstance = asset;
        //}

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