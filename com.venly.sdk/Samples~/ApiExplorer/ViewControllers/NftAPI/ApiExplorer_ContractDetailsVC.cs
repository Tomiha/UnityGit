using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Nft;
using Venly.Models.Shared;
using Venly.Utils;

public class ApiExplorer_ContractDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_CONTRACT = "contract";

    //DATA
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


        if (!TryGetBlackboardData(out _contract, localKey:DATAKEY_CONTRACT))
        {
            ViewManager.Exception.Show($"ContractDetailsVC >> BlackboardData \'{DATAKEY_CONTRACT}\' not set...");
        }
    }

    protected override async Task OnRefreshData()
    {
        //Retrieve Wallet Data
        //--------------------
        ViewManager.Loader.Show("Retrieving Wallet Info...");
        var result = await VenlyAPI.Nft.GetContract(_contract.Id);
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
        var tokenTypesResult = await VenlyAPI.Nft.GetTokenTypes(_contract.Id);
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
        var imageResult = await VenlyUnityUtils.DownloadImage(_contract.ImageUrl);
        ViewManager.Loader.Hide();

        _contractImage = imageResult.Success ? imageResult.Data : null;
    }

    protected override void OnRefreshUI()
    {
        //Set Image
        ToggleElement("img-container", _contractImage != null);
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

    private
#if ENABLE_VENLY_DEV_MODE
    async
#endif
    void onClick_Archive()
    {
#region DevMode Only (SERVER)
#if ENABLE_VENLY_DEV_MODE
        ViewManager.Loader.Show("Archiving Contract...");
        var result = await VenlyAPI.Nft.ArchiveContract(_contract.Id);
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
}
