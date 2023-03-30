using System;
using System.Collections.Generic;
using VenlySDK.Backends;
using VenlySDK.Core;
using VenlySDK.Models.Shared;

#if ENABLE_VENLY_AZURE
using VenlySDK.Azure;
#endif

namespace VenlySDK
{
    public static partial class Venly
    {
        public static bool IsInitialized { get; private set; }
        public static eVyEnvironment CurrentEnvironment { get; private set; }
        public static string CurrentProviderType => _provider?.ProviderType;

        private static readonly Dictionary<string, VyProviderBase> _registeredProviders = new();
        private static VyProviderBase _provider;

#if !VENLY_API_NOT_UNITY //Unity
        //Register a Provider that can be used during API Initialization
        public static void RegisterProvider(VyProviderBase provider)
        {
            if (IsProviderRegistered(provider))
                return;

            //Add Provider
            _registeredProviders.Add(provider.ProviderType, provider);
        }

        //Check if a provider is registered
        private static bool IsProviderRegistered(VyProviderBase provider)
        {
            return IsProviderRegistered(provider.ProviderType);
        }

        private static bool IsProviderRegistered(string providerType)
        {
            return _registeredProviders.ContainsKey(providerType);
        }

        public static void Initialize(string providerType, eVyEnvironment env)
        {
            if (!IsProviderRegistered(providerType))
                throw new Exception($"[VENLY-API] Provider of type \'{providerType}\' not found, please register before initilization.");

            var p = _registeredProviders[providerType];
            p._Initialize();

            Initialize(p, env);
        }
#endif

#if !VENLY_API_NOT_UNITY
        internal
#else
        public
#endif
        static void SetEnvironment(eVyEnvironment env)
        {
            CurrentEnvironment = env;
        }

#if !VENLY_API_NOT_UNITY
        public static VyTask Initialize(VyProviderBase provider, eVyEnvironment env)
#else
        public static VyTask Initialize(VyServerProviderBase provider, eVyEnvironment env)
#endif
        {
            //Deinitialize if required
            Deinitialize();

            //Check if provider is Initialized
            if (!provider.IsInitialized)
            {
                provider._Initialize();
            }

            //Set Current Provider
            _provider = provider;

            //Set Environment
            CurrentEnvironment = env;

            //Configure Task System (Synchronization Context)
            VyTaskBase.Initialize();

            //Set IsInitialized flag!
            IsInitialized = true;

#if !VENLY_API_NOT_UNITY
            return VyTask.Succeeded();
#else
            return provider.VerifyAuthentication();
#endif
        }

        public static void Deinitialize()
        {
            if (!IsInitialized) return;

            //Uninitialize the backend provider
            if (_provider != null)
            {
                _provider._Deinitialize();
                _provider = null;
            }

            //API Uninitialize
            //...

            IsInitialized = false;
        }

        public static void SetProviderData(string key, object data)
        {
            if (!IsInitialized)
            {
                throw new Exception("VenlyAPI not yet initialized!");
            }

            _provider._SetData(key, data);
        }

#region Request Helpers

        private static Exception VerifyRequest()
        {
            if (!IsInitialized) return new VyException("VenlyAPI not yet initialized!");
            if (_provider == null) return new VyException("VenlyAPI provider is null");

            return null!;
        }

        private static VyTask<T> Request<T>(VyRequestData requestData)
        {
            var ex = VerifyRequest();
            return ex != null ? VyTask<T>.Failed(ex) : _provider.MakeRequest<T>(requestData);
        }

#endregion
    }
}