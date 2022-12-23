using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Proto.Promises;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using VenlySDK.Core;
using VenlySDK.Editor.Utils;
using VenlySDK.Models;

namespace VenlySDK.Editor.Tools.SDKManager
{
    public class SDKManager
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

        [MenuItem("Window/Venly/SDK Manager")]
        public static void ShowSdkManager()
        {
            var types = new List<Type>()
            {
                // first add your preferences
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow"),
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow")
            };

            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>(types.ToArray());
            wnd.titleContent = new GUIContent("Venly SDK Manager");
        }

        //[MenuItem("Window/Venly/Force Close Manager")]
        //public static void ForceCloseManager()
        //{
        //    SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
        //    if (wnd != null)
        //    {
        //        wnd.Close();
        //    }
        //}

        #endregion

        #region Properties

        public bool IsInitialized { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public bool SettingsLoaded { get; private set; }

        public static readonly string URL_GitHubIssues = @"https://github.com/ArkaneNetwork/Unity-SDK/issues";
        public static readonly string URL_ChangeLog = @"https://github.com/ArkaneNetwork/Unity-SDK/releases";
        public static readonly string URL_Discord = @"https://www.venly.io";
        public static readonly string URL_Guide = @"https://docs.venly.io/venly-unity-sdk/";

        //public static readonly string URL_GitRepository = @"git+https://github.com/ArkaneNetwork/Unity-SDK.git?path=Packages/com.venly.sdk";
        //public static readonly string URL_GitReleases = @"https://github.com/ArkaneNetwork/Unity-SDK/releases";

        public static readonly string URL_GitRepository = @"git+https://github.com/Tomiha/UnityGit.git?path=com.venly.sdk";
        public static readonly string URL_GitReleases = @"https://api.github.com/repos/Tomiha/UnityGit/releases";

        #endregion

        #region Events

        public event Action OnSettingsLoaded;
        public event Action<bool> OnAuthenticatedChanged;
        public event Action OnInitialized;

        #endregion

        [InitializeOnLoadMethod]
        static void InitializeStatic()
        {
            //Initialize the SDK
            //******************
            SDKManager.Instance.Initialize();
        }

        private async void Initialize()
        {
            //Thread Context
            Promise.Config.ForegroundContext = SynchronizationContext.Current;

            //Load Settings
            SettingsLoaded = false;
            VenlyEditorSettings.Instance.LoadSettings();
            if (VenlyEditorSettings.Instance.SettingsLoaded)
            {
                SettingsLoaded = true;
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
                await Authenticate().AwaitResult(false);
            }

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        #region MANAGER FUNCTIONS
        public VyTask Authenticate()
        {
            //if (!IsInitialized) VyTask.Failed(new VyException("Authentication Failed. SDK Manager not yet initialized!"));
            if(!SettingsLoaded) return VyTask.Failed(new VyException("Authentication Failed. Settings not yet loaded!"));
            if (!VenlySettings.HasCredentials) return VyTask.Failed(new VyException("Authentication Failed. Credentials are not available!"));

            return Authenticate(VenlySettings.ClientId, VenlySettings.ClientSecret);
        }

        public VyTask Authenticate(string clientId, string clientSecret)
        {
            IsAuthenticated = false;
            VenlyEditorSettings.Instance.EditorData.SDKManager.CurrentClientId = null;

            var taskNotifier = VyTask.Create();

            VenlyEditorAPI.GetAccessToken(clientId, clientSecret)
                .OnComplete(result =>
                {
                    IsAuthenticated = result.Success;
                    if (IsAuthenticated)
                    {
                        VenlyEditorSettings.Instance.EditorData.SDKManager.CurrentClientId = clientId;
                    }

                    OnAuthenticatedChanged?.Invoke(IsAuthenticated);
                    taskNotifier.Notify(result.ToVoidResult());
                });

            return taskNotifier.Task;
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
                    var gitInfos = JsonConvert.DeserializeObject<GitReleaseInfo[]>(request.downloadHandler.text);
                    var latestVersion = VenlyEditorUtils.GetLatestSemVer(gitInfos?.Select(gi => gi.name).ToList());

                    if(string.IsNullOrEmpty(latestVersion)) taskNotifier.NotifyFail("Latest version not found");
                    else taskNotifier.NotifySuccess(latestVersion);
                }
                else
                {
                    Debug.LogWarning("[Venly SDK] Failed to retrieve SDK release list.");
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