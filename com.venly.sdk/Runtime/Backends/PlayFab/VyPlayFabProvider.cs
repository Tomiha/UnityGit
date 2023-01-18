using System;
using PlayFab;
using UnityEngine;
using VenlySDK.Core;
using VenlySDK.Models;

namespace VenlySDK.Backends.PlayFab
{
    public class VyPlayFabProvider : VyBackendProvider
    {
        public VyPlayFabProvider() : base(eVyBackendProvider.PlayFab)
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            Venly.RegisterProvider(new VyPlayFabProvider());
        }

        protected override void OnInitialize()
        {
            Requester = new VyPlayfabRequester();
            OverrideExtension(new VyPlayFabExtension(Requester as VyPlayfabRequester));
        }

        protected override void OnDeinitialize()
        {
            Requester = null;
            OverrideExtension(null);
        }

        public override bool HandleError(object err)
        {
            if (err is PlayFabError pfErr)
            {
                Debug.LogException(new VyException($"[PLAYFAB_BACKEND] {pfErr.ErrorMessage}"));
                return true;
            }

            return false;
        }
    }
}