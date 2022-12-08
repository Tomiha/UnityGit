using Venly.Models;

namespace Venly.Backends
{
    public abstract class BackendProvider
    {
        public bool IsInitialized { get; private set; }

        public eVyBackendProvider ProviderType { get; }
        public IVenlyRequester Requester { get; protected set; }
        public IBackendExtension Extensions { get; protected set; } = new DefaultBackendExtension();

        public int CustomId { get; } //used for multi ProviderType instance identification (custom type only)

        protected BackendProvider(eVyBackendProvider type, int customId = -1)
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

        protected abstract void OnInitialize();
        protected abstract void OnDeinitialize();
    }
}