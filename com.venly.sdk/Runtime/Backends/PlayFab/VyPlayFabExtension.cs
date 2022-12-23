using System.Net.Http;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;

namespace VenlySDK.Backends.PlayFab
{
    public class VyPlayFabExtension:IBackendExtension
    {
        private readonly IVenlyRequester _requester;

        public VyPlayFabExtension(VyPlayfabRequester requester)
        {
            _requester = requester;
        }

        public VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams)
        {
            var reqData = VyRequestData.Get("user_create_wallet", eVyApiEndpoint.Extension).AddJsonContent(reqParams);
            return _requester.MakeRequest<VyWalletDto>(reqData);
        }

        public VyTask<VyWalletDto> GetWalletForUser()
        {
            var reqData = VyRequestData.Get("user_get_wallet", eVyApiEndpoint.Extension);
            return _requester.MakeRequest<VyWalletDto>(reqData);
        }
    }
}