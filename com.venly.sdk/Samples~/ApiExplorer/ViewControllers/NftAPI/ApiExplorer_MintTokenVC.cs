using UnityEngine.UIElements;
using Venly;
using Venly.Models.Nft;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_MintTokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_CONTRACT = "contract";
    public const string DATAKEY_TOKENTYPE = "tokentype";

    //DATA
    private VyTokenTypeDto _sourceTokenType;
    private VyContractDto _sourceContract;

    //UI
    [UIBind("btn-select-contract")] private Button _btnSelectContract;
    [UIBind("btn-select-tokentype")] private Button _btnSelectTokenType;


    public ApiExplorer_MintTokenVC() :
        base(eApiExplorerViewId.NftApi_MintToken)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-select-contract", onClick_SelectContract);
        BindButton("btn-select-tokentype", onClick_SelectTokenType);
        BindButton("btn-select-target", onClick_SelectTarget);
        BindButton("btn-mint", onCick_Mint);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Check if Source Is Set
        TryGetBlackboardData(out _sourceContract, localKey: DATAKEY_CONTRACT);
        _btnSelectContract.ToggleDisplay(_sourceContract == null);
        UpdateSourceContract(_sourceContract);

        //Check if Token is Set
        if (_sourceContract != null)
        {
            TryGetBlackboardData(out _sourceTokenType, localKey: DATAKEY_TOKENTYPE);
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
        ClearBlackboardData();

        _sourceContract = null;
        _sourceTokenType = null;
    }
    #endregion

    #region EVENTS
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
        ViewManager.ClearViewBlackboardData(eApiExplorerViewId.NftApi_ViewTokenTypes);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_ViewTokenTypes, ApiExplorer_ViewTokenTypesVC.DATAKEY_CONTRACT, _sourceContract);
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
                    SetLabel("txt-target-address", wallet.Address);
                }
            });
    }

    private void onCick_Mint()
    {
        //Validate
        if (!ValidateData(_sourceContract, "contract")) return;
        if (!ValidateData(_sourceTokenType, "tokentype")) return;
        if (!ValidateInput("txt-target-address")) return;
        if (!ValidateInput<int>("txt-amount")) return;

        //Execute
        var reqParams = new VyMintTokensRequest()
        {
            Destinations = new []
            {
                new VyTokenDestinationDto()
                {
                    Address = GetValue("txt-target-address"),
                    Amount = GetValue<int>("txt-amount")
                }
            }
        };

        ViewManager.Loader.Show("Minting...");
        VenlyAPI.Nft.MintTokens(_sourceContract.Id, _sourceTokenType.Id, reqParams)
            .OnSuccess(mintInfo =>
            {
                //todo show confirmation
                ViewManager.Info.Show("Token successfully minted!");
                ViewManager.NavigateBack();
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
