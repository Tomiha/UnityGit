using System;
using System.Linq;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Core;
using VenlySDK.Models.Shared;
using VenlySDK.Models.Wallet;
using VenlySDK.Utils;

public class ApiExplorer_WalletDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    public const string DATAKEY_WALLET = "wallet";

    private string _lastLoadedWalletId = null;
    private VyWalletDto _walletData;
    private VyCryptoTokenDto[] _cryptoTokens;
    private VyMultiTokenDto[] _multiTokens;
    private VyMultiTokenDto[] _multiTokens_FT;
    private VyMultiTokenDto[] _multiTokens_NFT;
    private VyWalletEventDto[] _walletEvents;

    public ApiExplorer_WalletDetailsVC() :
        base(eApiExplorerViewId.WalletApi_WalletDetails)
    { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-cryptotokens", onClick_ShowCryptoTokens);
        BindButton("btn-multitokens-f", onClick_ShowMultiTokens_FT);
        BindButton("btn-multitokens-nf", onClick_ShowMultiTokens_NFT);
        BindButton("btn-events", onClick_ShowWalletEvents);

        BindButton("btn-archive", onClick_Archive);
        BindButton("btn-transfer", onClick_Transfer);

        //Only show Archive Button when DevMode is active
        ToggleElement("btn-archive", VenlySettings.BackendProvider == eVyBackendProvider.DevMode);
    }

    protected override void OnActivate()
    {
        _walletData = null;

        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = true;

        if (!HasBlackboardData(DATAKEY_WALLET))
        {
            throw new Exception($"WalletDetailsVC >> BlackboardData \'{DATAKEY_WALLET}\' not found...");
        }

        _walletData = GetBlackBoardData<VyWalletDto>(DATAKEY_WALLET);
        SetLabel("btn-transfer", $"Transfer {_walletData.Balance.Symbol}");

        RefreshWallet();
    }

    protected override void OnClick_NavigateBack()
    {
        if (PreviousView.ViewId == eApiExplorerViewId.WalletApi_CreateWallet)
        {
            ViewManager.SwitchView(PreviousView.PreviousView, false);
        }
        else
        {
            base.OnClick_NavigateBack();
        }
    }

    protected override void OnClick_Refresh()
    {
        RefreshWallet(true);
    }


    private
#if ENABLE_VENLY_DEVMODE
    async
#endif
    void onClick_Archive()
    {
        #region DevMode Only (SERVER)
#if ENABLE_VENLY_DEVMODE
        VyTaskResult<VyWalletMetadataResponseDto> result = null;
        if (!_walletData.Archived)
        {
            ViewManager.Loader.Show("Archiving Wallet...");
            result = await Venly.WalletAPI.Server.ArchiveWallet(_walletData.Id);
        }
        else
        {
            ViewManager.Loader.Show("Unarchiving Wallet...");
            result = await Venly.WalletAPI.Server.UnarchiveWallet(_walletData.Id);
        }
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            _walletData.UpdateFromMetadataResponse(result.Data);
            RefreshWallet();

            //Also update cached wallet
            if (ViewManager.HasGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached))
            {
                var cachedWallets =
                    ViewManager.GetGlobalBlackBoardData<VyWalletDto[]>(ApiExplorer_GlobalKeys.DATA_AllWalletsCached);
                var targetWallet = cachedWallets.FirstOrDefault(w => w.Id == _walletData.Id);

                if (targetWallet != null)
                {
                    targetWallet.UpdateFromMetadataResponse(result.Data);
                    ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached, cachedWallets);
                }
            }
        }
        else ViewManager.HandleException(result.Exception);
#endif
        #endregion
    }

    private void onClick_Transfer()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNativeToken, "sourceWallet", _walletData);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferNativeToken);
    }

    private void onClick_ShowCryptoTokens()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewCryptoTokens, "sourceWallet", _walletData);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewCryptoTokens, "tokenList", _cryptoTokens);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewCryptoTokens);
    }

    private void onClick_ShowMultiTokens_FT()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, "sourceWallet", _walletData);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, "tokenList", _multiTokens_FT);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewMultiTokens);
    }

    private void onClick_ShowMultiTokens_NFT()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, "sourceWallet", _walletData);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, "tokenList", _multiTokens_NFT);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewMultiTokens);
    }

    private void onClick_ShowWalletEvents()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewWalletEvents, "eventList", _walletEvents);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewWalletEvents);
    }

    private async void RefreshWallet(bool forceFreshLoad = false)
    {
        if (forceFreshLoad || _walletData.Id != _lastLoadedWalletId)
        {
            //Retrieve Wallet Data
            //--------------------
            ViewManager.Loader.Show("Retrieving Wallet Info...");
            var result = await Venly.WalletAPI.Client.GetWallet(_walletData.Id);
            ViewManager.Loader.Hide();

            if (!result.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _walletData = result.Data;


            //Retrieve MultiToken Balances
            //----------------------------
            ViewManager.Loader.Show("Retrieving ERC1155/721 Tokens...");
            var nftResult = await Venly.WalletAPI.Client.GetMultiTokenBalances(_walletData.Id);
            ViewManager.Loader.Hide();

            if (!nftResult.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _multiTokens = nftResult.Data;
            _multiTokens_FT = _multiTokens.Where(t => t.Fungible).ToArray();
            _multiTokens_NFT = _multiTokens.Where(t => !t.Fungible).ToArray();

            //Retrieve CryptoToken Balances
            //-----------------------------
            ViewManager.Loader.Show("Retrieving ERC20 Tokens...");
            var ftResult = await Venly.WalletAPI.Client.GetCryptoTokenBalances(_walletData.Id);
            ViewManager.Loader.Hide();

            if (!ftResult.Success)
            {
                ViewManager.HandleException(ftResult.Exception);
                return;
            }
            _cryptoTokens = ftResult.Data;

            //Retrieve Events
            //---------------
            ViewManager.Loader.Show("Retrieving Wallet Events...");
            var eventsResult = await Venly.WalletAPI.Client.GetWalletEvents(_walletData.Id);
            ViewManager.Loader.Hide();

            if (!eventsResult.Success)
            {
                ViewManager.HandleException(ftResult.Exception);
                return;
            }
            _walletEvents = eventsResult.Data;
        }

        _lastLoadedWalletId = _walletData.Id;

        //Refresh UI Elements
        SetLabel("lbl-wallet-address", _walletData.Address);
        SetLabel("lbl-wallet-id", _walletData.Id);
        SetLabel("lbl-wallet-chain", _walletData.Chain.GetMemberName());
        SetLabel("lbl-wallet-description", _walletData.Description);
        SetLabel("lbl-wallet-identifier", _walletData.Identifier);
        SetLabel("lbl-wallet-recoverable", _walletData.WalletType == eVyWalletType.WhiteLabel ? "YES" : "NO");
        SetLabel("lbl-wallet-balance", $"{_walletData.Balance.Balance} {_walletData.Balance.Symbol}");

        //Token Data
        SetLabel("lbl-cryptotoken-amount", $"{_cryptoTokens.Length} Token(s)");
        SetLabel("lbl-multitoken-ft-amount", $"{_multiTokens_FT.Length} Token(s)");
        SetLabel("lbl-multitoken-nft-amount", $"{_multiTokens_NFT.Length} Token(s)");
        SetLabel("lbl-wallet-event-amount", $"{_walletEvents.Length} Event(s)");

        //Archived State
        RefreshArchivedState();
    }

    private void RefreshArchivedState()
    {
        //View.rootVisualElement.SetDisplay("btn-archive", !_walletData.Archived);
        View.rootVisualElement.SetButtonLabel("btn-archive", _walletData.Archived ? "Unarchive" : "Archive");
        View.rootVisualElement.SetDisplay("lbl-archived", _walletData.Archived);
    }
}
