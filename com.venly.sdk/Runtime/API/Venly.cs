using System;
using System.Collections.Generic;
using VenlySDK.Backends;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;
#if ENABLE_VENLY_AZURE
using VenlySDK.Azure;
#endif

namespace VenlySDK
{
    public static partial class Venly
    {
        public static bool IsInitialized { get; private set; }
        public static eVyEnvironment CurrentEnvironement { get; private set; }

        public static IBackendExtension BackendExtension => _currentProvider?.Extensions;
        private static IVenlyRequester _requester => _currentProvider?.Requester;

        private static readonly List<BackendProvider> _backendProviders = new();
        private static BackendProvider _currentProvider;

        //Custom Initialization
        internal static void Initialize(BackendProvider provider, eVyEnvironment env)
        {
            //Deinitialize if required
            Deinitialize();

            //Check if provider is Initialized
            if (!provider.IsInitialized)
            {
                throw new VyException(
                    $"Provided Backend Provider (\'{provider.ProviderType}\') is not yet initialized.\nVenlyAPI Intialization Failed!");
            }

            //Set Current Provider
            _currentProvider = provider;

            //Set Environment
            CurrentEnvironement = env;

            //Configure Task System (Synchronization Context)
            VyTask.Configure();

            //Set IsInitialized flag!
            IsInitialized = true;
        }

        private static void Deinitialize()
        {
            if (!IsInitialized) return;

            //Uninitialize the backend provider
            if (_currentProvider != null)
            {
                _currentProvider.Deinitialize();
                _currentProvider = null;
            }

            //API Uninitialize
            //...

            IsInitialized = false;
        }

        public static void SetRequesterData(string key, object data)
        {
            if (!IsInitialized)
            {
                throw new Exception("VenlyAPI not yet initialized!");
            }

            _requester.SetData(key, data);
        }

        #region Request Helpers

        private static Exception VerifyRequest()
        {
            if (!IsInitialized) return new VyException("VenlyAPI not yet initialized!");
            if (_requester == null) return new VyException("VenlyAPI requester is null");

            return null!;
        }

        private static VyTask<T> Request<T>(VyRequestData requestData)
        {
            var ex = VerifyRequest();
            return ex != null ? VyTask<T>.Failed(ex) : _requester.MakeRequest<T>(requestData);
        }

#endregion
    }
}