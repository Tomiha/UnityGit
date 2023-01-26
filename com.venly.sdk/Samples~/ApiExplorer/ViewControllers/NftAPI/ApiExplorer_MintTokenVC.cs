using System;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;

public class ApiExplorer_MintTokenVC : SampleViewBase<eApiExplorerViewId>
{
    private VyTokenTypeDto _sourceTokenType;
    private VyContractDto _sourceContract;

    private TextField _txtTargetAddress;
    private TextField _txtAmount;

    private Button _btnSelectContract;
    private Button _btnSelectTokenType;


    public ApiExplorer_MintTokenVC() :
        base(eApiExplorerViewId.NftApi_MintToken)
    { }

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _txtTargetAddress, "txt-target-address");
        GetElement(out _txtAmount, "txt-amount");
        GetElement(out _btnSelectContract, "btn-select-contract");
        GetElement(out _btnSelectTokenType, "btn-select-tokentype");

        BindButton("btn-select-contract", onClick_SelectContract);
        BindButton("btn-select-tokentype", onClick_SelectTokenType);
        BindButton("btn-select-target", onClick_SelectTarget);
        BindButton("btn-mint", onCick_Mint);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        //Check if Source Is Set
        if (HasBlackboardData("sourceContract")) _sourceContract = GetBlackBoardData<VyContractDto>("sourceContract");
        else _sourceContract = null;

        _btnSelectContract.ToggleDisplay(_sourceContract == null);
        UpdateSourceContract(_sourceContract);

        //Check if Token is Set
        if (_sourceContract != null)
        {
            if (HasBlackboardData("sourceTokenType")) _sourceTokenType = GetBlackBoardData<VyTokenTypeDto>("sourceTokenType");
            else _sourceTokenType = null;

            _btnSelectTokenType.ToggleDisplay(_sourceTokenType == null);
            UpdateSourceTokenType(_sourceTokenType);
        }
    }

    private void UpdateSourceContract(VyContractDto sourceContract)
    {
        if (_sourceContract != sourceContract) //new contract, reset token
        {
            _sourceTokenType = null;
        }

        _sourceContract = sourceContract;

        if (_sourceContract == null)
        {
            SetLabel("lbl-source-contract", "select contract");
            SetLabel("lbl-source-tokentype", "select contract");
            SetLabel("lbl-fungible", "select contract");
            SetLabel("lbl-supply", "select contract");
        }
        else
        {
            SetLabel("lbl-source-contract", $"{_sourceContract.Name} ({_sourceContract.Id})");
            UpdateSourceTokenType(_sourceTokenType);
        }

        //Hide Select Token button if no wallet is selected yet
        _btnSelectTokenType.ToggleDisplay(_sourceContract != null);
    }

    private void UpdateSourceTokenType(VyTokenTypeDto sourceTokenType)
    {
        _sourceTokenType = sourceTokenType;

        if (_sourceTokenType == null)
        {
            SetLabel("lbl-source-tokentype", "select type");
            SetLabel("lbl-fungible", "select type");
            SetLabel("lbl-supply", "select type");
        }
        else
        {
            SetLabel("lbl-source-tokentype", $"{_sourceTokenType.Name} ({_sourceTokenType.Id})");
            SetLabel("lbl-fungible", _sourceTokenType.Fungible?"YES":"NO");
            SetLabel("lbl-supply", $"{_sourceTokenType.CurrentSupply}/{_sourceTokenType.MaxSupply}");
        }
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData("sourceContract");
        ClearBlackboardData("sourceTokenType");

        _sourceContract = null;
        _sourceTokenType = null;
    }

    private void onClick_SelectContract()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.NftApi_ViewContracts, "Select Contract")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var contract = result.Data as VyContractDto;
                    UpdateSourceContract(contract);
                }
            });
    }

    private void onClick_SelectTokenType()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_ViewTokenTypes, "sourceContract", _sourceContract);
        ViewManager.SelectionMode(eApiExplorerViewId.NftApi_ViewTokenTypes, "Select TokenType")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var token = result.Data as VyTokenTypeDto;
                    UpdateSourceTokenType(token);
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
            if (_sourceContract == null) throw new ArgumentException("No contract selected");
            if (_sourceTokenType == null) throw new ArgumentException("No token-type selected");
            if (string.IsNullOrEmpty(_txtTargetAddress.text)) throw new ArgumentException("Target address invalid");
            if (string.IsNullOrEmpty(_txtAmount.text) || !int.TryParse(_txtAmount.value, out _)) throw new ArgumentException("Amount invalid");
        }
        catch (Exception ex)
        {
            ViewManager.HandleException(ex);
            return false;
        }

        return true;
    }

    private void onCick_Mint()
    {
#if ENABLE_VENLY_DEVMODE
        if (!Validate()) return;

        var reqParams = new VyMintTokenDto()
        {
            ContractId = _sourceContract.Id,
            TokenId = _sourceTokenType.Id,
            Destinations = new []
            {
                new VyMintDestinationDto
                {
                    Address = _txtTargetAddress.value,
                    Amount = int.Parse(_txtAmount.value)
                }
            }
        };

        ViewManager.Loader.Show("Minting...");
        Venly.NftAPI.Server.MintToken(reqParams)
            .OnSuccess(mintInfo =>
            {
                //todo show confirmation
                //ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_hash", transferInfo.TransactionHash);
                //ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_chain", _sourceWallet.Chain);
                //ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails);
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
#else
        ViewManager.HandleException(new Exception("Minting only possible on a backend."));
#endif
    }

    //private void MintNFT()
    //{
    //    var reqParams = new VyMintNonFungibleTokenDto()
    //    {
    //        ContractId = _sourceContract.Id,
    //        TokenId = _sourceTokenType.Id,
    //        Amounts = new[] { int.Parse(_txtAmount.value) },
    //        Destinations = new[] { _txtTargetAddress.value }
    //    };

    //    ViewManager.Loader.Show("Minting...");
    //    Venly.NftAPI.Server.MintTokenNFT(reqParams)
    //        .OnSuccess(mintInfo =>
    //        {
    //            //todo show confirmation
    //            //ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_hash", transferInfo.TransactionHash);
    //            //ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_chain", _sourceWallet.Chain);
    //            //ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails);
    //        })
    //        .OnFail(ViewManager.HandleException)
    //        .Finally(ViewManager.Loader.Hide);
    //}

    //private void MintFT()
    //{
    //    var reqParams = new VyMintFungibleTokenDto()
    //    {
    //        ContractId = _sourceContract.Id,
    //        TokenId = _sourceTokenType.Id,
    //        Amounts = new []{int.Parse(_txtAmount.value)},
    //        Destinations = new []{_txtTargetAddress.value}
    //    };

    //    ViewManager.Loader.Show("Minting...");
    //    Venly.NftAPI.Server.MintTokenFT(reqParams)
    //        .OnSuccess(mintInfo =>
    //        {
    //            //todo show confirmation
    //            //ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_hash", transferInfo.TransactionHash);
    //            //ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, "tx_chain", _sourceWallet.Chain);
    //            //ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails);
    //        })
    //        .OnFail(ViewManager.HandleException)
    //        .Finally(ViewManager.Loader.Hide);
    //}
}
