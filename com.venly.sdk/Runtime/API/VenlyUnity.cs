using System;
using VenlySDK.Backends;
using VenlySDK.Models.Shared;
using VenlySDK.Utils;

namespace VenlySDK
{

    public static class VenlyUnity
    {
        public static void Initialize(eVyBackendProvider backendProviderType, eVyEnvironment env)
        {
            Venly.Initialize(backendProviderType.GetMemberName(), env);
        }

        public static void Initialize(IBackendExtension extensionsOverride = null)
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

            Venly.Initialize(providerType, VenlySettings.Environment);
        }
    }
}
