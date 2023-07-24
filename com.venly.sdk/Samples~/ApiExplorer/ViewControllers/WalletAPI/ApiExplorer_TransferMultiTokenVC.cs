using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransferMultiTokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_WALLET = "wallet";
    public const string DATAKEY_TOKEN = "token";

    //DATA
    private VyWalletDto _sourceWallet;
    private VyMultiTokenDto _sourceToken;

    //UI
    [UIBind("btn-select-source-wallet")] private Button _btnSelectSourceWallet;
    [UIBind("btn-select-source-token")] private Button _btnSelectSourceToken;


    public ApiExplorer_TransferMultiTokenVC() :
        base(eApiExplorerViewId.WalletApi_TransferMultiToken)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

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
        ToggleElement("btn-select-source", _sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);

        _btnSelectSourceWallet.ToggleDisplay(_sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);

        //Check if Token is Set
        if (_sourceWallet != null)
        {
            TryGetBlackboardData(out _sourceToken, localKey: DATAKEY_TOKEN);
            _btnSelectSourceToken.ToggleDisplay(_sourceToken == null);
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
            SetLabel("lbl-type", "select wallet");
        }
        else
        {
            SetLabel("lbl-source-wallet", _sourceWallet.Id);
            UpdateSourceToken(_sourceToken);
        }

        //Hide Select Token button if no wallet is selected yet
        _btnSelectSourceToken.ToggleDisplay(_sourceWallet != null);
    }

    private void UpdateSourceToken(VyMultiTokenDto sourceToken)
    {
        _sourceToken = sourceToken;

        if (_sourceToken == null)
        {
            SetLabel("lbl-source-token", "select token");
            SetLabel("lbl-type", "select token");
        }
        else
        {
            var tokenName = _sourceToken.Name;
            if (_sourceToken.HasAttribute("mintNumber")) tokenName += $" (#{_sourceToken.GetAttribute("mintNumber")})";

            SetLabel("lbl-source-token", tokenName);
            SetLabel("lbl-type", $"{_sourceToken.Contract.Type} ({(_sourceToken.Fungible?"Fungible":"NFT")})");
        }

        ToggleElement("txt-amount", _sourceToken?.Fungible??true);
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();
    }
    #endregion

    private void OnClick_SelectSourceToken()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewMultiTokens, "sourceWallet", _sourceWallet);
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewMultiTokens, "Select Token")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var token = result.Data as VyMultiTokenDto;
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

        if(_sourceToken?.Fungible ?? true)
            if (!ValidateInput<int>("txt-amount")) return;

        //Execute
        var reqParams = new VyTransactionMultiTokenTransferRequest()
        {
            Chain = _sourceWallet?.Chain ?? eVyChain.NotSupported,
            WalletId = _sourceWallet?.Id,
            TokenAddress = _sourceToken.Contract.Address,
            TokenId = int.Parse(_sourceToken.Id),
            ToAddress = GetValue("txt-target-address"),
            Amount = _sourceToken.Fungible?GetValue<int>("txt-amount"):null
        };

        ViewManager.Loader.Show("Transferring...");
        VenlyAPI.Wallet.ExecuteMultiTokenTransfer(GetValue("txt-pincode"), reqParams)
            .OnSuccess(transferInfo =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXHASH, transferInfo.TransactionHash);
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXCHAIN, _sourceWallet.Chain);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
}
