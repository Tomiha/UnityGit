using Proto.Promises;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Venly.Models;
using Venly.Utils;

namespace Venly
{
    public static partial class VenlyAPI
    {
        public static class Client
        {
            public static class Wallet
            {
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Wallet;

                /// <summary>
                /// Retrieve the supported chains for the Wallet API
                /// [/api/chains]
                /// </summary>
                /// <returns>List of supported chains</returns>
                public static Promise<eVySecretType[]> GetChains()
                {
                    return Request<eVySecretType[]>(HttpMethod.Get, "/api/chains", _apiEndpoint);
                }

                /// <summary>
                /// Retrieve information on a specific BlockChain
                /// [/api/chains/:secretType]
                /// </summary>
                /// <param name="secretType">The BlockChain to get information from</param>
                /// <returns>BlockChain information</returns>
                public static Promise<VyBlockchainInfo> GetBlockNumber(eVySecretType secretType)
                {
                    return Request<VyBlockchainInfo>(HttpMethod.Get, $"/api/chains/{secretType.GetMemberName()}", _apiEndpoint);
                }

                /// <summary>
                /// Retrieve all the wallets associated with this client account
                /// [/api/wallets]
                /// </summary>
                /// <returns>List of Venly Wallets</returns>
                public static Promise<VyWallet[]> GetWallets()
                {
                    return Request<VyWallet[]>(HttpMethod.Get, "/api/wallets", _apiEndpoint);
                }

                /// <summary>
                /// Retrieve a wallet based on a wallet ID
                /// [/api/wallets/:walletId]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>The wallet associated with the wallet ID</returns>
                public static Promise<VyWallet> GetWallet(string walletId)
                {
                    return Request<VyWallet>(HttpMethod.Get, $"/api/wallets/{walletId}", _apiEndpoint);
                }

                /// <summary>
                /// Retrieves all the Fungible Tokens from a specific wallet
                /// [/api/wallets/:walletId/balance/tokens]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of Fungible Tokens</returns>
                public static Promise<VyFungibleToken[]> GetTokenBalances(string walletId)
                {
                    return Request<VyFungibleToken[]>(HttpMethod.Get, $"/api/wallets/{walletId}/balance/tokens", _apiEndpoint);
                }

                /// <summary>
                /// Retrieves all the Non-Fungible Tokens from a specific wallet
                /// [/api/wallets/:walletId/nonfungibles]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of Non-Fungible Tokens</returns>
                public static Promise<VyNonFungibleToken[]> GetNftTokenBalances(string walletId)
                {
                    return Request<VyNonFungibleToken[]>(HttpMethod.Get, $"/api/wallets/{walletId}/nonfungibles", _apiEndpoint);
                }

                /// <summary>
                /// Retrieves all the Non-Fungible Tokens from a specific wallet for a specific BlockChain
                /// [/api/wallets/:secretType/:walletAddress/nonfungibles]
                /// </summary>
                /// <param name="walletAddress">The address of the wallet</param>
                /// <param name="secretType">The associated BlockChain</param>
                /// <returns>List of Non-Fungible Tokens</returns>
                public static Promise<VyNonFungibleToken[]> GetNftTokenBalances(eVySecretType secretType, string walletAddress)
                {
                    return Request<VyNonFungibleToken[]>(HttpMethod.Get, $"/api/wallets/{secretType.GetMemberName()}/{walletAddress}/nonfungibles", _apiEndpoint);
                }

                /// <summary>
                /// Retrieve information on a Transaction of a specific BlockChain
                /// [/api/transactions/:secretType/:txHash/status]
                /// </summary>
                /// <param name="secretType">The associated BlockChain</param>
                /// <param name="txHash">Hash of the transaction</param>
                /// <returns>Information about the requested Transaction</returns>
                public static Promise<VyTransactionInfo> GetTransactionInfo(eVySecretType secretType, string txHash)
                {
                    return Request<VyTransactionInfo>(HttpMethod.Get, $"/api/transactions/{secretType.GetMemberName()}/{txHash}/status", _apiEndpoint);
                }

                /// <summary>
                /// Retrieve the wallet events of a specific wallet
                /// [/api/wallets/:walletId/events]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of associated wallet events</returns>
                public static Promise<VyWalletEvent[]> GetWalletEvents(string walletId)
                {
                    return Request<VyWalletEvent[]>(HttpMethod.Get, $"/api/wallets/{walletId}/events", _apiEndpoint);
                }

                /// <summary>
                /// Validate a wallet address on a specific BlockChain
                /// [/api/wallets/address/validation/:secretType/:walletAddress]
                /// </summary>
                /// <param name="secretType">The BlockChain</param>
                /// <param name="walletAddress">The Address of the wallet</param>
                /// <returns>A wallet validation</returns>
                public static Promise<VyWalletValidation> ValidateWalletAddress(eVySecretType secretType, string walletAddress)
                {
                    return Request<VyWalletValidation>(HttpMethod.Get, $"/api/wallets/address/validation/{secretType.GetMemberName()}/{walletAddress}", _apiEndpoint);
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
                    public static Promise<VyTradingPair[]> GetPairs(string walletId)
                    {
                        return Request<VyTradingPair[]>(HttpMethod.Get, $"/api/wallets/{walletId}/swaps/pairs", _apiEndpoint);
                    }

                    /// <summary>
                    /// Retrieve the exchange rate for a specific swap
                    /// [/api/swaps/rates?...]
                    /// </summary>
                    /// <param name="reqParams">Required parameters for this request</param>
                    /// <returns>Results of all possible exchange rates, and the best exchange rate</returns>
                    public static Promise<VyExchangeRateResult> GetRate(VyParam_GetSwapRate reqParams)
                    {
                        return Request_FORM<VyExchangeRateResult, VyParam_GetSwapRate>(HttpMethod.Get, "/api/swaps/rates", _apiEndpoint, reqParams);
                    }
                }
                #endregion
            }

            public static class NFT
            {
                //Toggle between VyResponse wrapped responses (some calls are not wrapped yet...)
                private static readonly bool _useWrap = false;
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Nft;

                /// <summary>
                /// Retrieve a list of all the BlockChains that are supported by the NFT API
                /// [/api/env]
                /// </summary>
                /// <returns>List of supported BlockChains</returns>
                public static Promise<eVySecretType[]> GetChains()
                {
                    var defferedPromise = Promise<eVySecretType[]>.NewDeferred();

                    Request<JObject>(HttpMethod.Get, "/api/env", _apiEndpoint, _useWrap)
                        .Then(ret =>
                        {
                            defferedPromise.Resolve(ret["supportedChainsForItemCreation"].ToObject<eVySecretType[]>());
                        })
                        .CatchAndForget<eVySecretType[]>(defferedPromise);

                    return defferedPromise.Promise;
                }

                /// <summary>
                /// Retrieve information of all the NFT contracts associated with a specific application ID
                /// [/api/apps/:applicationId/contracts]
                /// </summary>
                /// <param name="applicationId">The applicationID associated with the retrieved contracts</param>
                /// <returns>List of Contract Information</returns>
                public static Promise<VyContract[]> GetContracts(string applicationId)
                {
                    return Request<VyContract[]>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts", _apiEndpoint, _useWrap);
                }

                /// <summary>
                /// Retrieve information of a single NFT contract associated with a specific application ID
                /// [/api/apps/:applicationId/contracts/:contractId]
                /// </summary>
                /// <param name="applicationId">The applicationID associated with the retrieved contract</param>
                /// <param name="contractId">The ID of the contract you want the information from</param>
                /// <returns>Contract Information</returns>
                public static Promise<VyContract> GetContract(string applicationId, int contractId)
                {
                    return Request<VyContract>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}", _apiEndpoint, _useWrap);
                }

                /// <summary>
                /// Retrieve information of all NFT token types (templates) from one of your contracts
                /// </summary>
                /// <param name="applicationId">The applicationID associated with the contract</param>
                /// <param name="contractId">The ID of the contract you want the token type information from</param>
                /// <returns>List NFT token type (template) information</returns>
                public static Promise<VyTokenType[]> GetTokenTypes(string applicationId, int contractId)
                {
                    return Request<VyTokenType[]>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/token-types", _apiEndpoint, _useWrap);
                }

                /// <summary>
                /// Retrieve information of a single token type (template) from one of your contracts
                /// </summary>
                /// <param name="applicationId">The ID of the application</param>
                /// <param name="contractId">The ID of the contract</param>
                /// <param name="tokenTypeId">The ID of the token type (template)</param>
                /// <returns>NFT token type (template) Information</returns>
                public static Promise<VyTokenType> GetTokenType(string applicationId, int contractId, int tokenTypeId)
                {
                    return Request<VyTokenType>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/token-types/{tokenTypeId}", _apiEndpoint, _useWrap);
                }

                //todo: check model types/names/differences between template_meta vs token_meta
                /// <summary>
                /// Retrieve information (+ metadata) of a single token type (template) from one of your contracts
                /// [/api/apps/:applicationId/contracts/:contractId/token-types/:tokenTypeId/metadata]
                /// </summary>
                /// <param name="applicationId">Your personal application ID</param>
                /// <param name="contractId">Contract ID related to the token type (nft-template)</param>
                /// <param name="tokenTypeId">ID of the token type (nft-template)</param>
                /// <returns>NFT token type (template) Information including metadata</returns>
                public static Promise<VyNonFungibleToken> GetTokenTypeMetadata(string applicationId, int contractId, int tokenTypeId)
                {
                    return Request<VyNonFungibleToken>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/token-types/{tokenTypeId}/metadata", _apiEndpoint, _useWrap);
                }

                //todo: check name/data mangling between VyNonFungibleToken & VyTokenType (including metadata)...
                /// <summary>
                /// Retrieve information (+ metadata) of a single token from one of your contracts
                /// [/api/apps/:applicationId/contracts/:contractId/tokens/:tokenId/metadata]
                /// </summary>
                /// <param name="applicationId">Your personal application ID</param>
                /// <param name="contractId">Contract ID related to the token</param>
                /// <param name="tokenId">ID of the token</param>
                /// <returns>NFT token Information including metadata</returns>
                public static Promise<VyNonFungibleToken> GetTokenMetadata(string applicationId, int contractId, int tokenId)
                {
                    return Request<VyNonFungibleToken>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/tokens/{tokenId}/metadata", _apiEndpoint, _useWrap);
                }

                /// <summary>
                /// Retrieve all the tokens related to a specific token-type (template)
                /// [/api/apps/:applicationId/contracts/:contractId/token-types/:tokenTypeId/tokens]
                /// </summary>
                /// <param name="applicationId">Your personal application ID</param>
                /// <param name="contractId">Contract ID related to the token-type (nft-template)</param>
                /// <param name="tokenTypeId">ID of the token-type (nft-template)</param>
                /// <returns>List of all the Tokens associated with the given token-type (nft-template)</returns>
                public static Promise<VyNonFungibleToken[]> GetTokensForType(string applicationId, int contractId, int tokenTypeId)
                {
                    return Request<VyNonFungibleToken[]>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/token-types/{tokenTypeId}/tokens", _apiEndpoint, _useWrap);
                }
            }

            public static class Market
            {

            }
        }
    }
}