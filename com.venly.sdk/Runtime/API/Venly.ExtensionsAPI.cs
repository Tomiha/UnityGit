using VenlySDK.Core;
using VenlySDK.Models.Shared;
using VenlySDK.Models.Wallet;

namespace VenlySDK
{
    public static partial class Venly
    {
        public static class ProviderExtensions
        {
            public static VyTask<T> Invoke<T>(VyExtensionRequestData requestData)
            {
                if (!_provider.ExtensionsSupported)
                    return VyTask<T>.Failed(
                        $"Provider Extensions are not supported for the selected provider \'{_provider.ProviderType}\'");

                return Request<T>(requestData.BaseRequest);
            }

            public static VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams)
            {
                var request = VyExtensionRequestData.Create("user_create_wallet")
                    .AddJsonContent(reqParams);

                return Invoke<VyWalletDto>(request);
            }

            public static VyTask<VyWalletDto> GetWalletForUser()
            {
                var request = VyExtensionRequestData.Create("user_get_wallet");
                return Invoke<VyWalletDto>(request);
            }
        }
    }
}
