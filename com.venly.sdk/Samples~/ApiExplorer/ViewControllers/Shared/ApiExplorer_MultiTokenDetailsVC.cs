using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_MultiTokenDetailsVC : SampleViewBase<eApiExplorerViewId>
{

    //DATA
    private VyMultiTokenDto _token;

    //UI
    [UIBind("img-token")] private VisualElement _imgToken;

    [UIBind("fld-animation-urls")] private Foldout _fldAnimationUrls;
    [UIBind("lst-animation-urls")] private VyControl_TypeValueListView _lstAnimationUrls;
    [UIBind("fld-attributes")] private Foldout _fldAttributes;
    [UIBind("lst-attributes")] private VyControl_AttributeListView _lstAttributes;

    public ApiExplorer_MultiTokenDetailsVC() : 
        base(eApiExplorerViewId.Shared_MultiTokenDetails)
    {
    }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        //Make sure UIBind Elements are configured
        base.OnBindElements(root);

        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        _fldAnimationUrls.value = false;
        _fldAttributes.value = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        TryGetBlackboardData(out _token, localKey: ApiExplorer_TokenDetailsDataKeys.DATAKEY_TOKEN);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_token, "token")) return;

        SetLabel("lbl-token-id", _token.Id);
        SetLabel("lbl-token-name", _token.Name);
        SetLabel("lbl-token-description", _token.Description);
        SetLabel("lbl-token-url", _token.Url);
        SetLabel("lbl-token-fungible", _token.Fungible ? "YES" : "NO");
        SetLabel("lbl-token-transfer-fees", _token.TransferFees ? "YES" : "NO");
        SetLabel("lbl-contract-name", _token.Contract.Name);
        SetLabel("lbl-contract-type", _token.Contract.Type);

        BuildAnimationUrls();
        BuildAttributes();

        VenlyUnityUtils.DownloadImage(_token.ImageUrl)
            .OnSuccess(tex =>
            {
                _imgToken.style.backgroundImage = new StyleBackground(tex);
            })
            .OnFail(ex => VenlyLog.Exception(ex));
    }

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
        _lstAnimationUrls.SetItemSource(_token.AnimationUrls);
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
        _lstAttributes.SetItemSource(_token.Attributes);
    }
    #endregion

    #region EVENTS
    private void OnClick_Transfer()
    {
        var wallet = GetBlackBoardData<VyWalletDto>(ApiExplorer_TokenDetailsDataKeys.DATAKEY_WALLET);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferMultiToken, ApiExplorer_TransferMultiTokenVC.DATAKEY_WALLET, wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferMultiToken, ApiExplorer_TransferMultiTokenVC.DATAKEY_TOKEN, _token);

        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferMultiToken);
    }
    #endregion
}