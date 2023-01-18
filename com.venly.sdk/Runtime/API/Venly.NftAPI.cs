using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Utils;

namespace VenlySDK
{
    public static partial class Venly
    {
        public static class NftAPI
        {
            private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Nft;

            public static class Client
            {
                /// <summary>
                /// Retrieve a list of all the BlockChains that are supported by the NFT API
                /// [/api/env]
                /// </summary>
                /// <returns>List of supported BlockChains</returns>
                public static VyTask<eVyChain[]> GetSupportedChains()
                {
                    var taskNotifier = VyTask<eVyChain[]>.Create();

                    var reqData = VyRequestData.Get("/api/env", _apiEndpoint)
                        .SelectProperty("supportedChainsForItemCreation");

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
                /// Retrieve information of all the NFT contracts associated with a specific application ID
                /// [/api/minter/contracts]
                /// </summary>
                /// <returns>List of Contract Information</returns>
                public static VyTask<VyContractDto[]> GetContracts()
                {
                    var reqData = VyRequestData.Get("/api/minter/contracts", _apiEndpoint);
                    return Request<VyContractDto[]>(reqData);
                }

                /// <summary>
                /// Retrieve information of a single NFT contract associated with a specific application ID
                /// [/api/minter/contracts/:contractId]
                /// </summary>
                /// <param name="contractId">The ID of the contract you want the information from</param>
                /// <returns>Contract Information</returns>
                public static VyTask<VyContractDto> GetContract(int contractId)
                {
                    var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}", _apiEndpoint);
                    return Request<VyContractDto>(reqData);
                }

                /// <summary>
                /// Retrieve information of all NFT token types (templates) from one of your contracts
                /// [/api/minter/contracts/:contractId/token-types]
                /// </summary>
                /// <param name="contractId">The ID of the contract you want the token type information from</param>
                /// <returns>List NFT token type (template) information</returns>
                public static VyTask<VyTokenTypeDto[]> GetTokenTypes(int contractId)
                {
                    var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types", _apiEndpoint);
                    return Request<VyTokenTypeDto[]>(reqData);
                }

                /// <summary>
                /// Retrieve information of a single token type (template) from one of your contracts
                /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId]
                /// </summary>
                /// <param name="contractId">The ID of the contract</param>
                /// <param name="tokenTypeId">The ID of the token type (template)</param>
                /// <returns>NFT token type (template) Information</returns>
                public static VyTask<VyTokenTypeDto> GetTokenType(int contractId, int tokenTypeId)
                {
                    var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}",
                        _apiEndpoint);
                    return Request<VyTokenTypeDto>(reqData);
                }

                //todo: check model types/names/differences between template_meta vs token_meta
                /// <summary>
                /// Retrieve information (+ metadata) of a single token type (template) from one of your contracts
                /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
                /// </summary>
                /// <param name="contractId">Contract ID related to the token type (nft-template)</param>
                /// <param name="tokenTypeId">ID of the token type (nft-template)</param>
                /// <returns>NFT token type (template) Information including metadata</returns>
                public static VyTask<VyMultiTokenDto> GetTokenTypeMetadata(int contractId, int tokenTypeId)
                {
                    var reqData = VyRequestData.Get($"/api/contracts/{contractId}/token-types/{tokenTypeId}/metadata",
                        _apiEndpoint);
                    return Request<VyMultiTokenDto>(reqData);
                }

                //todo: check name/data mangling between VyMultiTokenDto & VyTokenTypeDto (including metadata)...
                /// <summary>
                /// Retrieve information (+ metadata) of a single token from one of your contracts
                /// [/api/contracts/:contractId/tokens/:tokenId/metadata]
                /// </summary>
                /// <param name="contractId">Contract ID related to the token</param>
                /// <param name="tokenId">ID of the token</param>
                /// <returns>NFT token Information including metadata</returns>
                public static VyTask<VyMultiTokenDto> GetTokenMetadata(int contractId, int tokenId)
                {
                    var reqData = VyRequestData.Get($"/api/contracts/{contractId}/tokens/{tokenId}/metadata",
                        _apiEndpoint);
                    return Request<VyMultiTokenDto>(reqData);
                }

                /// <summary>
                /// Retrieve all the tokens related to a specific token-type (template)
                /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId/tokens]
                /// </summary>
                /// <param name="applicationId">Your personal application ID</param>
                /// <param name="contractId">Contract ID related to the token-type (nft-template)</param>
                /// <param name="tokenTypeId">ID of the token-type (nft-template)</param>
                /// <returns>List of all the Tokens associated with the given token-type (nft-template)</returns>
                public static VyTask<VyMultiTokenDto[]> GetTokensForType(int contractId, int tokenTypeId)
                {
                    var reqData =
                        VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}/tokens",
                            _apiEndpoint);
                    return Request<VyMultiTokenDto[]>(reqData);
                }
            }

#if (UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_API_SERVER) && !ENABLE_VENLY_PLAYFAB
            public static class Server
            {
                //todo: remove applicationID in params
                /// <summary>
                /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
                /// [/api/minter/contracts]
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The deployed NFT Contract</returns>
                public static VyTask<VyContractDto> CreateContract(VyCreateContractDto reqParams)
                {
                    var reqData = VyRequestData.Post("/api/minter/contracts", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyContractDto>(reqData);
                }

                //todo remove ApplicationID
                /// <summary>
                /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
                /// [/api/minter/contracts/:contractId/token-types]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The deployed NFT Token-Type (Template)</returns>
                public static VyTask<VyTokenTypeDto> CreateTokenType(VyCreateTokenTypeDto reqParams)
                {
                    var reqData = VyRequestData
                        .Post($"/api/minter/contracts/{reqParams.ContractId}/token-types", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTokenTypeDto>(reqData);
                }

                /// <summary>
                /// Mint a Non-Fungible Token (NFT) based on a specific Token-Type (Template)
                /// [/api/minter/contracts/:contractId/tokens/non-fungible]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Information on the Minted Tokens</returns>
                public static VyTask<VyMintedTokenInfoDto> MintTokenNFT(VyMintNonFungibleTokenDto reqParams)
                {
                    var reqData = VyRequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/tokens/non-fungible",
                            _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyMintedTokenInfoDto>(reqData);
                }

                /// <summary>
                /// Mint a Fungible Token (FT) based on a specific Token-Type (Template)
                /// [/api/minter/contracts/:contractId/tokens/fungible]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Information on the Minted Tokens</returns>
                public static VyTask<VyMintedTokenInfoDto> MintTokenFT(VyMintFungibleTokenDto reqParams)
                {
                    var reqData = VyRequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/tokens/fungible",
                            _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyMintedTokenInfoDto>(reqData);
                }

                /// <summary>
                /// Update the metadata of a Token-Type (NFT Template)
                /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Token-Type (Template)</returns>
                public static VyTask<VyTokenTypeMetadataDto> UpdateTokenTypeMetadata(
                    VyUpdateTokenTypeMetadataDto reqParams)
                {
                    var reqData = VyRequestData
                        .Put($"/api/contracts/{reqParams.ContractId}/token-types/{reqParams.TokenTypeId}/metadata",
                            _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTokenTypeMetadataDto>(reqData);
                }

                //todo: check with backend team about this response
                /// <summary>
                /// Update the metadata of a Contract
                /// [/api/contracts/:contractId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Contract Metadata</returns>
                public static VyTask<VyContractMetadataDto> UpdateContractMetadata(
                    VyUpdateContractMetadataDto reqParams)
                {
                    var reqData = VyRequestData.Patch($"/api/contracts/{reqParams.ContractId}/metadata", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyContractMetadataDto>(reqData);
                }

                /// <summary>
                /// Archive a specific Contract. When a contract is archived it is removed from your account together with all the token-types that are associated to that contract
                /// [/api/minter/contracts/:contractId]
                /// </summary>
                /// <param name="contractId"></param>
                /// <returns> void promise </returns>
                public static VyTask<VyTaskVoid> ArchiveContract(string applicationId, int contractId)
                {
                    var reqData = VyRequestData.Delete($"/api/minter/contracts/{contractId}", _apiEndpoint);
                    return Request<VyTaskVoid>(reqData);
                }

                /// <summary>
                /// Archive a specific Token Type (Template)
                /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId]
                /// </summary>
                /// <param name="contractId"></param>
                /// <param name="tokenTypeId"></param>
                /// <returns> void promise </returns>
                public static VyTask<VyTaskVoid> ArchiveTokenType(int contractId, int tokenTypeId)
                {
                    var reqData = VyRequestData.Delete($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}",
                        _apiEndpoint);
                    return Request<VyTaskVoid>(reqData);
                }
            }
#endif
        }
    }
}
