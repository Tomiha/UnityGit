using VenlySDK.Core;
using VenlySDK.Models;

namespace VenlySDK.Backends.PlayFab
{
    public class VyPlayFabExtension:IBackendExtension
    {
        private readonly VyRequester _requester;

        public VyPlayFabExtension(VyPlayfabRequester requester)
        {
            _requester = requester;
        }

        public VyTask<T> Invoke<T>(VyExtensionRequestData requestData)
        {
            return _requester.MakeRequest<T>(requestData);
        }

        public VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams, object customData = null)
        {
            var reqData = VyExtensionRequestData.Create("user_create_wallet").AddJsonContent(reqParams);
            return _requester.MakeRequest<VyWalletDto>(reqData);
        }

        public VyTask<VyWalletDto> GetWalletForUser(object customData = null)
        {
            var reqData = VyExtensionRequestData.Create("user_get_wallet");
            return _requester.MakeRequest<VyWalletDto>(reqData);
        }
    }
}