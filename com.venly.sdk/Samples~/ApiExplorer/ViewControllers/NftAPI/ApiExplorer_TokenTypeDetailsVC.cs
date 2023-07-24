using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Nft;
using Venly.Models.Shared;
using Venly.Utils;

public class ApiExplorer_TokenTypeDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_CONTRACT = "contract";
    public const string DATAKEY_TOKENTYPE = "tokentype";

    //DATA
    private VyTokenTypeDto _tokenType;
    private VyContractDto _sourceContract;

    //UI
    [UIBind("img-tokentype")] private VisualElement _imgToken;
    [UIBind("fld-animation-urls")] private Foldout _fldAnimationUrls;
    [UIBind("lst-animation-urls")] private VyControl_TypeValueListView _lstAnimationUrls;
    [UIBind("fld-attributes")] private Foldout _fldAttributes;
    [UIBind("lst-attributes")] private VyControl_AttributeListView _lstAttributes;


    public ApiExplorer_TokenTypeDetailsVC() :
        base(eApiExplorerViewId.NftApi_TokenTypeDetails)
    {
    }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-mint", onClick_Mint);
    }

    protected override void OnActivate()
    {
        _fldAnimationUrls.value = false;
        _fldAttributes.value = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        TryGetBlackboardData(out _tokenType, localKey: DATAKEY_TOKENTYPE);
        TryGetBlackboardData(out _sourceContract, localKey: DATAKEY_CONTRACT);
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Refreshing TokenType...");
        var result = await VenlyAPI.Nft.GetTokenType(_sourceContract.Id, _tokenType.Id);
        ViewManager.Loader.Hide();

        if(result.Success) _tokenType = result.Data;
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_sourceContract, DATAKEY_CONTRACT)) return;
        if (!ValidateData(_tokenType, DATAKEY_TOKENTYPE)) return;

        //Set Data
        SetLabel("lbl-token-id", _tokenType.Id.ToString());
        SetLabel("lbl-token-name", _tokenType.Name);
        SetLabel("lbl-token-description", _tokenType.Description);
        SetLabel("lbl-token-supply", $"{_tokenType.CurrentSupply} / {_tokenType.MaxSupply}");
        SetLabel("lbl-token-chain", _sourceContract.Chain.GetMemberName());
        SetLabel("lbl-token-externalUrl", _tokenType.ExternalUrl);
        SetLabel("lbl-token-fungible", _tokenType.Fungible ? "YES" : "NO");
        SetLabel("lbl-token-burnable", _tokenType.Burnable ? "YES" : "NO");
        SetLabel("lbl-contract-name", $"{_sourceContract.Name} ({_sourceContract.Id})");

        _lstAnimationUrls.SetItemSource(_tokenType.AnimationUrls);
        _lstAnimationUrls.ToggleDisplay(_tokenType.AnimationUrls.Length > 0);

        _lstAttributes.SetItemSource(_tokenType.Attributes);
        _lstAttributes.ToggleDisplay(_tokenType.Attributes.Length > 0);

        //Manage FoldOuts
        _fldAttributes.value = false;
        _fldAttributes.text = $"Attributes ({_tokenType.Attributes.Length})";
        _fldAttributes.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAnimationUrls.value = false;
        });

        _fldAnimationUrls.value = false;
        _fldAnimationUrls.text = $"Animation Urls ({_tokenType.AnimationUrls.Length})";
        _fldAnimationUrls.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAttributes.value = false;
        });

        //Image
        VenlyUnityUtils.DownloadImage(_tokenType.ImageUrl)
            .OnComplete(result =>
            {
                ToggleElement("img-container", result.Success);
                if (result.Success)_imgToken.style.backgroundImage = new StyleBackground(result.Data);
                else VenlyLog.Exception(result.Exception);
            });
    }
    #endregion

    #region EVENTS
    private void onClick_Mint()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_MintToken, ApiExplorer_MintTokenVC.DATAKEY_CONTRACT, _sourceContract);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_MintToken, ApiExplorer_MintTokenVC.DATAKEY_TOKENTYPE, _tokenType);
        ViewManager.SwitchView(eApiExplorerViewId.NftApi_MintToken, ViewId, false);
    }
    #endregion
}