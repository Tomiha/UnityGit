using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;
using VenlySDK.Utils;

namespace VenlySDK
{
    public static partial class Venly
    {
        public static class WalletAPI
        {
            private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Wallet;

            public static class Client
            {
                /// <summary>
                /// Retrieve the supported chains for the Wallet API
                /// [/api/chains]
                /// </summary>
                /// <returns>List of supported chains</returns>
                public static VyTask<eVyChain[]> GetChains()
                {
                    var reqData = VyRequestData.Get("/api/chains", _apiEndpoint);
                    return Request<eVyChain[]>(reqData);
                }

                /// <summary>
                /// Retrieve information on a specific BlockChain
                /// [/api/chains/:chain]
                /// </summary>
                /// <param name="chain">The BlockChain to get information from</param>
                /// <returns>BlockChain information</returns>
                public static VyTask<VyBlockchainInfoDto> GetBlockNumber(eVyChain chain)
                {
                    var reqData = VyRequestData.Get($"/api/chains/{chain.GetMemberName()}", _apiEndpoint);
                    return Request<VyBlockchainInfoDto>(reqData);
                }

                /// <summary>
                /// Retrieve all the wallets associated with this client account
                /// [/api/wallets]
                /// </summary>
                /// <returns>List of Venly Wallets</returns>
                public static VyTask<VyWalletDto[]> GetWallets()
                {
                    var reqData = VyRequestData.Get("/api/wallets", _apiEndpoint);
                    return Request<VyWalletDto[]>(reqData);
                }

                /// <summary>
                /// Retrieve a wallet based on a wallet ID
                /// [/api/wallets/:walletId]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>The wallet associated with the wallet ID</returns>
                public static VyTask<VyWalletDto> GetWallet(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyWalletDto>.Failed(VyException.Argument("Parameter cannot be NULL or empty",
                            nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}", _apiEndpoint);
                    return Request<VyWalletDto>(reqData);
                }

                /// <summary>
                /// Retrieves all the Fungible Tokens from a specific wallet
                /// [/api/wallets/:walletId/balance/tokens]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of Fungible Tokens</returns>
                public static VyTask<VyFungibleTokenDto[]> GetTokenBalances(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyFungibleTokenDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}/balance/tokens", _apiEndpoint);
                    return Request<VyFungibleTokenDto[]>(reqData);
                }

                /// <summary>
                /// Retrieves all the Non-Fungible Tokens from a specific wallet
                /// [/api/wallets/:walletId/nonfungibles]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of Non-Fungible Tokens</returns>
                public static VyTask<VyNonFungibleTokenDto[]> GetNftTokenBalances(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyNonFungibleTokenDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}/nonfungibles", _apiEndpoint);
                    return Request<VyNonFungibleTokenDto[]>(reqData);
                }

                /// <summary>
                /// Retrieves all the Non-Fungible Tokens from a specific wallet for a specific BlockChain
                /// [/api/wallets/:chain/:walletAddress/nonfungibles]
                /// </summary>
                /// <param name="walletAddress">The address of the wallet</param>
                /// <param name="chain">The associated BlockChain</param>
                /// <returns>List of Non-Fungible Tokens</returns>
                public static VyTask<VyNonFungibleTokenDto[]> GetNftTokenBalances(eVyChain chain, string walletAddress)
                {
                    if (string.IsNullOrEmpty(walletAddress))
                        return VyTask<VyNonFungibleTokenDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletAddress)));

                    var reqData = VyRequestData.Get(
                        $"/api/wallets/{chain.GetMemberName()}/{walletAddress}/nonfungibles",
                        _apiEndpoint);
                    return Request<VyNonFungibleTokenDto[]>(reqData);
                }

                /// <summary>
                /// Retrieve information on a Transaction of a specific BlockChain
                /// [/api/transactions/:chain/:txHash/status]
                /// </summary>
                /// <param name="chain">The associated BlockChain</param>
                /// <param name="txHash">Hash of the transaction</param>
                /// <returns>Information about the requested Transaction</returns>
                public static VyTask<VyTransactionInfoDto> GetTransactionInfo(eVyChain chain, string txHash)
                {
                    if (string.IsNullOrEmpty(txHash))
                        return VyTask<VyTransactionInfoDto>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(txHash)));

                    var reqData = VyRequestData.Get($"/api/transactions/{chain.GetMemberName()}/{txHash}/status",
                        _apiEndpoint);
                    return Request<VyTransactionInfoDto>(reqData);
                }

                /// <summary>
                /// Retrieve the wallet events of a specific wallet
                /// [/api/wallets/:walletId/events]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of associated wallet events</returns>
                public static VyTask<VyWalletEventDto[]> GetWalletEvents(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyWalletEventDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}/events", _apiEndpoint);
                    return Request<VyWalletEventDto[]>(reqData);
                }

                /// <summary>
                /// Validate a wallet address on a specific BlockChain
                /// [/api/wallets/address/validation/:chain/:walletAddress]
                /// </summary>
                /// <param name="chain">The BlockChain</param>
                /// <param name="walletAddress">The Address of the wallet</param>
                /// <returns>A wallet validation</returns>
                public static VyTask<VyWalletValidationDto> ValidateWalletAddress(eVyChain chain, string walletAddress)
                {
                    if (string.IsNullOrEmpty(walletAddress))
                        return VyTask<VyWalletValidationDto>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletAddress)));

                    var reqData =
                        VyRequestData.Get($"/api/wallets/address/validation/{chain.GetMemberName()}/{walletAddress}",
                            _apiEndpoint);
                    return Request<VyWalletValidationDto>(reqData);
                }

                #region SWAP

                public static class Swap
                {
                    /// <summary>
                    /// Retrieve the possible swap pairs for a specific wallet
                    /// [/api/wallets/:walletId/swaps/pairs]
                    /// </summary>
                    /// <param name="walletId">The ID of the wallet</param>
                    /// <returns>List of possible swap pairs</returns>
                    public static VyTask<VyTradingPairDto[]> GetPairs(string walletId)
                    {
                        if (string.IsNullOrEmpty(walletId))
                            return VyTask<VyTradingPairDto[]>.Failed(
                                VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                        var reqData = VyRequestData.Get($"/api/wallets/{walletId}/swaps/pairs", _apiEndpoint);
                        return Request<VyTradingPairDto[]>(reqData);
                    }

                    /// <summary>
                    /// Retrieve the exchange rate for a specific swap
                    /// [/api/swaps/rates?...]
                    /// </summary>
                    /// <param name="reqParams">Required parameters for this request</param>
                    /// <returns>Results of all possible exchange rates, and the best exchange rate</returns>
                    public static VyTask<VyExchangeRateResultDto> GetRate(VyGetSwapRateDto reqParams)
                    {
                        var reqData = VyRequestData.Get("/api/swaps/rates", _apiEndpoint)
                            .AddFormContent(reqParams);
                        return Request<VyExchangeRateResultDto>(reqData);
                    }
                }

                #endregion
            }

#if UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_API_SERVER
            public static class Server
            {
                /// <summary>
                /// Create a new wallet
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>New wallet</returns>
                public static VyTask<VyWalletDto> CreateWallet(VyCreateWalletDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/wallets", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWalletDto>(reqData);
                }

                /// <summary>
                /// Transfer a native token
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteTransfer(VyExecuteTransferDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Transfer a Fungible Token (FT)
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteTokenTransfer(VyExecuteTokenTransferDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Transfer a Non Fungible Token (NFT)
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteNftTransfer(VyExecuteNftTransferDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Execute a function on a smart contract (write) on any (supported) BlockChain
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Execution Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteContract(VyExecuteContractDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Read from a smart contract function
                /// [/api/contracts/read]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Output values from the function</returns>
                public static VyTask<VyTypeValuePair[]> ReadContract(VyReadContractDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/contracts/read", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTypeValuePair[]>(reqData);
                }

                /// <summary>
                /// Resubmit an existing transaction which failed to execute/propagate
                /// [/api/transactions/resubmit]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ResubmitTransaction(VyResubmitTransactionDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/resubmit", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Export a Venly Wallet
                /// [/api/wallets/:walletid/export]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Keystore of the exported wallet</returns>
                public static VyTask<VyWalletExportDto> ExportWallet(VyExportWalletDto reqParams)
                {
                    var reqData = VyRequestData.Post($"/api/wallets/{reqParams.WalletId}/export", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWalletExportDto>(reqData);
                }

                /// <summary>
                /// Import an external wallet (Keystore, PrivateKey or WIF)
                /// [/api/wallets/import]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The Imported Wallet</returns>
                public static VyTask<VyWalletDto> ImportWallet(VyImportWalletBaseDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/wallets/import", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWalletDto>(reqData);
                }

                /// <summary>
                /// Reset the PIN of a specific Wallet
                /// [/api/wallets/:walletId/security]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The Updated Wallet</returns>
                public static VyTask<VyWalletDto> ResetPin(VyUpdateWalletSecurityDto reqParams)
                {
                    var reqData = VyRequestData.Patch($"/api/wallets/{reqParams.WalletId}/security", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWalletDto>(reqData);
                }
            }
#endif
        }
    }
}
