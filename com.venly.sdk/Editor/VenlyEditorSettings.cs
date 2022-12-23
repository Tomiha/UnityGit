using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Proto.Promises;
using UnityEditor;
using UnityEngine;
using VenlySDK.Core;
using VenlySDK.Editor.Utils;
using AssetDatabase = UnityEditor.AssetDatabase;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace VenlySDK.Editor
{
    internal class VenlyEditorSettings
    {
        private static VenlyEditorSettings _instance;
        public static VenlyEditorSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    VenlyDebugEd.LogDebug("VenlyEditorSettings Singleton Creation");
                    _instance = new VenlyEditorSettings();
                    //_instance.Initialize();
                }

                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }
        public bool SettingsLoaded { get; private set; }

        private const string _sdkPackageRoot = "Packages\\com.venly.sdk\\";
        private const string _sdkEditorDataPath = "Packages\\com.venly.sdk\\Editor\\VenlyEditorData.asset";
        private const string _defaultResourceRoot = "Assets\\Resources\\";
        private const string _sdkPublicSettingsRoot = "Assets\\Resources\\";

        private VenlySettingsSO _settingsSO;
        public VenlySettingsSO Settings => _settingsSO;
        public SerializedObject SerializedSettings;

        private VenlyEditorDataSO _editorDataSO;
        public VenlyEditorDataSO EditorData => _editorDataSO;

        public event Action<VenlySettingsSO> OnSettingsLoaded;
        public event Action<VenlyEditorDataSO> OnEditorDataLoaded;

        private void Initialize()
        {
            if (IsInitialized) return;

            LoadSettings();
            IsInitialized = true;
        }

        internal void LoadSettings()
        {
            VenlyDebugEd.LogDebug("VenlyEditorSettings LoadSettings Called");

            //Load EditorData
            _editorDataSO = LoadSettingsFile<VenlyEditorDataSO>(_sdkEditorDataPath);
            _editorDataSO.hideFlags = HideFlags.NotEditable;
            VenlyEditorUtils.RestoreBackup(_editorDataSO);

            //Load VenlySettings
            _settingsSO = Resources.LoadAll<VenlySettingsSO>(_sdkPublicSettingsRoot).FirstOrDefault();
            if (_settingsSO == null) //First Creation
            {
                _settingsSO = RetrieveOrCreateResource<VenlySettingsSO>("VenlySettings", _sdkPublicSettingsRoot);
                _settingsSO.PublicResourceRoot = _defaultResourceRoot;
            }

            //_settingsSO.hideFlags = HideFlags.HideInInspector;
            _settingsSO.hideFlags = HideFlags.None;

            VenlyEditorUtils.RestoreBackup(_settingsSO);

            VenlyDebugEd.LogDebug($"VenlyEditorSettings Settings SYNC");
            //Sync Settings
            _editorDataSO.PublicResourceRoot = _settingsSO.PublicResourceRoot;
            _editorDataSO.SDKManager.SelectedBackend = _settingsSO.BackendProvider;
            _editorDataSO.SdkPackageRoot = _sdkPackageRoot;
            _editorDataSO.PackageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            _editorDataSO.Version = $"v{_editorDataSO.PackageInfo.version}";

            _settingsSO.SdkPackageRoot = _sdkPackageRoot;

            EditorUtility.SetDirty(_editorDataSO);
            AssetDatabase.SaveAssetIfDirty(_editorDataSO);

            EditorUtility.SetDirty(_settingsSO);
            AssetDatabase.SaveAssetIfDirty(_settingsSO);

            //Load Venly Settings
            VenlySettings.Load();

            VenlyDebugEd.LogDebug("Venly Settings Loaded!");

            //Serialized Objects
            SerializedSettings = new SerializedObject(_settingsSO);

            //Refresh Editor Data
            //todo: invoke refresh event
            if(VenlySettings.HasCredentials)
                RefreshEditorData(false);

            SettingsLoaded = true;
            OnEditorDataLoaded?.Invoke(_editorDataSO);
            OnSettingsLoaded?.Invoke(_settingsSO);
        }

        private void RefreshEditorData(bool force)
        {
            //Refresh SupportedChainsWallet
            if (force ||
                EditorData.SupportedChainsWallet == null
                || EditorData.SupportedChainsWallet.Length == 0)
            {
                VenlyEditorAPI.GetChainsWALLET()
                    .OnSucces(chains =>
                    {
                        EditorData.SupportedChainsWallet = chains;
                    });
            }

            //Refresh SupportedChainsNft
            if (force ||
                EditorData.SupportedChainsNft == null
                || EditorData.SupportedChainsNft.Length == 0)
            {
                VenlyEditorAPI.GetChainsNFT()
                    .OnSucces(chains =>
                    {
                        EditorData.SupportedChainsNft = chains;
                    });
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

        public static T LoadSettingsFile<T>(string assetPath) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                VenlyDebugEd.LogDebug("New VenlyEditorData asset created!");
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            else
            {
                VenlyDebugEd.LogDebug("Existing VenlyEditorData found!");
            }

            return asset;
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

        public VyTask SignInEditor(string clientId, string clientSecret)
        {
            var taskNotifier = VyTask.Create();

            //Verify Credentials
            VenlyEditorAPI.GetAccessToken(clientId, clientSecret)
                .OnComplete(result =>
                {
                    if (result.Success)
                    {
                        taskNotifier.NotifySuccess();
                    }
                    else
                    {
                        taskNotifier.NotifyFail(result.Exception);
                    }
                });

            return taskNotifier.Task;
        }

        public async Task<bool> VerifyAuthSettings(bool forceVerify = true)
        {
            if (string.IsNullOrEmpty(Settings.ClientId) || string.IsNullOrEmpty(Settings.ClientSecret))
            {
                return false;
            }

            if (string.IsNullOrEmpty(EditorData.SDKManager.CurrentClientId) || !EditorData.SDKManager.CurrentClientId.Equals(Settings.ClientId))
            {
                EditorData.SDKManager.CurrentClientId = null;
            }

            //Make sure the Settings Are Available
            var env = VenlySettings.Environment;

            //Verify Credentials (GetToken)
            var signInSuccess = await SignInEditor(Settings.ClientId, Settings.ClientSecret).AwaitResult(false);
            if (signInSuccess)
            {
                EditorData.SDKManager.CurrentClientId = Settings.ClientId;
                return true;
            }

            EditorData.SDKManager.CurrentClientId = null;
            return false;
            //var token = await VenlyEditorAPI.GetAccessToken(Settings.ClientId, Settings.ClientSecret).AwaitResult();
            //if (!token.IsValid)
            //{
            //    EditorData.SDKManager.CurrentClientId = null;
            //    return false;
            //}

            //Swap current client id
            //EditorData.SDKManager.CurrentClientId = Settings.ClientId;
            //return true;
        }
    }
}