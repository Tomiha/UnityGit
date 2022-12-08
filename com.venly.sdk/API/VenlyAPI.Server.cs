using System.Collections.Generic;
using System.Net.Http;
using Proto.Promises;
using Venly.Models;
using Venly.Utils;

namespace Venly
{
    public static partial class VenlyAPI
    {
#if UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_AZURE
        public static class Server
        {
            public static class Authentication
            {
                public static Promise<VyAccessToken> GetAccessToken(string clientId, string clientSecret)
                {
                    var formData = new Dictionary<string, string>
                    {
                        {"grant_type", "client_credentials"},
                        {"client_id", clientId},
                        {"client_secret", clientSecret}
                    };

                    return Request<VyAccessToken>(HttpMethod.Post, "/auth/realms/Arkane/protocol/openid-connect/token", eVyApiEndpoint.Auth, formData, false);
                }
            }

            public static class Wallet
            {
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Wallet;

                /// <summary>
                /// Create a new wallet
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>New wallet</returns>
                public static Promise<VyWallet> CreateWallet(VyParam_CreateWallet reqParams)
                {
                    return Request_JSON<VyWallet, VyParam_CreateWallet>(HttpMethod.Post, "/api/wallets", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Transfer a native token
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static Promise<VyTransferInfo> ExecuteTransfer(VyParam_ExecuteTransfer reqParams)
                {
                    return Request_JSON<VyTransferInfo, VyParam_ExecuteTransfer>(HttpMethod.Post, "/api/transactions/execute", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Transfer a Fungible Token (FT)
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static Promise<VyTransferInfo> ExecuteTokenTransfer(VyParam_ExecuteTokenTransfer reqParams)
                {
                    return Request_JSON<VyTransferInfo, VyParam_ExecuteTokenTransfer>(HttpMethod.Post, "/api/transactions/execute", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Transfer a Non Fungible Token (NFT)
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static Promise<VyTransferInfo> ExecuteNftTransfer(VyParam_ExecuteNftTransfer reqParams)
                {
                    return Request_JSON<VyTransferInfo, VyParam_ExecuteNftTransfer>(HttpMethod.Post, "/api/transactions/execute", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Execute a function on a smart contract (write) on any (supported) BlockChain
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Execution Transaction Info</returns>
                public static Promise<VyTransferInfo> ExecuteContract(VyParam_ExecuteContract reqParams)
                {
                    return Request_JSON<VyTransferInfo, VyParam_ExecuteContract>(HttpMethod.Post, "/api/transactions/execute", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Read from a smart contract function
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Output values from the function</returns>
                public static Promise<VyTypeValuePair[]> ReadContract(VyParam_ReadContract reqParams)
                {
                    return Request_JSON<VyTypeValuePair[], VyParam_ReadContract>(HttpMethod.Post, "/api/contracts/read", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Resubmit an existing transaction which failed to execute/propagate
                /// [/api/wallets/:walletid/export]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Transaction Info</returns>
                public static Promise<VyTransferInfo> ResubmitTransaction(VyParam_ResubmitTransaction reqParams)
                {
                    return Request_JSON<VyTransferInfo, VyParam_ResubmitTransaction>(HttpMethod.Post, "/api/transactions/resubmit", _apiEndpoint, reqParams);
                }

                /// <summary>
                /// Export a Venly Wallet
                /// [/api/wallets/:walletid/export]
                /// </summary>
                /// <param name="reqParamsExport">Required parameters for the request</param>
                /// <returns>Keystore of the exported wallet</returns>
                public static Promise<VyWalletExport> ExportWallet(VyParam_ExportWallet reqParamsExport)
                {
                    return Request_JSON<VyWalletExport, VyParam_ExportWallet>(HttpMethod.Post, $"/api/wallets/{reqParamsExport.WalletId}/export", _apiEndpoint, reqParamsExport);
                }

                /// <summary>
                /// Import an external wallet (Keystore, PrivateKey or WIF)
                /// [/api/wallets/import]
                /// </summary>
                /// <param name="reqParamsImport">Required parameters for the request</param>
                /// <returns>The Imported Wallet</returns>
                public static Promise<VyWallet> ImportWallet(VyParam_ImportWallet reqParamsImport)
                {
                    return Request_JSON<VyWallet, VyParam_ImportWallet>(HttpMethod.Post, "/api/wallets/import", _apiEndpoint, reqParamsImport);
                }

                /// <summary>
                /// Reset the PIN of a specific Wallet
                /// [/api/wallets/:walletId/security]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The Updated Wallet</returns>
                public static Promise<VyWallet> ResetPin(VyParam_UpdateWalletSecurity reqParams)
                {
                    return Request_JSON<VyWallet, VyParam_UpdateWalletSecurity>(HttpMethod.Patch, $"/api/wallets/{reqParams.WalletId}/security", _apiEndpoint, reqParams);
                }
            }

            public static class NFT
            {
                private static readonly bool _useWrap = false;
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Nft;

                //todo: implement global applicationId Query
                /// <summary>
                /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
                /// [/api/apps/:applicationId/contracts]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The deployed NFT Contract</returns>
                public static Promise<VyContract> CreateContract(VyParam_CreateContract reqParams)
                {
                    return Request_JSON<VyContract, VyParam_CreateContract>(HttpMethod.Post, $"/api/apps/{reqParams.ApplicationId}/contracts", _apiEndpoint, reqParams, _useWrap);
                }

                /// <summary>
                /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
                /// [/api/apps/:applicationId/contracts/:contractId/token-types]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The deployed NFT Token-Type (Template)</returns>
                public static Promise<VyTokenType> CreateTokenType(VyParam_CreateTokenType reqParams)
                {
                    return Request_JSON<VyTokenType, VyParam_CreateTokenType>(HttpMethod.Post, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/token-types", _apiEndpoint, reqParams, _useWrap);
                }

                /// <summary>
                /// Mint a Non-Fungible Token (NFT) based on a specific Token-Type (Template)
                /// [/api/apps/:applicationId/contracts/:contractId/tokens/non-fungible/]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Information on the Minted Tokens</returns>
                public static Promise<VyMintedTokenInfo> MintTokenNFT(VyParam_MintNFT reqParams)
                {
                    return Request_JSON<VyMintedTokenInfo, VyParam_MintNFT>(HttpMethod.Post, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/tokens/non-fungible", _apiEndpoint, reqParams, _useWrap);
                }

                /// <summary>
                /// Mint a Fungible Token (FT) based on a specific Token-Type (Template)
                /// [/api/apps/:applicationId/contracts/:contractId/tokens/fungible/]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Information on the Minted Tokens</returns>
                public static Promise<VyMintedTokenInfo> MintTokenFT(VyParam_MintFT reqParams)
                {
                    return Request_JSON<VyMintedTokenInfo, VyParam_MintFT>(HttpMethod.Post, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/tokens/fungible", _apiEndpoint, reqParams, _useWrap);
                }

                /// <summary>
                /// Update the metadata of a Token-Type (NFT Template)
                /// [/api/apps/:applicationId/contracts/:contractId/token-types/:tokenTypeId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Token-Type (Template)</returns>
                public static Promise<VyTokenTypeMetadata> UpdateTokenTypeMetadata(VyParam_UpdateTokenTypeMetadata reqParams)
                {
                    return Request_JSON<VyTokenTypeMetadata, VyParam_UpdateTokenTypeMetadata>(HttpMethod.Put, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/token-types/{reqParams.TokenTypeId}/metadata", _apiEndpoint, reqParams, _useWrap);
                }

                //todo: check with backend team about this response
                /// <summary>
                /// Update the metadata of a Contract
                /// [/api/apps/:applicationId/contracts/:contractId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Contract Metadata</returns>
                public static Promise<VyContractMetadata> UpdateContractMetadata(VyParam_UpdateContractMetadata reqParams)
                {
                    return Request_JSON<VyContractMetadata, VyParam_UpdateContractMetadata>(HttpMethod.Patch, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/metadata", _apiEndpoint, reqParams, _useWrap);
                }

                /// <summary>
                /// Archive a specific Contract. When a contract is archived it is removed from your account together with all the token-types that are associated to that contract
                /// [/api/apps/:applicationId/contracts/:contractId]
                /// </summary>
                /// <param name="applicationId"></param>
                /// <param name="contractId"></param>
                /// <returns> void promise </returns>
                public static Promise ArchiveContract(string applicationId, int contractId)
                {
                    var deferredPromise = Promise.NewDeferred();

                    //todo: fix empty response
                    Request<string>(HttpMethod.Delete, $"/api/apps/{applicationId}/contracts/{contractId}", _apiEndpoint, _useWrap)
                        .Then(ret => deferredPromise.Resolve())
                        .CatchAndForget(deferredPromise);

                    return deferredPromise.Promise;
                }

                /// <summary>
                /// Archive a specific Token Type (Template)
                /// [/api/apps/:applicationId/contracts/:contractId]
                /// </summary>
                /// <param name="applicationId"></param>
                /// <param name="contractId"></param>
                /// <param name="tokenTypeId"></param>
                /// <returns> void promise </returns>
                public static Promise ArchiveTokenType(string applicationId, int contractId, int tokenTypeId)
                {
                    var deferredPromise = Promise.NewDeferred();

                    //todo: fix empty response
                    Request<string>(HttpMethod.Delete, $"/api/apps/{applicationId}/contracts/{contractId}/token-types/{tokenTypeId}", _apiEndpoint, _useWrap)
                        .Then(ret => deferredPromise.Resolve())
                        .CatchAndForget(deferredPromise);

                    return deferredPromise.Promise;
                }
            }

            public static class Market
            {

            }
        }
#endif
    }
}