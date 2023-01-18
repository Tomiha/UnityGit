using System.Linq;
using VenlySDK.Backends;
using VenlySDK.Core;
using VenlySDK.Models;

namespace VenlySDK{

    public static partial class Venly
    {
        //Register a Provider that can be used during API Initialization
        public static void RegisterProvider(VyBackendProvider provider)
        {
            if (IsProviderRegistered(provider))
                return;

            //Check if singleInstance type provider is already registered...
            if (provider.ProviderType != eVyBackendProvider.Custom)
            {
                if (_backendProviders.Any(p => p.ProviderType == provider.ProviderType))
                {
                    throw new VyException(
                        $"A Backend Provider with type \'{provider.ProviderType}\' is already registered. (Multi-registration is only allowed for Custom Providers)");
                }
            }

            //Add Provider
            _backendProviders.Add(provider);
        }

        private static bool IsProviderRegistered(VyBackendProvider provider)
        {
            return _backendProviders.Any(p => p.ProviderType == provider.ProviderType && p.CustomId == provider.CustomId);
        }

        public static void Initialize(IBackendExtension extensionsOverride = null)
        {
            var customId = -1;
            if (VenlySettings.BackendProvider == eVyBackendProvider.Custom)
                customId = VenlySettings.CustomBackendSettings.CustomId;

            Initialize(VenlySettings.BackendProvider, extensionsOverride, customId);
        }

        //Provider Specific Initialization (Only if registered)
        public static void Initialize(eVyBackendProvider type, IBackendExtension extensionsOverride = null, int customId = -1)
        {
            //Deinitialize if required
            Deinitialize();

            //Find the requester provider type
            var providerMatches = _backendProviders.FindAll(p => p.ProviderType == type);
            var provider = providerMatches.Count == 1
                ? providerMatches[0]
                : providerMatches.FirstOrDefault(p => p.CustomId == customId);

            //Provider Found
            if (provider != null)
            {
                //Provider INIT
                provider.Initialize(); //todo error handling during provider init (promise? ret val?)

                //Override Backend Extensions if required
                if (extensionsOverride != null)
                    provider.OverrideExtension(extensionsOverride);

                //API INIT
                Initialize(provider, VenlySettings.Environment);

                return;
            }

            //Provider NOT Found
            throw new VyException(
                $"Provider with type \'{type}\' is not registered.\nVenlyAPI Initialization Failed!");
        }

        public static void Initialize(VyBackendProvider provider)
        {
            Initialize(provider, VenlySettings.Environment);
        }

        public static bool HandleProviderError(object err)
        {
            return _currentProvider != null && _currentProvider.HandleError(err);
        }

        public static void OverrideBackendExtensions(IBackendExtension extensions, eVyBackendProvider providerType = eVyBackendProvider.DevMode, int customId = -1)
        {
            VyBackendProvider provider = null;
            if(providerType != eVyBackendProvider.Custom) provider = _backendProviders.FirstOrDefault(p => p.ProviderType == providerType);
            else provider = _backendProviders.FirstOrDefault(p => (p.ProviderType == providerType) && (p.CustomId == customId));

            if (provider != null)
            {
                provider.OverrideExtension(extensions);
            }
        }
    }
}
