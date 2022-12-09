using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Venly.Editor.Utils;
using Venly.Models;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Venly.Editor.Tools.SDKManager
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

        #region MenuItem

        [MenuItem("Window/Venly/SDK Manager")]
        public static void ShowSdkManager()
        {
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            wnd.titleContent = new GUIContent("Venly SDK Manager");
        }

        [MenuItem("Window/Venly/Force Close Manager")]
        public static void ForceCloseManager()
        {
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            if (wnd != null)
            {
                wnd.Close();
            }
        }

        #endregion
        private AddAndRemoveRequest _packageAddRequest;
        public event Action OnInstallInitiated;
        public event Action OnInstallCompleted;

        private void NotifyInstallDone()
        {
            EditorWindow.GetWindow<SDKManagerView>().RefreshView();
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

        #region MANAGER FUNCTIONS
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
            VenlySettingsEd.Instance.Settings.BackendProvider = backend;
        }

        public void UpdatePackages(string[] packagesAdd, string[] packagesRemove = null)
        {
            OnInstallInitiated?.Invoke();

            //Monitor Process
            EditorApplication.update += OnUpdate;

            _packageAddRequest = Client.AddAndRemove(packagesAdd, packagesRemove);
        }
        #endregion
    }
}