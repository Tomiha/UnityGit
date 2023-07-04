using System;
using System.Threading.Tasks;
using UnityEngine;
using Venly.Backends;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Utils;

namespace Venly
{
    public static class VenlyUnity
    {
        public static VyTask Initialize(eVyBackendProvider backendProviderType, eVyEnvironment env)
        {
            return Initialize(backendProviderType.GetMemberName(), env);
        }

        public static VyTask Initialize(IVyBackendExtension extensionsOverride = null)
        {
            var providerType = "";
            switch (VenlySettings.BackendProvider)
            {
                case eVyBackendProvider.DevMode:
                case eVyBackendProvider.PlayFab:
                case eVyBackendProvider.Beamable:
                    providerType = VenlySettings.BackendProvider.GetMemberName();
                    break;

                case eVyBackendProvider.Custom:
                    providerType = VenlySettings.CustomBackendSettings.CustomType;
                    break;

                default:
                    return VyTask.Failed(new NotImplementedException($"Unknown BackendProvider \'{VenlySettings.BackendProvider}\'"));
            }

            return Initialize(providerType, VenlySettings.Environment);
        }

        private static VyTask Initialize(string providerType, eVyEnvironment env)
        {
            //Setup Logger
            VenlyLog.OnLog += (logData) =>
            {
                switch (logData.level)
                {
                    case VenlyLog.eVyLogLevel.Debug:
                    case VenlyLog.eVyLogLevel.Info:
                        Debug.Log(logData.message);
                        break;
                    case VenlyLog.eVyLogLevel.Warning:
                        Debug.LogWarning(logData.message);
                        break;
                    case VenlyLog.eVyLogLevel.Exception:
                        Debug.LogException(logData.exception);
                        break;
                }
            };

#if UNITY_EDITOR
            if (VenlySettings.PrintRemoteApiInfo)
            {
                if (providerType != eVyBackendProvider.DevMode.GetMemberName() &&
                    providerType != eVyBackendProvider.None.GetMemberName())
                {
                    VenlyAPI.OnRemoteApiInfo += (info, err) =>
                    {
                        if (string.IsNullOrEmpty(err))
                        {
                            VenlyLog.Info(
                                $"[Venly - Remote API Version] Version={info.CoreVersion} | Environment={info.ActiveEnvironment}");
                        }
                        else VenlyLog.Warning($"[Venly - Remote API Version] {err}");
                    };
                }
            }
#endif

#if UNITY_EDITOR
            //var taskNotifier = VyTask.Create();

            //taskNotifier.Scope(async () =>
            //{
            //    //Initialize API
            //    await VenlyAPI.Initialize(providerType, env, TaskScheduler.FromCurrentSynchronizationContext()).AwaitResult();

            //    //Check Backend Version
            //    if (providerType != eVyBackendProvider.DevMode.GetMemberName())
            //    {
            //        var infoResult = await VenlyAPI.ProviderExtensions.GetServerInfo();
            //        if (infoResult.Success)
            //        {
            //            //Verify
            //        }
            //        else
            //        {
            //            VenlyLog.Warning("Failed to verify remote API version. (Make sure you are using the correct \'Venly Core API\' version for your backend implementation)");
            //        }
            //    }
            //});

            //return taskNotifier.Task;

            //Initialize API
            return VenlyAPI.Initialize(providerType, env, TaskScheduler.FromCurrentSynchronizationContext());
#else
            //Initialize API
            return VenlyAPI.Initialize(providerType, env, TaskScheduler.FromCurrentSynchronizationContext());
#endif
        }
    }
}
