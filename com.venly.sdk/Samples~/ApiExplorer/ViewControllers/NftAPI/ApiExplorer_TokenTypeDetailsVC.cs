using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;
using VenlySDK.Models.Nft;
using VenlySDK.Utils;

public class ApiExplorer_TokenTypeDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //UI Elements
    private VisualElement _imgToken;

    private Foldout _fldAnimationUrls;
    private VyControl_TypeValueListView _lstAnimationUrls;
    private Foldout _fldAttributes;
    private VyControl_AttributeListView _lstAttributes;

    private VyTokenTypeDto _tokenType;
    private VyContractDto _sourceContract;

    private long? _lastTokenId = null;
    private Texture2D _tokenTexture = null;

    public ApiExplorer_TokenTypeDetailsVC() :
        base(eApiExplorerViewId.NftApi_TokenTypeDetails)
    {
    }

    protected override void OnBindElements(VisualElement root)
    {
        GetElement(out _fldAnimationUrls, "fld-animation-urls");
        GetElement(out _lstAnimationUrls, "lst-animation-urls");
        GetElement(out _fldAttributes, "fld-attributes");
        GetElement(out _lstAttributes, "lst-attributes");
        GetElement(out _imgToken, "img-tokentype");

        BindButton("btn-mint", onClick_Mint);
    }

    protected override void OnActivate()
    {
        ShowNavigateHome = true;
        ShowNavigateBack = true;
        ShowRefresh = true;

        _fldAnimationUrls.value = false;
        _fldAttributes.value = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        _tokenType = GetBlackBoardData<VyTokenTypeDto>("tokenType");
        _sourceContract = GetBlackBoardData<VyContractDto>("sourceContract");

        if (_tokenType == null || _sourceContract == null)
        {
            ViewManager.HandleException(new ArgumentException("TokenTypeDetails View > tokenType or sourceContract data not set."));
            return;
        }

        Refresh();
    }

    protected override void OnClick_Refresh()
    {
        Refresh(true);
    }

    private void Refresh(bool force = false)
    {
        if (force)
        {
            ViewManager.Loader.Show("Refreshing TokenType...");
            Venly.NftAPI.Client.GetTokenType(_sourceContract.Id, _tokenType.Id)
                .OnSuccess(tokenType =>
                {
                    _tokenType = tokenType;
                    _lastTokenId = null;

                    RefreshView();
                })
                .OnFail(ViewManager.HandleException)
                .Finally(ViewManager.Loader.Hide);

        }
        else
        {
            RefreshView();
        }
    }

    private void RefreshView()
    {
        //Retrieve Image
        if (_tokenTexture == null || _lastTokenId != _tokenType.Id)
        {
            VenlyUnityUtils.DownloadImage(_tokenType.Image)
                .OnComplete(result =>
                {
                    ToggleElement("img-container", result.Success);
                    if (result.Success)
                    {
                        _tokenTexture = result.Data;
                        _imgToken.style.backgroundImage = new StyleBackground(_tokenTexture);
                    }
                    else ViewManager.HandleException(result.Exception);
                });
        }

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
    }

    private void onClick_Mint()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_MintToken, "sourceContract", _sourceContract);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.NftApi_MintToken, "sourceTokenType", _tokenType);
        ViewManager.SwitchView(eApiExplorerViewId.NftApi_MintToken);
    }
}