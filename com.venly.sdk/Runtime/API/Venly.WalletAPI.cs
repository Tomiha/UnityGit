using Newtonsoft.Json.Linq;
using VenlySDK.Core;
using VenlySDK.Models;
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
                public static VyTask<eVyChain[]> GetSupportedChains()
                {
                    var taskNotifier = VyTask<eVyChain[]>.Create();

                    var reqData = VyRequestData.Get("/api/chains", _apiEndpoint);
                    Request<eVyChainFULL[]>(reqData)
                        .OnComplete(result =>
                        {
                            if (result.Success)
                            {
                                taskNotifier.NotifySuccess(VenlyUtils.TrimUnsupportedChains(result.Data));
                            }
                            else
                            {
                                taskNotifier.NotifyFail(result.Exception);
                            }
                        });

                    return taskNotifier.Task;
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
                /// Retrieves all the ERC20 Tokens (CryptoTokens) from a specific wallet
                /// [/api/wallets/:walletId/balance/tokens]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of CryptoTokens (ERC21)</returns>
                public static VyTask<VyCryptoTokenDto[]> GetCryptoTokenBalances(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyCryptoTokenDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}/balance/tokens", _apiEndpoint);
                    return Request<VyCryptoTokenDto[]>(reqData);
                }

                /// <summary>
                /// Retrieves the Native Balance of a wallet (Native Tokens, ETH/BTC/MATIC/...)
                /// </summary>
                /// <param name="walletId">The Id of the wallet</param>
                /// <returns>Native Balance Information</returns>
                public static VyTask<VyWalletBalanceDto> GetNativeBalance(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyWalletBalanceDto>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}/balance", _apiEndpoint);
                    return Request<VyWalletBalanceDto>(reqData);
                }

                /// <summary>
                /// Retrieves the Native Balance of a wallet (Native Tokens, ETH/BTC/MATIC/...)
                /// </summary>
                /// <param name="chain">The associated Blockchain</param>
                /// <param name="walletAddress">The address of the wallet on the associated Blockchain</param>
                /// <returns>Native Balance Information</returns>
                public static VyTask<VyWalletBalanceDto> GetNativeBalance(eVyChain chain, string walletAddress)
                {
                    if (string.IsNullOrEmpty(walletAddress))
                        return VyTask<VyWalletBalanceDto>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletAddress)));

                    var reqData = VyRequestData.Get($"/api/wallets/{chain.GetMemberName()}/{walletAddress}/balance", _apiEndpoint);
                    return Request<VyWalletBalanceDto>(reqData);
                }

                /// <summary>
                /// Retrieves all the ERC1155/ERC721 tokens ('MultiTokens') from a wallet
                /// [/api/wallets/:walletId/nonfungibles]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of MultiTokens (ERC1155/721)</returns>
                public static VyTask<VyMultiTokenDto[]> GetMultiTokenBalances(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyMultiTokenDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = VyRequestData.Get($"/api/wallets/{walletId}/nonfungibles", _apiEndpoint);
                    return Request<VyMultiTokenDto[]>(reqData);
                }

                /// <summary>
                /// Retrieves all the ERC1155/ERC721 tokens ('MultiTokens') from a wallet for a specific BlockChain
                /// [/api/wallets/:chain/:walletAddress/nonfungibles]
                /// </summary>
                /// <param name="walletAddress">The address of the wallet</param>
                /// <param name="chain">The associated BlockChain</param>
                /// <returns>List of MultiTokens (ERC1155/721)</returns>
                public static VyTask<VyMultiTokenDto[]> GetMultiTokenBalances(eVyChain chain, string walletAddress)
                {
                    if (string.IsNullOrEmpty(walletAddress))
                        return VyTask<VyMultiTokenDto[]>.Failed(
                            VyException.Argument("Parameter cannot be NULL or empty", nameof(walletAddress)));

                    var reqData = VyRequestData.Get(
                        $"/api/wallets/{chain.GetMemberName()}/{walletAddress}/nonfungibles",
                        _apiEndpoint);
                    return Request<VyMultiTokenDto[]>(reqData);
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

                #region Transfer (Requires Pincode)
                /// <summary>
                /// Transfer Native Tokens
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteNativeTokenTransfer(VyExecuteNativeTokenTransferDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Transfer ERC20 Tokens
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteCryptoTokenTransfer(VyExecuteCryptoTokenTransferDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }

                /// <summary>
                /// Transfer ERC1155 or ERC721 Tokens
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfoDto> ExecuteMultiTokenTransfer(VyExecuteMultiTokenTransferDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfoDto>(reqData);
                }
                #endregion

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

#if (UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_API_SERVER) && !ENABLE_VENLY_PLAYFAB
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
                /// Update the metadata of a Wallet (Primary, Archived, Description)
                /// [/api/wallets/:walletId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Parameters</returns>
                public static VyTask<VyWalletMetadataResponseDto> UpdateWalletMetadata(VyUpdateWalletMetadataDto reqParams)
                {
                    var reqData = VyRequestData.Patch($"/api/wallets/{reqParams.WalletId}", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWalletMetadataResponseDto>(reqData);
                }

                /// <summary>
                /// Archive a wallet
                /// [/api/wallets/:walletId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Parameters</returns>
                public static VyTask<VyWalletMetadataResponseDto> ArchiveWallet(string walletId)
                {
                    var reqParams = new VyUpdateWalletMetadataDto(walletId) { Archived = true };
                    return UpdateWalletMetadata(reqParams);
                }

                /// <summary>
                /// Unarchive a wallet
                /// [/api/wallets/:walletId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Parameters</returns>
                public static VyTask<VyWalletMetadataResponseDto> UnarchiveWallet(string walletId)
                {
                    var reqParams = new VyUpdateWalletMetadataDto(walletId) {Archived = false};
                    return UpdateWalletMetadata(reqParams);
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
