using VenlySDK.Core;
using VenlySDK.Models.Wallet;

namespace VenlySDK.Backends
{
    public interface IBackendExtension
    {
        public VyTask<T> Invoke<T>(VyExtensionRequestData requestData);
        public VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams, object customData = null);
        public VyTask<VyWalletDto> GetWalletForUser(object customData = null);
    }

    public class DefaultBackendExtension : IBackendExtension
    {
        public VyTask<T> Invoke<T>(VyExtensionRequestData requestData)
        {
            throw new VyException(
                "Backend Extension \'InvokeCustom\' is not supported by the current provider");
        }

        public VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams, object customData = null)
        {
            throw new VyException(
                "Backend Extension \'CreateWalletForUser\' is not supported by the current provider");
        }

        public VyTask<VyWalletDto> GetWalletForUser(object customData = null)
        {
            throw new VyException("Backend Extension \'GetWalletForUser\' is not supported by the current provider");
        }
    }
}