using System;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Server;
using Newtonsoft.Json;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;

namespace Venly.Backends.Beamable
{
    public class VenlyMicroservice : Microservice
    {
        private const string WALLET_ID_STAT_KEY = "venly-wallet-id";

        protected async Task<string> HandleRequest(string req)
        {
            try
            {
                //Initialize
                var initResult = await VenlyBeamable.Initialize(Services.RealmConfig, HandleExtension);
                if (!initResult.Success)
                {
                    return VyServerResponseDto.FromException(initResult.Exception).Serialize();
                }

                //Execute Request
                var reqData = JsonConvert.DeserializeObject<VyRequestData>(req);
                var response = await VenlyAPI.Backend.HandleRequest(reqData).AwaitResult();
                return response.Serialize();
            }
            catch (Exception ex)
            {
                return VyServerResponseDto.FromException(ex).Serialize();
            }
        }

        #region EXTENSIONS
        private VyTask<VyServerResponseDto> HandleExtension(VyRequestData requestData, object customData)
        {
            var routeName = requestData.Uri;

            switch (routeName)
            {
                case "user_create_wallet": return Ext_CreateWalletForUser(requestData);
                case "user_get_wallet": return Ext_UserGetWallet(requestData);
                case "meta_transaction_multi_token": return Ext_InvokeMetaTransaction_MultiToken(requestData);
            }

            return OnHandleExtension(requestData, customData);
        }

        protected virtual VyTask<VyServerResponseDto> OnHandleExtension(VyRequestData requestData, object customData)
        {
            return VyTask<VyServerResponseDto>.Succeeded(
                VyServerResponseDto.FromException(
                    new NotSupportedException($"No implementation for \'{requestData.Uri}\' extension.")
                )
            );
        }

        #region Extension Implementation
        private async VyTask<VyServerResponseDto> Ext_CreateWalletForUser(VyRequestData requestData)
        {
            var user = await Services.Auth.GetUser();
            var walletId = await Services.Stats.GetProtectedPlayerStat(user.id, WALLET_ID_STAT_KEY);

            if (string.IsNullOrEmpty(walletId))
            {
                //Create a new wallet
                var createWalletParams = requestData.GetJsonContent<VyCreateWalletRequest>();
                var result = await VenlyAPI.Wallet.CreateWallet(createWalletParams);

                if (result.Success)
                {
                    //Store the wallet id
                    await Services.Stats.SetProtectedPlayerStat(user.id, WALLET_ID_STAT_KEY, result.Data.Id);
                }

                //Return result
                return VyServerResponseDto.FromTaskResult(result, false);
            }

            //Wallet-ID already linked - retrieve wallet only
            var result2 = await VenlyAPI.Wallet.GetWallet(walletId);
            return VyServerResponseDto.FromTaskResult(result2, false);
        }

        private  async VyTask<VyServerResponseDto> Ext_UserGetWallet(VyRequestData requestData)
        {
            var user = await Services.Auth.GetUser();
            var walletId = await Services.Stats.GetProtectedPlayerStat(user.id, WALLET_ID_STAT_KEY);

            if(string.IsNullOrEmpty(walletId))
                return VyServerResponseDto.FromException(new Exception($"Wallet not found for User (BeamableId={user.id})"));

            var result = await VenlyAPI.Wallet.GetWallet(walletId);
            return VyServerResponseDto.FromTaskResult(result, false);
        }

        private async VyTask<VyServerResponseDto> Ext_InvokeMetaTransaction_MultiToken(VyRequestData requestData)
        {
            var txParams = requestData.GetJsonContent<VyExecuteTransactionRequest>();
            var txDetails = txParams.TransactionRequest as VyTransactionMultiTokenTransferRequest;

            var query = VyQuery_GetWallet.Create().IncludeBalance(false);
            var sourceWallet = await VenlyAPI.Wallet.GetWallet(txDetails.WalletId, query).AwaitResult();

            var tokens = await VenlyAPI.Wallet.GetMultiTokenBalances(txDetails.WalletId).AwaitResult();
            var targetToken = tokens.FirstOrDefault(t => t.Id == txDetails.TokenId.Value.ToString());

            if (targetToken == null)
                return VyServerResponseDto.FromException(new ArgumentException(
                    $"Wallet (id=\'{txDetails.WalletId}\') does not have a token with id=\'{txDetails.TokenId}\'"));

            var reqParams = new VyMetaTransferMultiTokenDto()
            {
                SourcePincode = txParams.Pincode,
                SourceWallet = sourceWallet,

                ExecutorPincode = "",
                ExecutorWallet = null,

                Amount = txDetails.Amount ?? 1,
                DestinationAddress = txDetails.ToAddress,
                Token = targetToken
            };

            var response = await VenlyAPI.Wallet.MetaTransaction_TransferMultiToken(reqParams);
            return VyServerResponseDto.FromTaskResult(response, true);
        }
        #endregion
        #endregion
    }
}