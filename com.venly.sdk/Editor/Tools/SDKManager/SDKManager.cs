using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packages.com.venly.sdk.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using Venly.Core;
using Venly.Editor.Utils;
using Venly.Models.Shared;
using Venly.Utils;

namespace Venly.Editor.Tools.SDKManager
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
            try
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
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to open SDK Manager... (please retry)");
                Debug.LogError(ex);
            }
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
        public static readonly string URL_Discord = @"https://venly.io/discord";
        public static readonly string URL_Guide = @"https://docs.venly.io/venly-gaming-toolkit/";
        #endregion

        #region Events

        //public event Action OnSettingsLoaded;
        public event Action<bool> OnAuthenticatedChanged;
        public event Action OnInitialized;

        #endregion

        private AddAndRemoveRequest _addRemoveRequest = null;


        //Initialization of SDK (including Settings)
        public void Initialize()
        {
            //Authenticate
            IsAuthenticated = false;
            if (VenlySettings.HasCredentials)
            {
                Authenticate()
                    .OnComplete(result =>
                    {
                        IsInitialized = true;
                        OnInitialized?.Invoke();
                    });
            }
            else
            {
                IsInitialized = true;
                OnInitialized?.Invoke();
            }
        }

        internal void ShowLoader(string msg = null)
        {
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            wnd?.ShowLoader(msg);
        }

        internal void HideLoader()
        {
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            wnd?.HideLoader();
        }

        internal bool VerifyAuthentication()
        {
            if (!VenlySettings.HasCredentials)
            {
                if (IsAuthenticated)
                {
                    IsAuthenticated = false;
                    OnAuthenticatedChanged?.Invoke(IsAuthenticated);
                }
            }

            return IsAuthenticated;
        }

        #region MANAGER FUNCTIONS
        //Only called if Authentication succeeded
        internal void UpdateEditorSettings_PostAuthentication(string clientId, string clientSecret,
            VyAccessTokenDto token)
        {
            try
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
            catch (Exception ex)
            {
                VenlyLog.Exception(ex, "SDK Manager >> Post Authentication");
            }
        }

        public VyTask Authenticate()
        {
            IsAuthenticated = false;

            VyException potentialException = null;
            if (!VyEditorData.IsLoaded) potentialException = new VyException("Authentication Failed. Settings not yet loaded!");
            if (!VenlySettings.HasCredentials) potentialException = new VyException("Authentication Failed. Credentials are not available!");
            if (potentialException != null)
            {
                OnAuthenticatedChanged?.Invoke(IsAuthenticated);
                return VyTask.Failed(potentialException);
            }

            return Authenticate(VenlySettings.ClientId, VenlySettings.ClientSecret);
        }

        //SDK Manager Authentication (Sets IsAuthenticated if success)
        private VyTask Authenticate(string clientId, string clientSecret)
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

        private VyTask<string> GetLatestVersion_GithubRelease()
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

        private VyTask<string> GetLatestVersion_Package()
        {
            var taskNotifier = VyTask<string>.Create();

            UnityWebRequest request = UnityWebRequest.Get(VyEditorData.URL_PackageJson);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest().completed += (op) =>
            {
                if (request.isDone && request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var packageInfo = JObject.Parse(request.downloadHandler.text);
                        packageInfo.TryGetValue("version", out var latestVersionToken);
                        
                        if(latestVersionToken == null) taskNotifier.NotifyFail("Failed to retrieve latest SDK version");
                        else taskNotifier.NotifySuccess($"v{latestVersionToken.Value<string>()}");
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

        public VyTask<string> GetLatestVersion()
        {
            var taskNotifier = VyTask<string>.Create();

            taskNotifier.Scope(async () =>
            {
                //Via GitHub Release
                var versionResult = await GetLatestVersion_GithubRelease();
                if (versionResult.Success)
                {
                    taskNotifier.NotifySuccess(versionResult.Data);
                    return;
                }

                //Via Package
                versionResult = await GetLatestVersion_Package();
                if (versionResult.Success)
                {
                    taskNotifier.NotifySuccess(versionResult.Data);
                    return;
                }

                taskNotifier.NotifyFail(versionResult.Exception);
            });

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

            _addRemoveRequest = Client.AddAndRemove(packages.ToArray(), null);
            EditorApplication.update += MonitorUpdateProcess;
            ShowLoader("Updating SDK...");
        }

        private void MonitorUpdateProcess()
        {
            if (_addRemoveRequest?.Status == StatusCode.Failure)
            {
                Debug.LogWarning(_addRemoveRequest?.Error.message);
                _addRemoveRequest = null;
            }

            if (_addRemoveRequest == null)
            {
                EditorApplication.update -= MonitorUpdateProcess;
                HideLoader();
            }
        }
        #endregion
    }
}