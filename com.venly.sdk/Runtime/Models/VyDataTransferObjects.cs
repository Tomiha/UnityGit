using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using VenlySDK.Models.Shared;
using VenlySDK.Models.Wallet;
using VenlySDK.Utils;

namespace VenlySDK.Models
{
    public class VyMetaTransferMultiTokenDto
    {
        public string SourcePincode { get; set; }
        public string ExecutorPincode { get; set; }
        public VyMultiTokenDto Token { get; set; }
        public VyWalletDto SourceWallet { get; set; }
        public VyWalletDto ExecutorWallet { get; set; }
        public string DestinationAddress { get; set; }
        public int Amount { get; set; }
    }

    internal static class TransactionHelpers
    {
        public static string CreateFunctionSignature_safeTransferFrom(string fromAddress, string toAddress, string tokenId, int amount = 1)
        {
            return "0xf242432a" +
                   $"{fromAddress.Replace("0x", "").PadLeft(64, '0')}" +
                   $"{toAddress.Replace("0x", "").PadLeft(64, '0')}" +
                   $"{HexPad(tokenId)}" +
                   $"{HexPad(amount)}" +
                   "00000000000000000000000000000000000000000000000000000000000000a00000000000000000000000000000000000000000000000000000000000000000";
        }

        private static string HexPad(string input, int padLength = 64)
        {
            var number = int.Parse(input);
            return HexPad(number);
        }

        private static string HexPad(int input, int padLength = 64)
        {
            var hexString = $"{input:x}";

            return hexString.PadLeft(padLength, '0');
        }

        public static int GetChainId(eVyChain chain, eVyEnvironment env)
        {
            var isStaging = env == eVyEnvironment.staging;
            //TODO: Verify IDs
            switch (chain)
            {
                case eVyChain.Matic: return (isStaging) ? 80001 : 137;
                case eVyChain.Ethereum: return (isStaging) ? 5 : 1;
                case eVyChain.Avac: return (isStaging) ? 43113 : 43114;
                case eVyChain.Bsc: return (isStaging) ? 97 : 56;
            }

            throw new NotSupportedException($"GetChainID for {chain.GetMemberName()} ({env.GetMemberName()}) is not supported.");
        }

        public static string CreateEIP721Document(VyMetaTransferMultiTokenDto data, string nonce, out string functionSignature)
        {
            //Function Signature
            functionSignature = CreateFunctionSignature_safeTransferFrom(data.SourceWallet.Address,
                data.DestinationAddress, data.Token.Id, data.Amount);

            //Salt
            var chainId = $"{GetChainId(data.SourceWallet.Chain, Venly.CurrentEnvironment):x}";
            var salt = $"0x{chainId.PadLeft(64, '0')}";

            return
                $"{{\"types\":{{\"EIP712Domain\":[{{\"name\":\"name\",\"type\":\"string\"}},{{\"name\":\"version\",\"type\":\"string\"}},{{\"name\":\"verifyingContract\",\"type\":\"address\"}},{{\"name\":\"salt\",\"type\":\"bytes32\"}}],\"MetaTransaction\":[{{\"name\":\"nonce\",\"type\":\"uint256\"}},{{\"name\":\"from\",\"type\":\"address\"}},{{\"name\":\"functionSignature\",\"type\":\"bytes\"}}]}},\"domain\":{{\"name\":\"{data.Token.Contract.Name}\",\"version\":\"1\",\"verifyingContract\":\"{data.Token.Contract.Address}\",\"salt\":\"{salt}\"}},\"primaryType\":\"MetaTransaction\",\"message\":{{\"nonce\":{nonce},\"from\":\"{data.SourceWallet.Address}\",\"functionSignature\":\"{functionSignature}\"}}}}";
        }
    }
}
