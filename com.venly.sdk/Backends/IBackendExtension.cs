using Proto.Promises;
using Venly.Models;

namespace Venly.Backends
{
    public interface IBackendExtension
    {
        public Promise<VyWallet> CreateWalletForUser(VyParam_CreateWallet walletDetails);
        public Promise<VyWallet> GetWalletForUser();
    }

    public class DefaultBackendExtension:IBackendExtension
    {
        public Promise<VyWallet> CreateWalletForUser(VyParam_CreateWallet walletDetails)
        {
            throw new VenlyException("Backend Extension \'CreateWalletForUser\' is not supported by the current provider");
        }

        public Promise<VyWallet> GetWalletForUser()
        {
            throw new VenlyException("Backend Extension \'GetWalletForUser\' is not supported by the current provider");
        }
    }
}
