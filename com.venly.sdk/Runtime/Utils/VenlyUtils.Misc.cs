using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using VenlySDK.Models;

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
            {eVyApiEndpoint.Market, @"https://api-staging.venly.market"},
            //{eVyApiEndpoint.Matic, @"https://matic-azrael-staging.venly.io"},
            //{eVyApiEndpoint.Bsc, @"https://bsc-azrael-staging.venly.io"},
            //{eVyApiEndpoint.Ethereum, @"https://ethereum-azrael-staging.venly.io"}
        };

        public static string GetUrl(string uri, eVyApiEndpoint endpoint, eVyEnvironment env = eVyEnvironment.staging)
        {
            return $"{GetBaseUrl(endpoint, env)}{uri}";
        }

        public static string GetBaseUrl(eVyApiEndpoint endpoint, eVyEnvironment env = eVyEnvironment.staging)
        {
            if (env == eVyEnvironment.staging) return _stagingEndpoints[endpoint];

            throw new NotImplementedException();
        }

        #endregion

        #region Conversions

        public static FormUrlEncodedContent ConvertToFormContent<T>(T obj)
        {
            var formData = new Dictionary<string, string>();

            var jsonObj = JObject.FromObject(obj);
            foreach (var property in jsonObj)
            {
                if (property.Value != null)
                    formData.Add(property.Key, property.Value.ToString());
            }

            return new FormUrlEncodedContent(formData);
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