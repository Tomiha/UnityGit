using System;
using System.Globalization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;

public class ApiExplorer_TransferCryptoTokenVC : SampleViewBase<eApiExplorerViewId>
{
    private VyWalletDto _sourceWallet;
    private VyCryptoTokenDto _sourceToken;

    private TextField _txtPincode;
    private TextField _txtTargetAddress;
    private TextField _txtAmount;

    private Button _btnSelectSourceWallet;
    private Button _btnSelectSourceToken;


    public ApiExplorer_TransferCryptoTokenVC() : 
        base(eApiExplorerViewId.WalletApi_TransferCryptoToken) { }

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _txtPincode, "txt-pincode");
        GetElement(out _txtTargetAddress, "txt-target-address");
        GetElement(out _txtAmount, "txt-amount");
        GetElement(out _btnSelectSourceWallet, "btn-select-source-wallet");
        GetElement(out _btnSelectSourceToken, "btn-select-source-token");

        BindButton("btn-select-source-wallet", onClick_SelectSourceWallet);
        BindButton("btn-select-source-token", onClick_SelectSourceToken);
        BindButton("btn-select-target", onClick_SelectTarget);
        BindButton("btn-transfer", onClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        //Check if Source Is Set
        if (HasBlackboardData("sourceWallet")) _sourceWallet = GetBlackBoardData<VyWalletDto>("sourceWallet");
        else _sourceWallet = null;

        _btnSelectSourceWallet.ToggleDisplay(_sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);

        //Check if Token is Set
        if (_sourceWallet != null)
        {
            if (HasBlackboardData("sourceToken")) _sourceToken = GetBlackBoardData<VyCryptoTokenDto>("sourceToken");
            else _sourceToken = null;

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
            SetLabel("lbl-balance", "select wallet");
        }
        else
        {
            SetLabel("lbl-source-wallet", _sourceWallet.Id);
            UpdateSourceToken(_sourceToken);
        }

        //Hide Select Token button if no wallet is selected yet
        _btnSelectSourceToken.ToggleDisplay(_sourceWallet != null);
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
       ClearBlackboardData("sourceWallet");
       ClearBlackboardData("sourceToken");

       _sourceWallet = null;
       _sourceToken = null;
    }

    private void onClick_SelectSourceToken()
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

    private void onClick_SelectSourceWallet()
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

    private void onClick_SelectTarget()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    _txtTargetAddress.value = wallet.Address;
                }
            });
    }

    private bool Validate()
    {
        try
        {
            if (_sourceWallet == null) throw new ArgumentException("No wallet selected");
            if (_sourceToken == null) throw new ArgumentException("No token selected");
            if (string.IsNullOrEmpty(_txtPincode.text)) throw new ArgumentException("Pincode invalid");
            if (string.IsNullOrEmpty(_txtTargetAddress.text)) throw new ArgumentException("Target address invalid");
            if (string.IsNullOrEmpty(_txtAmount.text)) throw new ArgumentException("Amount invalid");
        }
        catch (Exception ex)
        {
            ViewManager.HandleException(ex);
            return false;
        }

        return true;
    }

    private void onClick_Transfer()
    {
        if (!Validate()) return;

        var reqParams = new VyExecuteCryptoTokenTransferDto()
        {
            Pincode = _txtPincode.text,
            Request = new VyTransferCryptoRequest()
            {
                Chain = _sourceWallet?.Chain??eVyChain.NotSupported,
                WalletId = _sourceWallet?.Id,
                TokenAddress = _sourceToken.TokenAddress,
                ToAddress = _txtTargetAddress.value,
                Value = double.Parse(_txtAmount.value)
            }
        };

        ViewManager.Loader.Show("Transferring...");
        Venly.WalletAPI.Client.ExecuteCryptoTokenTransfer(reqParams)
            .OnSuccess(transferInfo =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_hash", transferInfo.TransactionHash);
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_chain", _sourceWallet.Chain);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails);
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
}
