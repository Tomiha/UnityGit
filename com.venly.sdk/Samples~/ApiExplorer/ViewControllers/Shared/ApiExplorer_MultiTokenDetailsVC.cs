using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK.Models;
using VenlySDK.Models.Shared;
using VenlySDK.Models.Wallet;

public class ApiExplorer_MultiTokenDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //UI Elements
    [UIBind("img-token")] private VisualElement _imgToken;

    [UIBind("fld-animation-urls")] private Foldout _fldAnimationUrls;
    [UIBind("lst-animation-urls")] private ListView _lstAnimationUrls;
    [UIBind("fld-attributes")] private Foldout _fldAttributes;
    [UIBind("lst-attributes")] private ListView _lstAttributes;

    private VyMultiTokenDto _token;

    private string _lastTokenId = null;
    private Texture2D _tokenTexture = null;

    public ApiExplorer_MultiTokenDetailsVC() : 
        base(eApiExplorerViewId.Shared_MultiTokenDetails)
    {
    }

    protected override void OnBindElements(VisualElement root)
    {
        //Make sure UIBind Elements are configured
        base.OnBindElements(root);

        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;

        _fldAnimationUrls.value = false;
        _fldAttributes.value = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        _token = GetBlackBoardData<VyMultiTokenDto>("token");

        //Retrieve Image
        if (_tokenTexture == null || _lastTokenId != _token.Id)
        {
            VenlyUnityUtils.DownloadImage(_token.ImageUrl)
                .OnComplete(result =>
                {
                    _imgToken.ToggleDisplay(result.Success);
                    if (result.Success)
                    {
                        _tokenTexture = result.Data;
                        _imgToken.style.backgroundImage = new StyleBackground(_tokenTexture);
                    }
                    else ViewManager.HandleException(result.Exception);
                });
        }

        //Set Data
        SetLabel("lbl-token-id", _token.Id);
        SetLabel("lbl-token-name", _token.Name);
        SetLabel("lbl-token-description", _token.Description);
        SetLabel("lbl-token-url", _token.Url);
        SetLabel("lbl-token-fungible", _token.Fungible?"YES":"NO");
        SetLabel("lbl-token-transfer-fees", _token.TransferFees?"YES":"NO");
        SetLabel("lbl-contract-name", _token.Contract.Name);
        SetLabel("lbl-contract-type", _token.Contract.Type);

        BuildAnimationUrls();
        BuildAttributes();
    }

    protected override void OnDeactivate() {}

    private void BuildAnimationUrls()
    {
        _fldAttributes.ToggleDisplay(_token.AnimationUrls != null);
        if (_token.AnimationUrls == null) return;

        _fldAnimationUrls.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAttributes.value = false;
        });

        _fldAnimationUrls.text = $"Animation Urls ({_token.AnimationUrls.Length})";

        _lstAnimationUrls.ToggleDisplay(_token.AnimationUrls.Any());
        if (!_token.AnimationUrls.Any()) return;

        _lstAnimationUrls.makeItem = () =>
        {
            return new Label();
        };

        _lstAnimationUrls.bindItem = (element, i) =>
        {
            var item = _token.AnimationUrls[i];
            (element as Label).text = $"Type = {item.Type}\nValue = {item.Value}\n";
        };

        _lstAnimationUrls.itemsSource = _token.AnimationUrls;
    }

    private void BuildAttributes()
    {
        _fldAttributes.ToggleDisplay(_token.Attributes != null);
        if (_token.Attributes == null) return;

        _fldAttributes.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAnimationUrls.value = false;
        });

        _fldAttributes.text = $"Attributes ({_token.Attributes.Length})";

        _lstAnimationUrls.ToggleDisplay(_token.Attributes.Any());
        if (!_token.Attributes.Any()) return;

        _lstAttributes.makeItem = () =>
        {
            return new Label();
        };

        _lstAttributes.bindItem = (element, i) =>
        {
            var item = _token.Attributes[i];
            (element as Label).text = $"Type = {item.Type}\nName = {item.Name}\nValue = {item.Value}\n";
        };

        _lstAttributes.itemsSource = _token.Attributes;
    }

    private void OnClick_Transfer()
    {
        var transferView = eApiExplorerViewId.WalletApi_TransferMultiToken;
        var wallet = GetBlackBoardData<VyWalletDto>("sourceWallet");
        ViewManager.SetViewBlackboardData(transferView, "sourceWallet", wallet);
        ViewManager.SetViewBlackboardData(transferView, "sourceToken", _token);

        ViewManager.SwitchView(transferView);
    }
}