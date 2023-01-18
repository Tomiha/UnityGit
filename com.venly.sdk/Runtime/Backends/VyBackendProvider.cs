using VenlySDK.Models;

namespace VenlySDK.Backends
{
    public abstract class VyBackendProvider
    {
        public bool IsInitialized { get; private set; }

        public eVyBackendProvider ProviderType { get; }
        public VyRequester Requester { get; protected set; }
        public IBackendExtension Extensions { get; private set; } = new DefaultBackendExtension();
        //public VyExtensionHandler ExtensionHandler { get; private set; } = new VyDefaultExtensionHandler();

        public int CustomId { get; } //used for multi ProviderType instance identification (custom type only)

        protected VyBackendProvider(eVyBackendProvider type, int customId = -1)
        {
            ProviderType = type;
            CustomId = customId;
        }

        public void Initialize()
        {
            if (IsInitialized) return;
            OnInitialize();
            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized) return;
            OnDeinitialize();
            IsInitialized = false;
        }

        public virtual bool HandleError(object err)
        {
            return false;
        }

        public void OverrideExtension(IBackendExtension extensions)
        {
            Extensions = extensions;

            if (Extensions == null)
                Extensions = new DefaultBackendExtension();
        }

        protected abstract void OnInitialize();
        protected abstract void OnDeinitialize();
    }
}