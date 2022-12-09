using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Venly.Editor.Tools.SDKManager
{
    public class VenlySDKManager
    {
        #region Singleton

        private static VenlySDKManager _instance;

        public static VenlySDKManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VenlySDKManager();
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        #endregion

        #region MenuItem

        [MenuItem("Window/Venly/SDK Manager")]
        public static void ShowSdkManager()
        {
            ManagerView wnd = EditorWindow.GetWindow<ManagerView>();
            wnd.titleContent = new GUIContent("Venly SDK Manager");
        }

        #endregion

        public VenlyManagerSO Settings { get; private set; }

        private const string _managerPackageRoot = "Packages\\com.venly.sdkmanager\\";

        private AddAndRemoveRequest _packageAddRequest;
        public event Action OnInstallInitiated;
        public event Action OnInstallCompleted;

        [InitializeOnLoadMethod]
        public static void OnLoaded()
        {
            Instance.VerifySDK();
        }

        public void VerifySDK()
        {

        }

        #region Initialize Manager

        private void Initialize()
        {
            LoadSettings();
            VerifySettings();
        }

        private void LoadSettings()
        {
            Settings = AssetDatabase.LoadAssetAtPath<VenlyManagerSO>("Assets\\VenlyManagerSettings.asset");

            if (Settings == null)
            {
                Settings = ScriptableObject.CreateInstance<VenlyManagerSO>();
                Settings.hideFlags = HideFlags.NotEditable;
                AssetDatabase.CreateAsset(Settings, "Assets\\VenlyManagerSettings.asset");
            }

            Settings.hideFlags = HideFlags.NotEditable;
        }

        private Version ParseSemVer(string version)
        {
            version = version.Replace("v", "");
            version = version.Split('-')[0];
            return Version.Parse(version);
        }

        private void VerifySettings()
        {
            var allPackages = PackageInfo.GetAllRegisteredPackages();
            var sdkPackage = allPackages.FirstOrDefault(p => p.name.Equals("com.venly.sdk"));

            Settings.SdkVersionStr = null;
            Settings.ManagerPackageRoot = _managerPackageRoot;
            Settings.IsSdkInstalled = sdkPackage != null;

            if (Settings.IsSdkInstalled)
            {
                Settings.SdkVersionStr = sdkPackage.version;
                Settings.SdkVersion = ParseSemVer(Settings.SdkVersionStr);
            }

            //Temps
            if (!Settings.IsSdkInstalled)
            {
                Settings.IsSdkInstalled = AssetDatabase.IsValidFolder("Packages\\com.venly.sdk\\");
                if (Settings.IsSdkInstalled)
                {
                    Settings.SdkVersionStr = "v0.1.1-alpha";
                    Settings.SdkVersion = ParseSemVer(Settings.SdkVersionStr);
                }
            }

            AssetDatabase.SaveAssetIfDirty(Settings);
        }

        #endregion

        #region Helpers

        public static T LoadManagerAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>($"{_managerPackageRoot}{assetPath}");
        }

        #endregion

        public void UpdatePackages(string[] packagesAdd, string[] packagesRemove = null)
        {
            OnInstallInitiated?.Invoke();

            //Monitor Process
            EditorApplication.update += OnUpdate;

            _packageAddRequest = Client.AddAndRemove(packagesAdd, packagesRemove);
        }

        public void InstallSDK(string version)
        {
            OnInstallInitiated?.Invoke();

            //Monitor Process
            EditorApplication.update += OnUpdate;

            _packageAddRequest = Client.AddAndRemove(new[]
            {
                $"git+https://github.com/TimCassell/ProtoPromise.git?path=ProtoPromise_Unity/Assets/Plugins/ProtoPromise#v2.3.0",
                $"git+https://github.com/Tomiha/UnityGit?path=com.venly.sdk#{version}"
            });
        }

        private void NotifyInstallDone()
        {
            VerifySettings();
            EditorWindow.GetWindow<ManagerView>().RefreshPanels();

            OnInstallCompleted?.Invoke();
        }

        private void OnUpdate()
        {
            if (_packageAddRequest.IsCompleted)
            {
                EditorApplication.update -= OnUpdate;

                if (_packageAddRequest.Status == StatusCode.Failure)
                {
                    Debug.LogException(new Exception($"Package Install Failed >> {_packageAddRequest.Error.message}"));
                }

                NotifyInstallDone();
            }

            //    foreach (var request in _packageAddRequests)
            //    {
            //        if (request.Status == StatusCode.InProgress) continue;
            //        if (request.Status == StatusCode.Failure)
            //        {
            //            EditorApplication.update -= OnUpdate;
            //            _installSdkTask.SetException(new Exception(request.Error.message));
            //        }
            //    }

            //    if (_packageAddRequests.All(r => r.Status == StatusCode.Success))
            //    {
            //        EditorApplication.update -= OnUpdate;
            //        _installSdkTask.SetResult(true);
            //    }
            //}
            //else
            //{
            //    EditorApplication.update -= OnUpdate;
            //    _installSdkTask.SetResult(false);
            //}
        }
    }
}