using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VenlySDK.Core;
using VenlySDK.Editor.Tools.SDKManager;
using VenlySDK.Editor.Utils;
using AssetDatabase = UnityEditor.AssetDatabase;

namespace VenlySDK.Editor
{
    internal class VenlyEditorSettings
    {
        private static VenlyEditorSettings _instance;
        //public static VenlyEditorSettings Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            VenlyDebugEd.LogDebug("VenlyEditorSettings Singleton Creation");
        //            _instance = new VenlyEditorSettings();
        //        }

        //        return _instance;
        //    }
        //}

        public bool IsInitialized { get; private set; }
        public bool SettingsLoaded { get; private set; }

        private VenlySettingsSO _settingsSO;
        public VenlySettingsSO Settings => _settingsSO;
        public SerializedObject SerializedSettings;

        private VenlyEditorDataSO _editorDataSO;
        public VenlyEditorDataSO EditorData => _editorDataSO;

        private bool _initInProgress = false;

        //private void Initialize()
        //{
        //    if (IsInitialized) return;
        //    //if (_initInProgress) return;

        //    _initInProgress = true;

        //    LoadSettings();
        //    IsInitialized = true;
        //    _initInProgress = false;
        //}

        //private void VerifyData()
        //{

        //}

        ////Can be called to make sure the Settings are properly loaded
        //internal static void Load()
        //{
        //    if (_instance == null)
        //    {
        //        _instance = new VenlyEditorSettings();
        //        _instance.Initialize();
        //    }

        //    if (!_instance.SettingsLoaded)
        //    {
        //        _instance.LoadSettings();
        //    }
        //}

        //internal void LoadSettings()
        //{
        //    VenlyDebugEd.LogDebug("VenlyEditorSettings LoadSettings Called");

        //    //EDITOR SETTINGS
        //    //===============

        //    //Load EditorSettings
        //    _editorDataSO = Resources.LoadAll<VenlyEditorDataSO>("").FirstOrDefault();
        //    if (_editorDataSO == null) //First Creation
        //    {
        //        _editorDataSO = RetrieveOrCreateResource<VenlyEditorDataSO>("VenlyEditorData",SDKManager.DefaultPublicResourceRoot);
        //    }

        //    //VenlyEditorUtils.RestoreBackup(_editorDataSO);
        //    _editorDataSO.hideFlags = HideFlags.NotEditable;

        //    //Update EditorSettings
        //    //SDKManager.Instance.UpdateEditorSettings();

        //    //Save Changes
        //    EditorUtility.SetDirty(_editorDataSO);
        //    AssetDatabase.SaveAssetIfDirty(_editorDataSO);

        //    //VENLY SETTINGS
        //    //==============

        //    //Load VenlySettings
        //    _settingsSO = Resources.LoadAll<VenlySettingsSO>("").FirstOrDefault();
        //    if (_settingsSO == null) //First Creation
        //    {
        //        _settingsSO = RetrieveOrCreateResource<VenlySettingsSO>("VenlySettings", SDKManager.DefaultPublicResourceRoot);
        //        _settingsSO.PublicResourceRoot = SDKManager.DefaultPublicResourceRoot;
        //    }

        //    //VenlyEditorUtils.RestoreBackup(_settingsSO);
        //    //_settingsSO.hideFlags = HideFlags.HideInInspector;
        //    _settingsSO.hideFlags = HideFlags.None;

        //    //Update VenlySettings
        //    SDKManager.Instance.UpdateVenlySettings(_settingsSO);

        //    //Save Changes
        //    EditorUtility.SetDirty(_settingsSO);
        //    AssetDatabase.SaveAssetIfDirty(_settingsSO);

        //    //Small reassurance
        //    VenlySettings.Load(_settingsSO);

        //    //Serialized Objects
        //    SerializedSettings = new SerializedObject(_settingsSO);

        //    //Load Venly Settings (Static)
        //    VenlySettings.Load();

        //    SettingsLoaded = true;
        //}

        //public static void VerifyFolder(string path)
        //{
        //    if (AssetDatabase.IsValidFolder(path)) return;

        //    var splitFolders = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        //    var parentFolder = splitFolders[0];
        //    for (var i = 1; i < splitFolders.Length; i++)
        //    {
        //        var childFolder = splitFolders[i];
        //        if (!AssetDatabase.IsValidFolder($"{parentFolder}\\{childFolder}"))
        //            AssetDatabase.CreateFolder(parentFolder, childFolder);

        //        parentFolder += $"\\{childFolder}";
        //    }
        //}

        //public static T LoadSettingsFile<T>(string assetPath) where T : ScriptableObject
        //{
        //    var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        //    if (asset == null)
        //    {
        //        VenlyDebugEd.LogDebug("New VenlyEditorData asset created!");
        //        asset = ScriptableObject.CreateInstance<T>();
        //        AssetDatabase.CreateAsset(asset, assetPath);
        //    }
        //    else
        //    {
        //        VenlyDebugEd.LogDebug("Existing VenlyEditorData found!");
        //    }

        //    return asset;
        //}
        
        //public static T RetrieveOrCreateResource<T>(string soName, string path = null) where T : ScriptableObject
        //{
        //    var allResources = Resources.LoadAll<T>("");
        //    if (allResources.Any()) //Settings Found
        //    {
        //        var resource = allResources[0];
        //        var p = AssetDatabase.GetAssetPath(resource);
        //        if (allResources.Length > 1) //Multiple Settings files
        //        {
        //            Debug.LogWarning($"[Venly SDK] Multiple \'{typeof(T)}\' resources found. (removing all but one)");
        //            foreach (var loadedSettings in allResources)
        //            {
        //                if (resource != loadedSettings)
        //                {
        //                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(loadedSettings));
        //                }
        //            }
        //        }

        //        return resource;
        //    }

        //    if(path != null)
        //    {
        //        VerifyFolder(path);
        //        var resource = ScriptableObject.CreateInstance<T>();
        //        //resource.hideFlags = HideFlags.NotEditable;
        //        AssetDatabase.CreateAsset(resource, $"{path}{soName}.asset");
        //        return resource;
        //    }

        //    return null;
        //}

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