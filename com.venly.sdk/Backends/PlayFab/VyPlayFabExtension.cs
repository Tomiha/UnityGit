using System.Net.Http;
using Proto.Promises;
using Venly.Models;

namespace Venly.Backends.PlayFab
{
    public class VyPlayFabExtension:IBackendExtension
    {
        private readonly IVenlyRequester _requester;

        public VyPlayFabExtension(VyPlayfabRequester requester)
        {
            _requester = requester;
        }

        public Promise<VyWallet> CreateWalletForUser(VyParam_CreateWallet walletDetails)
        {
            return _requester.Request_JSON<VyWallet, VyParam_CreateWallet>(HttpMethod.Get, "user_create_wallet", eVyApiEndpoint.Extension, walletDetails, false);
        }

        public Promise<VyWallet> GetWalletForUser()
        {
            return _requester.Request<VyWallet>(HttpMethod.Get, "user_get_wallet", eVyApiEndpoint.Extension, false);
        }
    }
}