using UnityEngine;
using VenlySDK.Models;

namespace VenlySDK.Backends.Local
{
    public class LocalProvider : BackendProvider
    {
        public LocalProvider() : base(eVyBackendProvider.DevMode)
        {
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            Venly.RegisterProvider(new LocalProvider());
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