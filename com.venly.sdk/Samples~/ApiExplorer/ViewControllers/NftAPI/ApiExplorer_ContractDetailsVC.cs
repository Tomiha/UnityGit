using System;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models.Nft;
using VenlySDK.Models.Shared;
using VenlySDK.Utils;

public class ApiExplorer_ContractDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    public const string DATAKEY_CONTRACT = "contract";

    private int? _lastLoadedContractId = null;
    private VyContractDto _contract;
    private VyTokenTypeDto[] _tokenTypes;
    private Texture2D _contractImage;

    public ApiExplorer_ContractDetailsVC() :
        base(eApiExplorerViewId.NftApi_ContractDetails) { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-tokenTypes", onClick_ShowTokenTypes);

        BindButton("btn-archive", onClick_Archive);

        //Only show Archive Button when DevMode is active
        ToggleElement("btn-archive", VenlySettings.BackendProvider == eVyBackendProvider.DevMode);
    }

    protected override void OnActivate()
    {
        _contract = null;

        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = true;

        if (!HasBlackboardData(DATAKEY_CONTRACT))
        {
            throw new Exception($"ContractDetailsVC >> BlackboardData \'{DATAKEY_CONTRACT}\' not found...");
        }

        _contract = GetBlackBoardData<VyContractDto>(DATAKEY_CONTRACT);

        RefreshContract();
    }

    protected override void OnClick_Refresh()
    {
        RefreshContract(true);
    }


    private
#if ENABLE_VENLY_DEVMODE
    async
#endif
    void onClick_Archive()
    {
#region DevMode Only (SERVER)
#if ENABLE_VENLY_DEVMODE
        ViewManager.Loader.Show("Archiving Contract...");
        var result = await Venly.NftAPI.Server.ArchiveContract(_contract.Id);
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            //Reset Global Contracts Cache & return to list
            ViewManager.ClearGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_AllContractsCached);
            ViewManager.SwitchView(eApiExplorerViewId.NftApi_ViewContracts);
        }
        else ViewManager.HandleException(result.Exception);
#endif
#endregion
    }

    private void onClick_ShowTokenTypes()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_ViewTokenTypes, "sourceContract", _contract);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_ViewTokenTypes, "tokenTypes", _tokenTypes);
        ViewManager.SwitchView(eApiExplorerViewId.NftApi_ViewTokenTypes);
    }

    private async void RefreshContract(bool forceFreshLoad = false)
    {
        if (forceFreshLoad || _contract.Id != _lastLoadedContractId)
        {
            //Retrieve Wallet Data
            //--------------------
            ViewManager.Loader.Show("Retrieving Wallet Info...");
            var result = await Venly.NftAPI.Client.GetContract(_contract.Id);
            ViewManager.Loader.Hide();

            if (!result.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _contract = result.Data;


            //Retrieve Token Types
            //----------------------------
            ViewManager.Loader.Show("Retrieving Token Types...");
            var tokenTypesResult = await Venly.NftAPI.Client.GetTokenTypes(_contract.Id);
            ViewManager.Loader.Hide();

            if (!tokenTypesResult.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _tokenTypes = tokenTypesResult.Data;

            //Retrieve Image
            //--------------
            ViewManager.Loader.Show("Retrieving Contract Image...");
            var imageResult = await VenlyUnityUtils.DownloadImage(_contract.Image);
            ViewManager.Loader.Hide();

            _contractImage = imageResult.Success ? imageResult.Data : null;
        }

        _lastLoadedContractId = _contract.Id;

        //Set Image
        ToggleElement("img-container", _contractImage!=null);
        if (_contractImage != null)
            GetElement<VisualElement>("img-contract").style.backgroundImage = new StyleBackground(_contractImage);

        //Refresh UI Elements
        SetLabel("lbl-contract-name", _contract.Name);
        SetLabel("lbl-contract-id", _contract.Id.ToString());
        SetLabel("lbl-contract-address", _contract.Address);
        SetLabel("lbl-contract-chain", _contract.Chain.GetMemberName());
        SetLabel("lbl-contract-symbol", _contract.Symbol);
        SetLabel("lbl-contract-description", _contract.Description);
        SetLabel("lbl-contract-externalUrl", _contract.ExternalUrl);
        SetLabel("lbl-contract-owner", _contract.Owner);


        //Token Data
        SetLabel("lbl-tokentype-amount", $"{_tokenTypes.Length} Type(s)");
    }
}
