using VenlySDK.Core;
using VenlySDK.Models;

namespace VenlySDK.Backends
{
    public interface IBackendExtension
    {
        public VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams);
        public VyTask<VyWalletDto> GetWalletForUser();
    }

    public class DefaultBackendExtension : IBackendExtension
    {
        public VyTask<VyWalletDto> CreateWalletForUser(VyCreateWalletDto reqParams)
        {
            throw new VyException(
                "Backend Extension \'CreateWalletForUser\' is not supported by the current provider");
        }

        public VyTask<VyWalletDto> GetWalletForUser()
        {
            throw new VyException("Backend Extension \'GetWalletForUser\' is not supported by the current provider");
        }
    }
}