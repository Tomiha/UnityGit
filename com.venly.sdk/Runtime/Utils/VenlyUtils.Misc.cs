using System;
using System.Collections.Generic;
using System.Linq;
using VenlySDK.Models.Shared;

namespace VenlySDK.Utils
{
    //Miscellaneous Utilities
    public static partial class VenlyUtils
    {
        #region Endpoint Helpers

        private static readonly Dictionary<eVyApiEndpoint, string> _stagingEndpoints = new()
        {
            {eVyApiEndpoint.None, ""},
            {eVyApiEndpoint.Auth, @"https://login-staging.arkane.network"},
            {eVyApiEndpoint.Wallet, @"https://api-wallet-staging.venly.io"},
            {eVyApiEndpoint.Nft, @"https://api-business-staging.venly.io"},
            {eVyApiEndpoint.Market, @"https://api-staging.venly.market"}
        };

        private static readonly Dictionary<eVyApiEndpoint, string> _productionEndpoints = new()
        {
            { eVyApiEndpoint.None, "" },
            { eVyApiEndpoint.Auth, @"https://login.arkane.network" },
            { eVyApiEndpoint.Wallet, @"https://api-wallet.venly.io" },
            { eVyApiEndpoint.Nft, @"https://api-business.venly.io" },
            { eVyApiEndpoint.Market, @"https://api.venly.market" }
        };

        public static string GetUrl(string uri, eVyApiEndpoint endpoint, VyAccessTokenDto token)
        {
            return $"{GetBaseUrl(endpoint, token.Environment)}{uri}";
        }

        public static string GetUrl(string uri, eVyApiEndpoint endpoint, eVyEnvironment env = eVyEnvironment.staging)
        {
            return $"{GetBaseUrl(endpoint, env)}{uri}";
        }

        public static string GetBaseUrl(eVyApiEndpoint endpoint, eVyEnvironment env = eVyEnvironment.staging)
        {
            if (env == eVyEnvironment.production) return _productionEndpoints[endpoint];
            return _stagingEndpoints[endpoint];
        }

        #endregion

        internal static eVyChain[] TrimUnsupportedChains(eVyChainFULL[] input)
        {
            var supportedChains = (eVyChain[])Enum.GetValues(typeof(eVyChain));
            var filteredList = new List<eVyChain>();

            foreach (var supported in supportedChains)
            {
                if(input.Any(inputChain => inputChain.GetMemberName() == supported.GetMemberName()))
                    filteredList.Add(supported);
            }

            return filteredList.ToArray();
        }
    }
}