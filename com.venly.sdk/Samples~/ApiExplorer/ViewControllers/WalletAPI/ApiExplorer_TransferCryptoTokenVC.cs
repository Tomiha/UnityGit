using System.Globalization;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransferCryptoTokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_WALLET = "wallet";
    public const string DATAKEY_TOKEN = "token";

    //DATA
    private VyWalletDto _sourceWallet;
    private VyCryptoTokenDto _sourceToken;

    public ApiExplorer_TransferCryptoTokenVC() : 
        base(eApiExplorerViewId.WalletApi_TransferCryptoToken) { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-select-source-wallet", OnClick_SelectSourceWallet);
        BindButton("btn-select-source-token", OnClick_SelectSourceToken);
        BindButton("btn-select-target", OnClick_SelectTarget);
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        _sourceWallet = null;
        _sourceToken = null;

        //Check if Source Is Set
        TryGetBlackboardData(out _sourceWallet, DATAKEY_WALLET, ApiExplorer_GlobalKeys.DATA_UserWallet);
        ToggleElement("btn-select-source-wallet", _sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);

        //Check if Token is Set
        if (_sourceWallet != null)
        {
            TryGetBlackboardData(out _sourceToken, localKey: DATAKEY_TOKEN);
            ToggleElement("btn-select-source-token", _sourceToken == null);
            UpdateSourceToken(_sourceToken);
        }
    }

    private void UpdateSourceWallet(VyWalletDto sourceWallet)
    {
        if (_sourceWallet != sourceWallet) //new wallet, reset token
        {
            _sourceToken = null;
        }

        _sourceWallet = sourceWallet;

        if (_sourceWallet == null)
        {
            SetLabel("lbl-source-wallet", "select wallet");
            SetLabel("lbl-source-token", "select wallet");
            SetLabel("lbl-balance", "select wallet");
        }
        else
        {
            SetLabel("lbl-source-wallet", _sourceWallet.Id);
            UpdateSourceToken(_sourceToken);
        }

        //Hide Select Token button if no wallet is selected yet
        ToggleElement("btn-select-source-token", _sourceWallet != null);
    }

    private void UpdateSourceToken(VyCryptoTokenDto sourceToken)
    {
        _sourceToken = sourceToken;

        if (_sourceToken == null)
        {
            SetLabel("lbl-source-token", "select token");
            SetLabel("lbl-balance", "select token");
        }
        else
        {
            SetLabel("lbl-source-token", _sourceToken.Symbol);
            SetLabel("lbl-balance", _sourceToken.Balance.ToString(CultureInfo.InvariantCulture));
        }
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();
    }
    #endregion

    #region EVENTS
    private void OnClick_SelectSourceToken()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewCryptoTokens, "sourceWallet", _sourceWallet);
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewCryptoTokens, "Select Token")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var token = result.Data as VyCryptoTokenDto;
                    UpdateSourceToken(token);
                }
            });
    }

    private void OnClick_SelectSourceWallet()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    UpdateSourceWallet(wallet);
                }
            });
    }

    private void OnClick_SelectTarget()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    SetLabel("txt-target-address", wallet.Address);
                }
            });
    }


    private void OnClick_Transfer()
    {
        //Validate
        if (!ValidateData(_sourceWallet, "sourceWallet")) return;
        if (!ValidateData(_sourceToken, "sourceToken")) return;
        if (!ValidateInput("txt-pincode")) return;
        if (!ValidateInput("txt-target-address")) return;
        if (!ValidateInput<double>("txt-amount")) return;

        //Execute
        var reqParams = new VyTransactionCrytoTokenTransferRequest()
        {
            Chain = _sourceWallet?.Chain ?? eVyChain.NotSupported,
            WalletId = _sourceWallet?.Id,
            TokenAddress = _sourceToken.TokenAddress,
            ToAddress = GetValue("txt-target-address"),
            Value = GetValue<double>("txt-amount")
        };

        ViewManager.Loader.Show("Transferring...");
        VenlyAPI.Wallet.ExecuteCryptoTokenTransfer(GetValue("txt-pincode"), reqParams)
            .OnSuccess(transferInfo =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXHASH, transferInfo.TransactionHash);
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXCHAIN, _sourceWallet.Chain);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
