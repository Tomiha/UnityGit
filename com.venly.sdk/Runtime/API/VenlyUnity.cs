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
        public static void Initialize(eVyBackendProvider backendProviderType, eVyEnvironment env)
        {
            Initialize(backendProviderType.GetMemberName(), env);
        }

        public static void Initialize(IVyBackendExtension extensionsOverride = null)
        {
            var providerType = "";
            switch (VenlySettings.BackendProvider)
            {
                case eVyBackendProvider.DevMode:
                case eVyBackendProvider.PlayFab:
                    providerType = VenlySettings.BackendProvider.GetMemberName();
                    break;

                case eVyBackendProvider.Custom:
                    providerType = VenlySettings.CustomBackendSettings.CustomType;
                    break;

                default:
                    throw new NotImplementedException($"Unknown BackendProvider \'{VenlySettings.BackendProvider}\'");
            }

            Initialize(providerType, VenlySettings.Environment);
        }

        private static void Initialize(string providerType, eVyEnvironment env)
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

            //Initialize API
            VenlyAPI.Initialize(providerType, env, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
