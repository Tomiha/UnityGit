using System.Collections.Generic;
using VenlySDK.Core;
using VenlySDK.Models.Shared;

namespace VenlySDK.Backends
{
    public abstract class VyProviderBase
    {
        public string ProviderType { get; } = null;
        public bool IsInitialized { get; private set; }
        public bool ExtensionsSupported { get; protected set; } = false;

        private Dictionary<string, object> _providerData = new();

        protected VyProviderBase(string providerType)
        {
            ProviderType = providerType;
        }

        public abstract VyTask<T> MakeRequest<T>(VyRequestData requestData);
        protected virtual void OnInitialize(){}
        protected virtual void OnDeinitialize() {}

        internal void _Initialize()
        {
            if(IsInitialized)
                _Deinitialize();

            OnInitialize();
            IsInitialized = true;
        }
        internal void _Deinitialize()
        {
            if (!IsInitialized)
                return;

            OnDeinitialize();
            IsInitialized = false;
        }

        protected bool HasData(string key)
        {
            return _providerData.ContainsKey(key);
        }

        protected T GetData<T>(string key)
        {
            if (_providerData.ContainsKey(key))
                return (T) _providerData[key];

            return default;
        }

        internal void _SetData<T>(string key, T data)
        {
            SetData(key, data);
        }

        protected void SetData<T>(string key, T data)
        {
            _providerData[key] = data;
            OnSetData(key, data);
        }

        protected virtual void OnSetData(string key, object data) {}
    }
}