using System;
using System.Globalization;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransferNativeTokenVC : SampleViewBase<eApiExplorerViewId>
{
    private VyWalletDto _sourceWallet;

    private TextField _txtPincode;
    private TextField _txtTargetAddress;
    private TextField _txtAmount;


    public ApiExplorer_TransferNativeTokenVC() : 
        base(eApiExplorerViewId.WalletApi_TransferNativeToken) { }

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _txtPincode, "txt-pincode");
        GetElement(out _txtTargetAddress, "txt-target-address");
        GetElement(out _txtAmount, "txt-amount");

        BindButton("btn-select-source", OnClick_SelectSource);
        BindButton("btn-select-target", OnClick_SelectTarget);
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        //Check if Source Is Set
        if (HasBlackboardData("sourceWallet"))
        {
            ToggleElement("btn-select-source", false);
            UpdateSourceWallet(GetBlackBoardData<VyWalletDto>("sourceWallet"));
            SetLabel("lbl-source-wallet", _sourceWallet.Id);
        }
        else
        {
            ToggleElement("btn-select-source", true);
            UpdateSourceWallet(null);
        }
    }

    private void UpdateSourceWallet(VyWalletDto sourceWallet)
    {
        _sourceWallet = sourceWallet;

        SetLabel("lbl-source-wallet", _sourceWallet == null?"select wallet":_sourceWallet.Id);
        SetLabel("lbl-token", _sourceWallet == null?"select wallet":_sourceWallet.Balance.Symbol);
        SetLabel("lbl-balance", _sourceWallet == null?"select wallet":_sourceWallet.Balance.Balance.ToString(CultureInfo.InvariantCulture));
    }

    protected override void OnDeactivate()
    {
       ClearBlackboardData("sourceWallet");

       _sourceWallet = null;
    }

    private void OnClick_SelectSource()
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
                    _txtTargetAddress.value = wallet.Address;
                }
            });
    }

    private bool Validate()
    {
        try
        {
            if (_sourceWallet == null) throw new ArgumentException("No wallet selected");
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

    private void OnClick_Transfer()
    {
        if (!Validate()) return;

        var reqParams = new VyTransactionNativeTokenTransferRequest()
        {
            Chain = _sourceWallet?.Chain ?? eVyChain.NotSupported,
            WalletId = _sourceWallet?.Id,
            ToAddress = _txtTargetAddress.value,
            Value = double.Parse(_txtAmount.value),
            Data = "Venly Api Explorer Transaction (Native Token)"

        };

        ViewManager.Loader.Show("Transferring...");
        VenlyAPI.Wallet.ExecuteNativeTokenTransfer(_txtPincode.text, reqParams)
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
