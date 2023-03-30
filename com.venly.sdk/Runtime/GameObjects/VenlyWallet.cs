using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using VenlySDK.Core;
using VenlySDK.Models.Shared;
using VenlySDK.Models.Wallet;

namespace VenlySDK.GameObjects
{
    class VenlyWallet : MonoBehaviour
    {
        public bool IsInitialized { get; set; }

        public bool ConnectOnStartup = false;
        public string TargetWalletId = string.Empty;

        public bool EventPolling = false;
        public float PollInterval = 10.0f;

        [HideInInspector] public VyWalletDto WalletDto;
        [HideInInspector] public VyCryptoTokenDto[] FungibleTokensDto;
        [HideInInspector] public VyMultiTokenDto[] NonFungibleTokensDto;
        [HideInInspector] public VyWalletEventDto[] WalletEventsDto;

        public UnityEvent OnWalletInitialized;

        async void Awake()
        {
            if(!Venly.IsInitialized)
                VenlyUnity.Initialize();

            if (ConnectOnStartup)
            {
                if (!string.IsNullOrWhiteSpace(TargetWalletId))
                {
                    await Connect(TargetWalletId);
                }
                else
                {
                    Debug.LogWarning("VenlyWallet > Connect On Startup Failed (InitWalletId is empty)");
                }
            }
        }

        public void Reset()
        {
            IsInitialized = false;

            TargetWalletId = null;

            WalletDto = null;
            FungibleTokensDto = null;
            NonFungibleTokensDto = null;
            WalletEventsDto = null;
        }

        public VyTask Connect(string walletId)
        {
            //todo: Check if currently connected
            var taskNotifier = VyTask.Create("ConnectWalletId");

            //Reset Current State
            Reset();
            TargetWalletId = walletId;

            //Gather Data
            Task.Run(async () =>
            {
                try
                {
                    //Retrieve Wallet Data
                    WalletDto = await Venly.WalletAPI.Client.GetWallet(walletId).AwaitResult();

                    //Retrieve CryptoTokens
                    FungibleTokensDto = await Venly.WalletAPI.Client.GetCryptoTokenBalances(walletId).AwaitResult();

                    //Retrieve MultiTokens
                    NonFungibleTokensDto = await Venly.WalletAPI.Client.GetMultiTokenBalances(walletId).AwaitResult();

                    //Retrieve WalletEvents
                    WalletEventsDto = await Venly.WalletAPI.Client.GetWalletEvents(walletId).AwaitResult();

                    //Initialize Wallet
                    Initialize();

                    taskNotifier.NotifySuccess();

                    //Callback
                    OnWalletInitialized?.Invoke();
                }
                catch (VyException e)
                {
                    Reset();
                    Debug.LogWarning("VenlyWallet > Failed to Connect (see exception details below)");
                    Debug.LogException(e);
                    taskNotifier.NotifyFail(e);
                }
            });

            return taskNotifier.Task;
        }

        private void Initialize()
        {
            IsInitialized = true;
            Debug.Log("Wallet Initialized!");
        }
    }
}
