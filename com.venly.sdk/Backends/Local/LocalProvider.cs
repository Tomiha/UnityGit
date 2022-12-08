using UnityEngine;
using Venly.Models;

namespace Venly.Backends.Local
{
    public class LocalProvider : BackendProvider
    {
        public LocalProvider() : base(eVyBackendProvider.Local)
        {
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            VenlyAPI.RegisterProvider(new LocalProvider());
        }
#endif

        protected override void OnInitialize()
        {
#if UNITY_EDITOR
            Requester = new LocalRequester(VenlySettings.ClientId, VenlySettings.ClientSecret);
            //Default Backend Extensions
#endif
        }

        protected override void OnDeinitialize()
        {
            Requester = null;
        }
    }
}