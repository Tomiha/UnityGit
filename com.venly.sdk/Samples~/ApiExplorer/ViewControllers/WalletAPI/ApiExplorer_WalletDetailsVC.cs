using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_WalletDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_WALLET = "wallet";

    //DATA
    private VyWalletDto _wallet;
    private VyCryptoTokenDto[] _cryptoTokens;
    private VyMultiTokenDto[] _multiTokens;
    private VyMultiTokenDto[] _multiTokens_FT;
    private VyMultiTokenDto[] _multiTokens_NFT;
    private VyWalletEventDto[] _walletEvents;

    public ApiExplorer_WalletDetailsVC() :
        base(eApiExplorerViewId.WalletApi_WalletDetails)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-cryptotokens", OnClick_ShowCryptoTokens);
        BindButton("btn-multitokens-f", OnClick_ShowMultiTokens_FT);
        BindButton("btn-multitokens-nf", OnClick_ShowMultiTokens_NFT);
        BindButton("btn-events", OnClick_ShowWalletEvents);

        BindButton("btn-archive", OnClick_Archive);
        BindButton("btn-transfer", OnClick_Transfer);

        //Only show Archive Button when DevMode is active
        ToggleElement("btn-archive", VenlySettings.BackendProvider == eVyBackendProvider.DevMode);
    }

    protected override void OnActivate()
    {
        TryGetBlackboardData(out _wallet, DATAKEY_WALLET, ApiExplorer_GlobalKeys.DATA_UserWallet);
    }

    protected override async Task OnRefreshData()
    {
        //Retrieve Wallet Data
        //--------------------
        ViewManager.Loader.Show("Retrieving Wallet Info...");
        var result = await VenlyAPI.Wallet.GetWallet(_wallet.Id);
        ViewManager.Loader.Hide();

        if (!result.Success)
        {
            ViewManager.HandleException(result.Exception);
            return;
        }

        _wallet = result.Data;
        SetBlackboardData(DATAKEY_WALLET, _wallet); //Update Blackboard (contains Balance now)

        //Retrieve MultiToken Balances
        //----------------------------
        ViewManager.Loader.Show("Retrieving ERC1155/721 Tokens...");
        var nftResult = await VenlyAPI.Wallet.GetMultiTokenBalances(_wallet.Id);
        ViewManager.Loader.Hide();

        if (!nftResult.Success)
        {
            ViewManager.HandleException(result.Exception);
            _multiTokens = Array.Empty<VyMultiTokenDto>();
            _multiTokens_FT = _multiTokens;
            _multiTokens_NFT = _multiTokens;
        }
        else
        {
            _multiTokens = nftResult.Data;
            _multiTokens_FT = _multiTokens.Where(t => t.Fungible).ToArray();
            _multiTokens_NFT = _multiTokens.Where(t => !t.Fungible).ToArray();
        }

        //Retrieve CryptoToken Balances
        //-----------------------------
        ViewManager.Loader.Show("Retrieving ERC20 Tokens...");
        var ftResult = await VenlyAPI.Wallet.GetCryptoTokenBalances(_wallet.Id);
        ViewManager.Loader.Hide();

        if (!ftResult.Success)
        {
            ViewManager.HandleException(ftResult.Exception);
            _cryptoTokens = Array.Empty<VyCryptoTokenDto>();
        }
        else _cryptoTokens = ftResult.Data;

        //Retrieve Events
        //---------------
        ViewManager.Loader.Show("Retrieving Wallet Events...");
        var eventsResult = await VenlyAPI.Wallet.GetWalletEvents(_wallet.Id);
        ViewManager.Loader.Hide();

        if (!eventsResult.Success)
        {
            ViewManager.HandleException(ftResult.Exception);
            _walletEvents = Array.Empty<VyWalletEventDto>();
        }
        else _walletEvents = eventsResult.Data;

    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_wallet, DATAKEY_WALLET)) return;
        if (!ValidateData(_cryptoTokens, "cryptoTokens")) return;
        if (!ValidateData(_multiTokens_FT, "multiTokens_ft")) return;
        if (!ValidateData(_multiTokens_NFT, "multiTokens_nft")) return;
        if (!ValidateData(_walletEvents, "events")) return;

        //Refresh UI Elements
        SetLabel("btn-transfer", $"Transfer {_wallet.Balance.Symbol}");
        SetLabel("lbl-wallet-address", _wallet.Address);
        SetLabel("lbl-wallet-id", _wallet.Id);
        SetLabel("lbl-wallet-chain", _wallet.Chain.GetMemberName());
        SetLabel("lbl-wallet-description", _wallet.Description);
        SetLabel("lbl-wallet-identifier", _wallet.Identifier);
        SetLabel("lbl-wallet-recoverable", _wallet.WalletType == eVyWalletType.WhiteLabel ? "YES" : "NO");
        SetLabel("lbl-wallet-balance", $"{_wallet.Balance.Balance} {_wallet.Balance.Symbol}");

        //Token Data
        SetLabel("lbl-cryptotoken-amount", $"{_cryptoTokens.Length} Token(s)");
        SetLabel("lbl-multitoken-ft-amount", $"{_multiTokens_FT.Length} Token(s)");
        SetLabel("lbl-multitoken-nft-amount", $"{_multiTokens_NFT.Length} Token(s)");
        SetLabel("lbl-wallet-event-amount", $"{_walletEvents.Length} Event(s)");

        //Archived State
        View.rootVisualElement.SetButtonLabel("btn-archive", _wallet.Archived ? "Unarchive" : "Archive");
        View.rootVisualElement.SetDisplay("lbl-archived", _wallet.Archived);
    }
    #endregion

    #region EVENTS
    private async void OnClick_Archive()
    {
        VyTaskResult<VyWalletDto> result = null;
        if (!_wallet.Archived)
        {
            ViewManager.Loader.Show("Archiving Wallet..");
            result = await VenlyAPI.Wallet.ArchiveWallet(_wallet.Id);
        }
        else
        {
            ViewManager.Loader.Show("Unarchiving Wallet..");
            result = await VenlyAPI.Wallet.UnarchiveWallet(_wallet.Id);
        }
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            _wallet.CopyMetadataFrom(result.Data);
            Refresh(false);

            //Also update cached wallet
            if (ViewManager.HasGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached))
            {
                var cachedWallets =
                    ViewManager.GetGlobalBlackBoardData<VyWalletDto[]>(ApiExplorer_GlobalKeys.DATA_AllWalletsCached);
                var targetWallet = cachedWallets.FirstOrDefault(w => w.Id == _wallet.Id);

                if (targetWallet != null)
                {
                    targetWallet.CopyMetadataFrom(result.Data);
                    ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllWalletsCached, cachedWallets);
                }
            }
        }
        else ViewManager.HandleException(result.Exception);
    }

    private void OnClick_Transfer()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNativeToken, ApiExplorer_TransferNativeTokenVC.DATAKEY_WALLET, _wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferNativeToken, ViewId, false);
    }

    private void OnClick_ShowCryptoTokens()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewCryptoTokens, ApiExplorer_ViewCryptoTokensVC.DATAKEY_WALLET, _wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewCryptoTokens, ApiExplorer_ViewCryptoTokensVC.DATAKEY_TOKENLIST, _cryptoTokens);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewCryptoTokens, ViewId, false);
    }

    private void OnClick_ShowMultiTokens_FT()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, ApiExplorer_ViewMultiTokensVC.DATAKEY_WALLET, _wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, ApiExplorer_ViewMultiTokensVC.DATAKEY_TOKENLIST, _multiTokens_FT);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewMultiTokens, ViewId, false);
    }

    private void OnClick_ShowMultiTokens_NFT()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, ApiExplorer_ViewMultiTokensVC.DATAKEY_WALLET, _wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, ApiExplorer_ViewMultiTokensVC.DATAKEY_TOKENLIST, _multiTokens_NFT);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewMultiTokens, ViewId, false);
    }

    private void OnClick_ShowWalletEvents()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewWalletEvents, ApiExplorer_ViewWalletEventsVC.DATAKEY_EVENTS, _walletEvents);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewWalletEvents, ViewId, false);
    }
    #endregion
}
