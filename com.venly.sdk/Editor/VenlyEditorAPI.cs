using System;
using System.Collections.Generic;
using System.Threading;
using VenlySDK.Core;
using VenlySDK.Editor.Utils;
using VenlySDK.Models;


namespace VenlySDK.Editor
{
#if UNITY_EDITOR
    internal static class VenlyEditorAPI
    {
        public static bool IsInitialized = false;
        private static VyEditorRequester _requester;

        static VenlyEditorAPI()
        {
            _requester = new VyEditorRequester();
            IsInitialized = true;

            //Make sure the Task System is initialized
            VyTaskBase.Initialize();
        }

        #region AUTH
        public static VyTask<VyAccessTokenDto> GetAccessToken(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return VyTask<VyAccessTokenDto>.Failed(VyException.Argument("Client-ID cannot be NULL or empty", nameof(clientId)));

            if (string.IsNullOrWhiteSpace(clientSecret))
                return VyTask<VyAccessTokenDto>.Failed(VyException.Argument("Client-SECRET cannot be NULL or empty", nameof(clientSecret)));

            var formData = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", clientId},
                {"client_secret", clientSecret}
            };

            var reqData = VyRequestData.Post("/auth/realms/Arkane/protocol/openid-connect/token", eVyApiEndpoint.Auth)
                .AddFormContent(formData);
            return Request<VyAccessTokenDto>(reqData);
        }
        #endregion

        #region SERVER
        /// <summary>
        /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
        /// [/api/minter/contracts]
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Contract</returns>
        public static VyTask<VyContractDto> CreateContract(VyCreateContractDto reqParams)
        {
            var reqData = VyRequestData.Post("/api/minter/contracts", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyContractDto>(reqData);
        }

        /// <summary>
        /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
        /// [/api/minter/contracts/:contractId/token-types]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Token-Type (Template)</returns>
        public static VyTask<VyTokenTypeDto> CreateTokenType(VyCreateTokenTypeDto reqParams)
        {
            var reqData = VyRequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/token-types", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyTokenTypeDto>(reqData);
        }

        /// <summary>
        /// Update the metadata of a Token-Type (NFT Template)
        /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>Updated Token-Type (Template)</returns>
        public static VyTask<VyTokenTypeMetadataDto> UpdateTokenTypeMetadata(VyUpdateTokenTypeMetadataDto reqParams)
        {
            var reqData = VyRequestData.Put($"/api/contracts/{reqParams.ContractId}/token-types/{reqParams.TokenTypeId}/metadata", eVyApiEndpoint.Nft)
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
        public static VyTask<VyContractMetadataDto> UpdateContractMetadata(VyUpdateContractMetadataDto reqParams)
        {
            var reqData = VyRequestData.Patch($"/api/contracts/{reqParams.ContractId}/metadata", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyContractMetadataDto>(reqData);
        }

        /// <summary>
        /// Archive a specific Contract. When a contract is archived it is removed from your account together with all the token-types that are associated to that contract
        /// [/api/minter/contracts/:contractId]
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns> void promise </returns>
        public static VyTask<VyTaskVoid> ArchiveContract(int contractId)
        {
            var reqData = VyRequestData.Delete($"/api/minter/contracts/{contractId}", eVyApiEndpoint.Nft);
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
            var reqData = VyRequestData.Delete($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Nft);
            return Request<VyTaskVoid>(reqData);
        }
        #endregion

        #region CLIENT
        /// <summary>
        /// Retrieve the supported chains for the Wallet API
        /// [/api/chains]
        /// </summary>
        /// <returns>List of supported chains</returns>
        public static VyTask<eVyChain[]> GetChainsWALLET()
        {
            var taskNotifier = VyTask<eVyChain[]>.Create();

            var reqData = VyRequestData.Get("/api/chains", eVyApiEndpoint.Wallet);
            Request<eVyChainFULL[]>(reqData)
                .OnComplete(result =>
                {
                    if (result.Success)
                    {
                        taskNotifier.NotifySuccess(VenlyEditorUtils.TrimUnsupportedChains(result.Data));
                    }
                    else
                    {
                        taskNotifier.NotifyFail(result.Exception);
                    }
                });

            return taskNotifier.Task;
        }

        /// <summary>
        /// Retrieve a list of all the BlockChains that are supported by the NFT API
        /// [/api/env]
        /// </summary>
        /// <returns>List of supported BlockChains</returns>
        public static VyTask<eVyChain[]> GetChainsNFT()
        {
            var taskNotifier = VyTask<eVyChain[]>.Create();

            var reqData = VyRequestData.Get("/api/env", eVyApiEndpoint.Nft)
                .SelectProperty("supportedChainsForItemCreation");

            Request<eVyChainFULL[]>(reqData)
                .OnComplete(result =>
                {
                    if (result.Success)
                    {
                        taskNotifier.NotifySuccess(VenlyEditorUtils.TrimUnsupportedChains(result.Data));
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
            var reqData = VyRequestData.Get("/api/minter/contracts", eVyApiEndpoint.Nft);
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
            var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}", eVyApiEndpoint.Nft);
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
            var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types", eVyApiEndpoint.Nft);
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
            var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Nft);
            return Request<VyTokenTypeDto>(reqData);
        }
        #endregion


        #region Request Helpers
        private static Exception VerifyRequest()
        {
            if (!VenlyEditorSettings.Instance.SettingsLoaded)
            {
                VenlySettings.Load(); //Force Settings Load
                VenlyDebugEd.LogDebug("[VenlyEditorAPI] VenlySettings Force Loading", 1);
            }

            if (!IsInitialized) return new VyException("VenlyEditorAPI not yet initialized!");
            if (_requester == null) return new VyException("VenlyAPI requester is null");

            return null!;
        }

        private static VyTask<T> Request<T>(VyRequestData requestData)
        {
            var ex = VerifyRequest();
            //requestData.StackTrace = new StackTrace(true);
            //requestData.CallingOrigin = requestData.StackTrace.GetFrame(1).ToString();
            return ex != null ? VyTask<T>.Failed(ex) : _requester.MakeRequest<T>(requestData);
        }
        #endregion
    }
#endif
}
