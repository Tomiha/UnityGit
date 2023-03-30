using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        private VenlyEditorDataSO.SDKManagerData _managerData => VenlyEditorSettings.Instance.EditorData.SDKManager;

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
        public bool SettingsLoaded { get; private set; }

        public VenlyEditorDataSO EditorSettings => VenlyEditorSettings.Instance.EditorData;

        public static readonly string URL_GitHubIssues = @"https://github.com/ArkaneNetwork/Unity-SDK/issues";
        public static readonly string URL_ChangeLog = @"https://github.com/ArkaneNetwork/Unity-SDK/releases";
        public static readonly string URL_Discord = @"https://discord.gg/rKUFbUWMaw";
        public static readonly string URL_Guide = @"https://docs.venly.io/venly-unity-sdk/";

        private PackageInfo _packageInfo;
        public static readonly string DefaultPublicResourceRoot = "Assets\\Resources\\";
        public static readonly string SdkPackageRoot = "Packages\\com.venly.sdk\\";

#if VENLY_STAGING_REPO
        public static readonly string URL_GitRepository = @"git+https://github.com/Tomiha/UnityGit.git?path=com.venly.sdk";
        public static readonly string URL_GitReleases = @"https://api.github.com/repos/Tomiha/UnityGit/releases";
#else
        public static readonly string URL_GitRepository = @"git+https://github.com/ArkaneNetwork/Unity-SDK.git?path=Packages/com.venly.sdk";
        public static readonly string URL_GitReleases = @"https://api.github.com/repos/ArkaneNetwork/Unity-SDK/releases";
#endif

#endregion

        #region Events

        public event Action OnSettingsLoaded;
        public event Action<bool> OnAuthenticatedChanged;
        public event Action OnInitialized;

#endregion

#if VENLYSDK_DEBUG
        [MenuItem("Window/Venly/Debug/Force SDK Manager Init", priority = -1)]
#endif
        [InitializeOnLoadMethod]
        static void InitializeStatic()
        {
            //Load EditorSettings
            VenlyEditorSettings.Load();

            //Initialize the SDK
            //******************
            SDKManager.Instance.Initialize();
        }

        //Initialization of SDK (including Settings)
        private async void Initialize()
        {
            //Thread Context
            VyTaskBase.Initialize();

            //Load Settings
            SettingsLoaded = false;
            VenlyEditorSettings.Instance.LoadSettings();
            if (VenlyEditorSettings.Instance.SettingsLoaded)
            {
                SettingsLoaded = true;
                UpdateEditorSettings();
                OnSettingsLoaded?.Invoke();
            }
            else
            {
                throw new VyException("An error occurred while initializing the SDK Manager (Load Settings)");
            }

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
        private void ResetSettings()
        {
            var performReset = true;
            if (EditorPrefs.HasKey("com.venly.sdk.last_settings_reset"))
            {
                var lastResetVersion = EditorPrefs.GetString("com.venly.sdk.last_settings_reset");
                performReset = lastResetVersion != EditorSettings.Version;
            }

            if (!performReset) return;

            //Editor Settings Reset
            EditorSettings.SupportedChainsNft = Array.Empty<eVyChain>();
            EditorSettings.SupportedChainsWallet = Array.Empty<eVyChain>();

            EditorPrefs.SetString("com.venly.sdk.last_settings_reset", EditorSettings.Version);
        }

        internal void UpdateVenlySettings(VenlySettingsSO settingsSo)
        {
            //Verify Selected Backend
            settingsSo.BackendProvider = GetConfiguredBackend();
        }

        internal void UpdateEditorSettings()
        {
            //Get Package Information
            _packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            EditorSettings.Version = $"v{_packageInfo.version}";

            //Check if some Settings need to be reset (after new version for example)
            ResetSettings();

            //Public Resource Root
            if (string.IsNullOrEmpty(EditorSettings.PublicResourceRoot))
                EditorSettings.PublicResourceRoot = DefaultPublicResourceRoot;

            //Verify Selected Backend
            EditorSettings.SDKManager.SelectedBackend = GetConfiguredBackend();
            EditorSettings.SDKManager.UnappliedSettings = false;
        }

        //Only called if Authentication succeeded
        internal void UpdateEditorSettings_PostAuthentication(string clientId, string clientSecret, VyAccessTokenDto token)
        {
            //VenlySettings
            VenlySettings.SetCredentials(clientId, clientSecret);

            //JWT Decoding
            var jwtInfo = VenlyUtils.JWT.ExtractInfoFromToken(token);
            VenlySettings.SetAccessAndEnvironment(jwtInfo);

            //Current ClientId
            EditorSettings.SDKManager.CurrentClientId = clientId;
            AssetDatabase.SaveAssetIfDirty(EditorSettings);

            //Refresh SupportedChainsWallet (ASYNC)
            if (EditorSettings.SupportedChainsWallet == null
                || EditorSettings.SupportedChainsWallet.Length == 0)
            {
                VenlyEditorAPI.GetChainsWALLET()
                    .OnSuccess(chains =>
                    {
                        EditorSettings.SupportedChainsWallet = chains;
                        AssetDatabase.SaveAssetIfDirty(EditorSettings);
                    });
            }

            //Refresh SupportedChainsNft (ASYNC)
            if (EditorSettings.SupportedChainsNft == null
                || EditorSettings.SupportedChainsNft.Length == 0)
            {
                VenlyEditorAPI.GetChainsNFT()
                    .OnSuccess(chains =>
                    {
                        EditorSettings.SupportedChainsNft = chains;
                        AssetDatabase.SaveAssetIfDirty(EditorSettings);
                    });
            }
        }

        public VyTask Authenticate()
        {
            //if (!IsInitialized) VyTask.Failed(new VyException("Authentication Failed. SDK Manager not yet initialized!"));
            if(!SettingsLoaded) return VyTask.Failed(new VyException("Authentication Failed. Settings not yet loaded!"));
            if (!VenlySettings.HasCredentials) return VyTask.Failed(new VyException("Authentication Failed. Credentials are not available!"));

            return Authenticate(VenlySettings.ClientId, VenlySettings.ClientSecret);
        }

        //SDK Manager Authentication (Sets IsAuthenticated if success)
        public VyTask Authenticate(string clientId, string clientSecret)
        {
            IsAuthenticated = false;
            VenlyEditorSettings.Instance.EditorData.SDKManager.CurrentClientId = null;

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

        private static eVyBackendProvider GetConfiguredBackend()
        {
            var selectedBackend = eVyBackendProvider.DevMode;

            var currBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(currBuildTarget, out var currentDefines);

            //Search
            var definesList = currentDefines.ToList();
            definesList.RemoveAll(define => !define.Contains("_VENLY_"));

            if (definesList.Count > 0)
            {
                if (definesList.Any(define => define.EndsWith("DEVMODE"))) selectedBackend = eVyBackendProvider.DevMode;
                else if(definesList.Any(define => define.EndsWith("PLAYFAB"))) selectedBackend = eVyBackendProvider.PlayFab;
                else if (definesList.Any(define => define.EndsWith("CUSTOM"))) selectedBackend = eVyBackendProvider.Custom;
                else ConfigureForBackend(selectedBackend);
            }
            else ConfigureForBackend(selectedBackend);

            return selectedBackend;
        }

        [MenuItem("Window/Venly/Update Scripting Defines", priority = 3)]
        public static void UpdateDefines()
        {
            var defines = GenerateDefinesForProvider(VenlySettings.BackendProvider);
            var targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            ApplyDefines(defines, target);
        }

        private static void ApplyDefines(List<string> defines, NamedBuildTarget target)
        {
            PlayerSettings.GetScriptingDefineSymbols(target, out var currentDefines);
            var definesList = currentDefines.ToList();

            //REMOVE PREVIOUS
            definesList.RemoveAll(d => d.Contains("VENLY_"));

            //ADD NEW ONES
            definesList.AddRange(defines);

            PlayerSettings.SetScriptingDefineSymbols(target, definesList.ToArray());
        }

        private static List<string> GenerateDefinesForProvider(eVyBackendProvider backend)
        {
            var list =  new List<string>
            {
                //"VENLY_API_UNITY",
                $"ENABLE_VENLY_{backend.GetMemberName().ToUpper()}"
            };

#if UNITY_EDITOR
            if (backend == eVyBackendProvider.DevMode)
            {
                list.Add("ENABLE_VENLY_API_SERVER");
            }
#endif

            return list;
        }

        public static void ConfigureForBackend(eVyBackendProvider backend)
        {
            //Generate Defines
            var definesList = GenerateDefinesForProvider(backend);

            //Apply to BuildTargets
            ApplyDefines(definesList, NamedBuildTarget.Standalone);
            ApplyDefines(definesList, NamedBuildTarget.WebGL);
            ApplyDefines(definesList, NamedBuildTarget.iOS);
            ApplyDefines(definesList, NamedBuildTarget.Android);
            ApplyDefines(definesList, NamedBuildTarget.WindowsStoreApps);

            //SET BACKEND
            VenlyEditorSettings.Instance.Settings.BackendProvider = backend;
        }

        public VyTask<string> GetLatestVersion()
        {
            var taskNotifier = VyTask<string>.Create();

            UnityWebRequest request = UnityWebRequest.Get(URL_GitReleases);
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
            VenlySDKUpdater.Instance.UpdateSDK(targetVersion);

            //Prepare for Update
            VenlyEditorUtils.StoreBackup(VenlyEditorSettings.Instance.Settings);
            VenlyEditorUtils.StoreBackup(VenlyEditorSettings.Instance.EditorData);

            //Update Link
            var packages = new List<string>();

            //Venly SDK
            packages.Add($"{URL_GitRepository}#{targetVersion}");

            Client.AddAndRemove(packages.ToArray(), null);
        }
#endregion
    }
}