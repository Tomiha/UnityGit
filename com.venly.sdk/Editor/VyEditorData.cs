using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Venly.Core;
using Venly.Editor.Tools.SDKManager;
using Venly.Editor.Utils;
using Venly.Models.Shared;
using Venly.Utils;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Packages.com.venly.sdk.Editor
{
    internal static class VyEditorData
    {
        #region CONSTANTS
        //Constants
        public const string DefaultResourcePath = "Assets\\Resources\\";
#if VENLY_STAGING_REPO
        public const string URL_GitRepository = @"git+https://github.com/Tomiha/UnityGit.git?path=com.venly.sdk";
        public const string URL_GitReleases = @"https://api.github.com/repos/Tomiha/UnityGit/releases";
#else
        public const string URL_GitRepository = @"git+https://github.com/ArkaneNetwork/Unity-SDK.git?path=Packages/com.venly.sdk";
        public const string URL_GitReleases = @"https://api.github.com/repos/ArkaneNetwork/Unity-SDK/releases";
#endif
        #endregion

        #region PUBLICS
        public static VenlyEditorDataSO EditorSettings
        {
            get
            {
                if (_editorSettingsSO == null)
                    LoadSettings();

                if (_editorSettingsSO == null)
                    throw new NullReferenceException("[VyEditorData] EditorSettings failed to load (NULL)");

                return _editorSettingsSO;
            }
        }

        public static VenlySettingsSO RuntimeSettings
        {
            get
            {
                if (_runtimeSettingsSO == null)
                    LoadSettings();

                if (_runtimeSettingsSO == null)
                    throw new NullReferenceException("[VyEditorData] RuntimeSettings failed to load (NULL)");

                return _runtimeSettingsSO;
            }
        }

        public static SerializedObject SerializedRuntimeSettings
        {
            get
            {
                if (_serializedRuntimeSettings == null)
                    LoadSettings();

                if (_serializedRuntimeSettings == null)
                    throw new NullReferenceException("[VyEditorData] SerializedRuntimeSettings failed to load (NULL)");

                return _serializedRuntimeSettings;
            }
        }

        public static bool IsLoaded { get; private set; }
        public static event Action OnLoaded;
        #endregion

        private static VenlyEditorDataSO _editorSettingsSO;
        private static VenlySettingsSO _runtimeSettingsSO;
        private static SerializedObject _serializedRuntimeSettings;
        private static PackageInfo _packageInfo;

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
#if VENLYSDK_DEBUG
            VenlyLog.ToggleLogLevel(VenlyLog.eVyLogLevel.Debug, true);
            VenlyLog.OnLog += (logData) =>
            {
                if (logData.level == VenlyLog.eVyLogLevel.Exception) Debug.LogException(logData.exception);
                else Debug.Log(logData.message);
            };
#endif

            //0. Set Foreground TaskScheduler
            VyTaskBase.Initialize(TaskScheduler.FromCurrentSynchronizationContext());

            //1. Load All Settings
            LoadSettings();
            VerifyVersion(); //Double Check version (can be different if just update and settings were still in project from previous version)

            //2. Initialize SDK Manager
            SDKManager.Instance.Initialize();
        }

#if VENLYSDK_DEBUG
        [MenuItem("Window/Venly/Debug/Reload SDK Manager", priority = -1)]
#endif
        public static void Reload()
        {
            LoadSettings(true);
            SDKManager.Instance.Initialize();
        }

        public static void SaveAll()
        {
            LoadSettings();
            SaveEditorSetting();
            SaveRuntimeSettings();
        }

        public static void SaveEditorSetting()
        {
            if (_editorSettingsSO == null)
                throw new NullReferenceException("[VyEditorData] SaveEditorSetting failed, setting NULL");

            AssetDatabase.SaveAssetIfDirty(_editorSettingsSO);
        }

        public static void SaveRuntimeSettings()
        {
            if (_runtimeSettingsSO == null)
                throw new NullReferenceException("[VyEditorData] SaveRuntimeSettings failed, setting NULL");

            AssetDatabase.SaveAssetIfDirty(_runtimeSettingsSO);
        }

        public static bool VerifyIsLoaded()
        {
            if (IsLoaded)
            {
                if (_runtimeSettingsSO == null || _editorSettingsSO == null)
                {
                    LoadSettings();
                }
            }

            return IsLoaded;
        }

        private static void LoadSettings(bool force = false)
        {
            IsLoaded = false;
            var isDirty = false; //Track if any of the settings references are updated

            #region EDITOR SETTINGS
            //EDITOR SETTINGS
            //+++++++++++++++
            if (_editorSettingsSO == null || force)
            {
                isDirty = true;
                _editorSettingsSO = RetrieveOrCreateResource<VenlyEditorDataSO>("VenlyEditorData", DefaultResourcePath);

                #region Update Settings
                //Package Version
                _packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
                _editorSettingsSO.Version = $"v{_packageInfo.version}";
                _editorSettingsSO.PublicResourceRoot = DefaultResourcePath;

                //SDK Manager (set to current state)
                _editorSettingsSO.SDKManager.SelectedBackend = GetActiveProviderType();
                _editorSettingsSO.SDKManager.UnappliedSettings = false;

                #endregion

                _editorSettingsSO.hideFlags = HideFlags.NotEditable;
                EditorUtility.SetDirty(_editorSettingsSO);
                AssetDatabase.SaveAssetIfDirty(_editorSettingsSO);
            }


            #endregion

            #region RUNTIME SETTINGS
            //RUNTIME SETTINGS
            //++++++++++++++++
            if (_runtimeSettingsSO == null || force)
            {
                isDirty = true;
                _runtimeSettingsSO = RetrieveOrCreateResource<VenlySettingsSO>("VenlySettings", DefaultResourcePath);

                //Update Settings
                _runtimeSettingsSO.BackendProvider = GetActiveProviderType();
                _runtimeSettingsSO.PublicResourceRoot = DefaultResourcePath;

                //Check if Provider is NONE, force to DevMode in that case
                if (_runtimeSettingsSO.BackendProvider == eVyBackendProvider.None)
                    ConfigureForBackend(eVyBackendProvider.DevMode); //Force to DevMode

                _runtimeSettingsSO.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(_runtimeSettingsSO);
                AssetDatabase.SaveAssetIfDirty(_runtimeSettingsSO);

                //Get Serialized Settings
                _serializedRuntimeSettings = new SerializedObject(_runtimeSettingsSO);
            }

            //Update VenlySettings
            VenlySettings.Load(_runtimeSettingsSO);

            #endregion

            //Set Loaded Flag
            IsLoaded = _runtimeSettingsSO != null && _editorSettingsSO != null;
            if (IsLoaded && isDirty) OnLoaded?.Invoke();
        }

        private static void VerifyVersion()
        {
            _packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            var currVersion = $"v{_packageInfo.version}";

            if (currVersion != _editorSettingsSO.Version)
            {
                _editorSettingsSO.Version = currVersion;
                EditorUtility.SetDirty(_editorSettingsSO);
                AssetDatabase.SaveAssetIfDirty(_editorSettingsSO);
            }
        }

        #region Scripting Defines Helpers
        [MenuItem("Window/Venly/Update Scripting Defines", priority = 3)]
        private static void UpdateDefines()
        {
            var defines = GenerateDefinesForProvider(VenlySettings.BackendProvider);
            var targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            ApplyDefines(defines, target);
        }

        public static eVyBackendProvider GetActiveProviderType()
        {
            var currBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(currBuildTarget, out var currDefinesArr);

            //Search
            var currDefinesList = currDefinesArr.ToList();
            currDefinesList.RemoveAll(def => !def.Contains("ENABLE_VENLY_"));

            //Possible Providers
            var providerList = Enum.GetNames(typeof(eVyBackendProvider))
                .Select(enumName => Enum.Parse<eVyBackendProvider>(enumName)).ToList();

            foreach (var providerType in providerList)
            {
                var providerName = providerType.GetMemberName();
                if (currDefinesList.Any(def => def.EndsWith(providerName)))
                    return providerType;
            }

            return eVyBackendProvider.None;
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
            var list = new List<string>
            {
                //"VENLY_API_UNITY",
                $"ENABLE_VENLY_{backend.GetMemberName().ToUpper()}"
            };

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
            if (_runtimeSettingsSO == null)
            {
                Debug.LogWarning("[VENLY] Failed to store current Provider to settings. (settings NULL)");
                return;
            }

            _runtimeSettingsSO.BackendProvider = backend;
        }
        #endregion

        #region Helpers
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

            if (path != null)
            {
                VerifyFolder(path);
                var resource = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(resource, $"{path}{soName}.asset");
                return resource;
            }

            return null;
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
        #endregion
    }
}
