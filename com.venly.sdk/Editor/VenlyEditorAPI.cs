using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Proto.Promises;
using Venly.Models;
using Venly.Utils;

namespace Venly.Editor
{
#if UNITY_EDITOR
    internal static class VenlyEditorAPI
    {
        public static bool IsInitialized = false;
        private static VenlyEditorRequester _requester;

        private static bool _useWrapNFT = false;

        static VenlyEditorAPI()
        {
            _requester = new VenlyEditorRequester();
            IsInitialized = true;

            Promise.Config.ForegroundContext = SynchronizationContext.Current;
        }

        #region AUTH
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
        #endregion

        #region SERVER
        /// <summary>
        /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
        /// [/api/apps/:applicationId/contracts]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Contract</returns>
        public static Promise<VyContract> CreateContract(VyParam_CreateContract reqParams)
        {
            return Request_JSON<VyContract, VyParam_CreateContract>(HttpMethod.Post, $"/api/apps/{reqParams.ApplicationId}/contracts", eVyApiEndpoint.Nft, reqParams, _useWrapNFT);
        }

        /// <summary>
        /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
        /// [/api/apps/:applicationId/contracts/:contractId/token-types]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Token-Type (Template)</returns>
        public static Promise<VyTokenType> CreateTokenType(VyParam_CreateTokenType reqParams)
        {
            return Request_JSON<VyTokenType, VyParam_CreateTokenType>(HttpMethod.Post, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/token-types", eVyApiEndpoint.Nft, reqParams, _useWrapNFT);
        }

        /// <summary>
        /// Update the metadata of a Token-Type (NFT Template)
        /// [/api/apps/:applicationId/contracts/:contractId/token-types/:tokenTypeId/metadata]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>Updated Token-Type (Template)</returns>
        public static Promise<VyTokenTypeMetadata> UpdateTokenTypeMetadata(VyParam_UpdateTokenTypeMetadata reqParams)
        {
            return Request_JSON<VyTokenTypeMetadata, VyParam_UpdateTokenTypeMetadata>(HttpMethod.Put, $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/token-types/{reqParams.TokenTypeId}/metadata", eVyApiEndpoint.Nft, reqParams, _useWrapNFT);
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
            return Request_JSON<VyContractMetadata, VyParam_UpdateContractMetadata>(new HttpMethod("PATCH"), $"/api/apps/{reqParams.ApplicationId}/contracts/{reqParams.ContractId}/metadata", eVyApiEndpoint.Nft, reqParams, _useWrapNFT);
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
            Request<string>(HttpMethod.Delete, $"/api/apps/{applicationId}/contracts/{contractId}", eVyApiEndpoint.Nft, _useWrapNFT)
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
            Request<string>(HttpMethod.Delete, $"/api/apps/{applicationId}/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Nft, _useWrapNFT)
                .Then(ret => deferredPromise.Resolve())
                .CatchAndForget(deferredPromise);

            return deferredPromise.Promise;
        }
        #endregion

        #region CLIENT
        public static Promise<VyApp[]> GetApps()
        {
            return Request<VyApp[]>(HttpMethod.Get, "/api/apps", eVyApiEndpoint.Nft, _useWrapNFT);
        }

        /// <summary>
        /// Retrieve information of all the NFT contracts associated with a specific application ID
        /// [/api/apps/:applicationId/contracts]
        /// </summary>
        /// <param name="applicationId">The applicationID associated with the retrieved contracts</param>
        /// <returns>List of Contract Information</returns>
        public static Promise<VyContract[]> GetContracts(string applicationId)
        {
            return Request<VyContract[]>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts", eVyApiEndpoint.Nft, _useWrapNFT);
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
            return Request<VyContract>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}", eVyApiEndpoint.Nft, _useWrapNFT);
        }

        /// <summary>
        /// Retrieve information of all NFT token types (templates) from one of your contracts
        /// </summary>
        /// <param name="applicationId">The applicationID associated with the contract</param>
        /// <param name="contractId">The ID of the contract you want the token type information from</param>
        /// <returns>List NFT token type (template) information</returns>
        public static Promise<VyTokenType[]> GetTokenTypes(string applicationId, int contractId)
        {
            return Request<VyTokenType[]>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/token-types", eVyApiEndpoint.Nft, _useWrapNFT);
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
            return Request<VyTokenType>(HttpMethod.Get, $"/api/apps/{applicationId}/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Nft, _useWrapNFT);
        }
        #endregion


        #region Request Helpers
        private static Exception VerifyRequest()
        {
            if (!IsInitialized) return new VenlyException("VenlyEditorAPI not yet initialized!");
            if (_requester == null) return new VenlyException("VenlyAPI requester is null");

            return null!;
        }

        private static Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, Dictionary<string, object> jsonData, bool wrap = true)
        {
            var ex = VerifyRequest();
            return ex != null ? Promise<T>.Rejected(ex) : _requester.Request<T>(method, uri, endpoint, jsonData, wrap);
        }

        private static Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, Dictionary<string, string> formData, bool wrap = true)
        {
            var ex = VerifyRequest();
            return ex != null ? Promise<T>.Rejected(ex) : _requester.Request<T>(method, uri, endpoint, formData, wrap);
        }

        private static Promise<T> Request<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint, bool wrap = true)
        {
            var ex = VerifyRequest();
            return ex != null ? Promise<T>.Rejected(ex) : _requester.Request<T>(method, uri, endpoint, wrap);
        }

        private static Promise<T> Request_FORM<T, TBody>(HttpMethod method, string uri, eVyApiEndpoint endpoint, TBody body, bool wrap = true)
        {
            var ex = VerifyRequest();
            return ex != null ? Promise<T>.Rejected(ex) : _requester.Request_FORM<T, TBody>(method, uri, endpoint, body, wrap);
        }

        private static Promise<T> Request_JSON<T, TBody>(HttpMethod method, string uri, eVyApiEndpoint endpoint, TBody body, bool wrap = true)
        {
            var ex = VerifyRequest();
            return ex != null ? Promise<T>.Rejected(ex) : _requester.Request_JSON<T, TBody>(method, uri, endpoint, body, wrap);
        }
        #endregion
    }
#endif
}
