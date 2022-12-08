using System;
using PlayFab;
using UnityEngine;
using Venly.Models;

namespace Venly.Backends.PlayFab
{
    public class VyPlayFabProvider : BackendProvider
    {
        public VyPlayFabProvider() : base(eVyBackendProvider.PlayFab)
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            VenlyAPI.RegisterProvider(new VyPlayFabProvider());
        }

        protected override void OnInitialize()
        {
            Requester = new VyPlayfabRequester();
            Extensions = new VyPlayFabExtension(Requester as VyPlayfabRequester);
        }

        protected override void OnDeinitialize()
        {
            Requester = null;
            Extensions = null;
        }

        public override bool HandleError(object err)
        {
            if (err is PlayFabError pfErr)
            {
                Debug.LogException(new VenlyException($"[PLAYFAB_BACKEND] {pfErr.ErrorMessage}"));
                return true;
            }

            return false;
        }
    }
}