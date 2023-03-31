using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Packages.com.venly.sdk.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using VenlySDK.Core;
using VenlySDK.Editor.Utils;
using VenlySDK.Models.Shared;
using VenlySDK.Utils;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace VenlySDK.Editor.Tools.SDKManager
{
    internal class SDKManager
    {
        #region Singleton

        private static SDKManager _instance;

        public static SDKManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SDKManager();
                }

                return _instance;
            }
        }

        #endregion

        #region GIT Helpers

        private class GitReleaseInfo
        {
            public string name;
        }

        #endregion

        #region MenuItem

        [MenuItem("Window/Venly/SDK Manager", priority = 1)]
        public static void ShowSdkManager()
        {
            //Make sure there is no panel open at the moment...
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            if (wnd != null)
            {
                wnd.Close();
            }

            var types = new List<Type>()
            {
                // first add your preferences
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow"),
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow")
            };

            wnd = EditorWindow.GetWindow<SDKManagerView>(types.ToArray());
            wnd.titleContent = new GUIContent("Venly SDK Manager");
        }

#if VENLYSDK_DEBUG
        [MenuItem("Window/Venly/Debug/Force Close Manager", priority = -1)]
        public static void ForceCloseManager()
        {
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            if (wnd != null)
            {
                wnd.Close();
            }
        }
#endif

#endregion

#region Properties

        public bool IsInitialized { get; private set; }
        public bool IsAuthenticated { get; private set; }
        //public bool SettingsLoaded { get; private set; }

        public static readonly string URL_GitHubIssues = @"https://github.com/ArkaneNetwork/Unity-SDK/issues";
        public static readonly string URL_ChangeLog = @"https://github.com/ArkaneNetwork/Unity-SDK/releases";
        public static readonly string URL_Discord = @"https://discord.gg/rKUFbUWMaw";
        public static readonly string URL_Guide = @"https://docs.venly.io/venly-unity-sdk/";

        #endregion

        #region Events

        //public event Action OnSettingsLoaded;
        public event Action<bool> OnAuthenticatedChanged;
        public event Action OnInitialized;

#endregion


        //Initialization of SDK (including Settings)
        public async void Initialize()
        {
            //Thread Context
            VyTaskBase.Initialize();

            ////Load Settings
            //SettingsLoaded = false;
            //VenlyEditorSettings.Instance.LoadSettings();
            //if (VenlyEditorSettings.Instance.SettingsLoaded)
            //{
            //    SettingsLoaded = true;
            //    UpdateEditorSettings();
            //    OnSettingsLoaded?.Invoke();
            //}
            //else
            //{
            //    throw new VyException("An error occurred while initializing the SDK Manager (Load Settings)");
            //}

            //Authenticate
            IsAuthenticated = false;
            if (VenlySettings.HasCredentials)
            {
                await Authenticate().AwaitResult();
            }

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

#region MANAGER FUNCTIONS
        //private void ResetSettings()
        //{
        //    var performReset = true;
        //    if (EditorPrefs.HasKey("com.venly.sdk.last_settings_reset"))
        //    {
        //        var lastResetVersion = EditorPrefs.GetString("com.venly.sdk.last_settings_reset");
        //        performReset = lastResetVersion != EditorSettings.Version;
        //    }

        //    if (!performReset) return;

        //    //Editor Settings Reset
        //    EditorSettings.SupportedChainsNft = Array.Empty<eVyChain>();
        //    EditorSettings.SupportedChainsWallet = Array.Empty<eVyChain>();

        //    EditorPrefs.SetString("com.venly.sdk.last_settings_reset", EditorSettings.Version);
        //}

        //internal void UpdateVenlySettings(VenlySettingsSO settingsSo)
        //{
        //    //Verify Selected Backend
        //    settingsSo.BackendProvider = GetConfiguredBackend();
        //}

        //internal void UpdateEditorSettings()
        //{
        //    //Get Package Information
        //    _packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
        //    EditorSettings.Version = $"v{_packageInfo.version}";

        //    //Check if some Settings need to be reset (after new version for example)
        //    ResetSettings();

        //    //Public Resource Root
        //    if (string.IsNullOrEmpty(EditorSettings.PublicResourceRoot))
        //        EditorSettings.PublicResourceRoot = DefaultPublicResourceRoot;

        //    //Verify Selected Backend
        //    EditorSettings.SDKManager.SelectedBackend = GetConfiguredBackend();
        //    EditorSettings.SDKManager.UnappliedSettings = false;
        //}

        //Only called if Authentication succeeded
        internal void UpdateEditorSettings_PostAuthentication(string clientId, string clientSecret, VyAccessTokenDto token)
        {
            //VenlySettings
            VenlySettings.SetCredentials(clientId, clientSecret);

            //JWT Decoding
            var jwtInfo = VenlyUtils.JWT.ExtractInfoFromToken(token);
            VenlySettings.SetAccessAndEnvironment(jwtInfo);

            //Current ClientId
            VyEditorData.EditorSettings.SDKManager.CurrentClientId = clientId;
            VyEditorData.SaveEditorSetting();

            //Refresh SupportedChainsWallet (ASYNC)
            if (VyEditorData.EditorSettings.SupportedChainsWallet == null
                || VyEditorData.EditorSettings.SupportedChainsWallet.Length == 0)
            {
                VenlyEditorAPI.GetChainsWALLET()
                    .OnSuccess(chains =>
                    {
                        VyEditorData.EditorSettings.SupportedChainsWallet = chains;
                        VyEditorData.SaveEditorSetting();
                    });
            }

            //Refresh SupportedChainsNft (ASYNC)
            if (VyEditorData.EditorSettings.SupportedChainsNft == null
                || VyEditorData.EditorSettings.SupportedChainsNft.Length == 0)
            {
                VenlyEditorAPI.GetChainsNFT()
                    .OnSuccess(chains =>
                    {
                        VyEditorData.EditorSettings.SupportedChainsNft = chains;
                        VyEditorData.SaveEditorSetting();
                    });
            }
        }

        public VyTask Authenticate()
        {
            //if (!IsInitialized) VyTask.Failed(new VyException("Authentication Failed. SDK Manager not yet initialized!"));
            if(!VyEditorData.IsLoaded) return VyTask.Failed(new VyException("Authentication Failed. Settings not yet loaded!"));
            if (!VenlySettings.HasCredentials) return VyTask.Failed(new VyException("Authentication Failed. Credentials are not available!"));

            return Authenticate(VenlySettings.ClientId, VenlySettings.ClientSecret);
        }

        //SDK Manager Authentication (Sets IsAuthenticated if success)
        public VyTask Authenticate(string clientId, string clientSecret)
        {
            IsAuthenticated = false;

            VyEditorData.EditorSettings.SDKManager.CurrentClientId = null;
            //VenlyEditorSettings.Instance.EditorData.SDKManager.CurrentClientId = null;

            var taskNotifier = VyTask.Create("SDKManager_Authenticate");

            VenlyEditorAPI.GetAccessToken(clientId, clientSecret)
                .OnComplete(result =>
                {
                    IsAuthenticated = result.Success;
                    if (IsAuthenticated)
                    {
                        UpdateEditorSettings_PostAuthentication(clientId, clientSecret, result.Data);
                    }
                    
                    //Debug.Log($"[SDK MANAGER] Authentication Completed. IsAuthenticated={IsAuthenticated}");

                    OnAuthenticatedChanged?.Invoke(IsAuthenticated);
                    taskNotifier.Notify(result);
                });

            return taskNotifier.Task;
        }

        public VyTask<string> GetLatestVersion()
        {
            var taskNotifier = VyTask<string>.Create();

            UnityWebRequest request = UnityWebRequest.Get(VyEditorData.URL_GitReleases);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest().completed += (op) =>
            {
                if (request.isDone && request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var gitInfos = JsonConvert.DeserializeObject<GitReleaseInfo[]>(request.downloadHandler.text);
                        var latestVersion = VenlyEditorUtils.GetLatestSemVer(gitInfos?.Select(gi => gi.name).ToList());

                        if (string.IsNullOrEmpty(latestVersion)) taskNotifier.NotifyFail("Latest version not found");
                        else taskNotifier.NotifySuccess(latestVersion);
                    }
                    catch (Exception ex)
                    {
                        taskNotifier.NotifyFail(ex);
                    }
                }
                else
                {
                    taskNotifier.NotifyFail("Failed to retrieve SDK release list");
                }
            };

            return taskNotifier.Task;
        }
        
        public void UpdateSDK(string targetVersion)
        {
            //Legacy...
            //VenlySDKUpdater.Instance.UpdateSDK(targetVersion);

            //Prepare for Update
            VenlyEditorUtils.StoreBackup(VyEditorData.EditorSettings);
            VenlyEditorUtils.StoreBackup(VyEditorData.RuntimeSettings);

            //Update Link
            var packages = new List<string>();

            //Venly SDK
            packages.Add($"{VyEditorData.URL_GitRepository}#{targetVersion}");

            Client.AddAndRemove(packages.ToArray(), null);
        }
#endregion
    }
}