using System;
using System.Linq;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Utils;

public class ApiExplorer_WalletDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    public const string DATAKEY_WALLET = "wallet";

    private string _lastLoadedWalletId = null;
    private VyWalletDto _walletData;
    private VyCryptoToken[] _cryptoTokens;
    private VyMultiTokenDto[] _multiTokens;
    private VyMultiTokenDto[] _multiTokens_FT;
    private VyMultiTokenDto[] _multiTokens_NFT;
    private VyWalletEventDto[] _walletEvents;

    //UI References
    private Label _lblWalletAddress;
    private Label _lblWalletId;
    private Label _lblChain;
    private Label _lblDescription;
    private Label _lblIdentifier;
    private Label _lblIsRecoverable;
    private Label _lblBalance;

    private ListView _lstWalletEvents;
    private Foldout _fldWalletEvents;

    private ListView _lstCryptoTokens;
    private Foldout _fldCryptoTokens;

    private Foldout _fldMultiTokens;
    private Foldout _fldMultiTokens_NFT;
    private Foldout _fldMultiTokens_FT;
    private ListView _lstMultiTokens_NFT;
    private ListView _lstMultiTokens_FT;

    public ApiExplorer_WalletDetailsVC() :
        base(eApiExplorerViewId.WalletApi_WalletDetails) { }

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _lblWalletAddress, "lbl-wallet-address");
        GetElement(out _lblWalletId, "lbl-wallet-id");
        GetElement(out _lblChain, "lbl-wallet-chain");
        GetElement(out _lblDescription, "lbl-wallet-description");
        GetElement(out _lblIdentifier, "lbl-wallet-identifier");
        GetElement(out _lblIsRecoverable, "lbl-wallet-recoverable");
        GetElement(out _lblBalance, "lbl-wallet-balance");

        GetElement(out _lstWalletEvents, "lst-wallet-events");
        GetElement(out _fldWalletEvents, "fld-wallet-events");

        GetElement(out _lstCryptoTokens, "lst-crypto-tokens");
        GetElement(out _fldCryptoTokens, "fld-crypto-tokens");

        GetElement(out _fldMultiTokens, "fld-multi-tokens");
        GetElement(out _fldMultiTokens_NFT, "fld-multi-nonfungible");
        GetElement(out _lstMultiTokens_NFT, "lst-multi-nonfungible");
        GetElement(out _fldMultiTokens_FT, "fld-multi-fungible");
        GetElement(out _lstMultiTokens_FT, "lst-multi-fungible");

        BindButton("btn-archive", OnClick_Archive);

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


        RefreshWallet();
    }

    protected override void OnDeactivate()
    {
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

#if ENABLE_VENLY_DEVMODE
    private async void OnClick_Archive()
    {
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
    }
#else
    private void OnClick_Archive()
    {
    }
#endif


    private async void RefreshWallet(bool forceFreshLoad = false)
    {
        if (forceFreshLoad || _walletData.Id != _lastLoadedWalletId)
        {
            //Retrieve Wallet Data
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
        _lblWalletAddress.text = _walletData.Address;
        _lblWalletId.text = _walletData.Id;
        _lblChain.text = _walletData.Chain.GetMemberName();
        _lblDescription.text = _walletData.Description;
        _lblIdentifier.text = _walletData.Identifier;
        _lblIsRecoverable.text = _walletData.WalletType == eVyWalletType.WhiteLabel ? "YES" : "NO";
        _lblBalance.text = $"{_walletData.Balance.Balance} {_walletData.Balance.Symbol}";

        RefreshList_WalletEvents();
        RefreshList_CryptoTokens();
        RefreshList_MultiTokens();

        RefreshArchivedState();
    }

    private void RefreshArchivedState()
    {
        //View.rootVisualElement.SetDisplay("btn-archive", !_walletData.Archived);
        View.rootVisualElement.SetButtonLabel("btn-archive", _walletData.Archived?"Unarchive":"Archive");
        View.rootVisualElement.SetDisplay("lbl-archived", _walletData.Archived);
    }

    private void RefreshList_WalletEvents()
    {
        _fldWalletEvents.text = $"Events ({_walletEvents.Length})";
        _fldWalletEvents.value = false;

        //Make sure other foldouts are collapsed on expand
        _fldWalletEvents.RegisterValueChangedCallback((evtArgs) =>
        {
            if (evtArgs.newValue)
            {
                _fldMultiTokens.value = false;
                _fldCryptoTokens.value = false;
            }
        });

        _lstWalletEvents.makeItem = () => new Label();
        _lstWalletEvents.bindItem = (element, i) =>
        {
            var lbl = element as Label;
            var walletEvent = _walletEvents[i];
            lbl.text = walletEvent.EventType;
        };

        _lstWalletEvents.itemsSource = _walletEvents;
    }

    private void RefreshList_CryptoTokens()
    {
        _fldCryptoTokens.text = $"Crypto Tokens [ERC20] ({_cryptoTokens.Length})";
        _fldCryptoTokens.value = false;

        //Make sure other foldouts are collapsed on expand
        _fldCryptoTokens.RegisterValueChangedCallback((evtArgs) =>
        {
            if (evtArgs.newValue)
            {
                _fldMultiTokens.value = false;
                _fldWalletEvents.value = false;
            }
        });

        _lstCryptoTokens.ToggleDisplay(_cryptoTokens.Any());
        if (!_cryptoTokens.Any()) return;

        _lstCryptoTokens.makeItem = () => new Label();
        _lstCryptoTokens.bindItem = (element, i) =>
        {
            var lbl = element as Label;
            var token = _cryptoTokens[i];
            lbl.text = $"[{token.Symbol}] {token.Name}";
        };

        _lstCryptoTokens.itemsSource = _cryptoTokens;

        //selection
        _lstCryptoTokens.onItemsChosen += (itemList) =>
        {
            OnClick_CryptoToken(itemList.First() as VyCryptoToken);
        };
    }

    private void RefreshList_MultiTokens()
    {
        //Multi Tokens
        _fldMultiTokens.text = $"Multi Tokens [ERC1155/721] ({_multiTokens.Length})";
        _fldMultiTokens.value = false;

        //Make sure other foldouts are collapsed on expand
        _fldMultiTokens.RegisterValueChangedCallback((evtArgs) =>
        {
            if (evtArgs.newValue)
            {
                _fldCryptoTokens.value = false;
                _fldWalletEvents.value = false;
            }
        });

        //NFT
        _fldMultiTokens_NFT.text = $"Non Fungible [NFT] ({_multiTokens_NFT.Length})";
        _fldMultiTokens_NFT.value = false;

        _fldMultiTokens_NFT.RegisterValueChangedCallback((evtArgs) =>
        {
            if (evtArgs.newValue) _fldMultiTokens_FT.value = false;
        });

        CreateMultiTokenListView(_lstMultiTokens_NFT, _multiTokens_NFT);

        //FT
        _fldMultiTokens_FT.text = $"Fungible [FT] ({_multiTokens_FT.Length})";
        _fldMultiTokens_FT.value = false;

        _fldMultiTokens_FT.RegisterValueChangedCallback((evtArgs) =>
        {
            if (evtArgs.newValue) _fldMultiTokens_NFT.value = false;
        });

        CreateMultiTokenListView(_lstMultiTokens_FT, _multiTokens_FT);
    }

    private void CreateMultiTokenListView(ListView lstView, VyMultiTokenDto[] tokens)
    {
        lstView.ToggleDisplay(tokens.Any());
        if (!tokens.Any()) return;

        lstView.makeItem = () => new Label();
        lstView.bindItem = (element, i) =>
        {
            var lbl = element as Label;
            var token = tokens[i];

            if (token.Fungible) //FT
            {
                lbl.text = $"{token.Name}";
            }
            else //NFT
            {
                lbl.text = $"{token.Name} (#{token.GetAttribute("mintNumber").As<int>()})";
            }
        };

        lstView.itemsSource = tokens;

        //selection
        lstView.onItemsChosen += (itemList) =>
        {
            OnClick_MultiToken(itemList.First() as VyMultiTokenDto);
        };
    }

    private void OnClick_MultiToken(VyMultiTokenDto token)
    {
        //Set Blackboard Data
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.Shared_MultiTokenDetails, "token", token);
        //Switch View
        ViewManager.SwitchView(eApiExplorerViewId.Shared_MultiTokenDetails);
    }

    private void OnClick_CryptoToken(VyCryptoToken token)
    {
        //Set Blackboard Data
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.Shared_CryptoTokenDetails, "token", token);
        //Switch View
        ViewManager.SwitchView(eApiExplorerViewId.Shared_CryptoTokenDetails);
    }
}
