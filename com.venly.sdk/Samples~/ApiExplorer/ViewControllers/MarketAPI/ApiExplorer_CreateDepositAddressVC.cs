using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Models.Shared;

public class ApiExplorer_CreateDepositAddressVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_USERPROFILE = "user-profile";

    //DATA
    private VyUserProfileDto _userProfile;
    private List<string> _availableChains;

    //UI
    [UIBind("selector-chain")] private DropdownField _selectorChain;

    public ApiExplorer_CreateDepositAddressVC() : 
        base(eApiExplorerViewId.MarketApi_CreateDepositAddress) {}

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", OnClick_CreateAddress);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        TryGetBlackboardData(out _userProfile, DATAKEY_USERPROFILE, ApiExplorer_GlobalKeys.DATA_UserMarketProfile);
        if(_userProfile == null)
        {
            ViewManager.Exception.Show($"CreateDepositAddressVC >> DATAKEY \'{DATAKEY_USERPROFILE}\' not set...");
        }

        //Set Values
        if (_availableChains == null)
        {
            ViewManager.Loader.Show("Checking available chains...");
            VenlyAPI.Market.GetSupportedChains()
                .OnSuccess(chains =>
                {
                    _availableChains = chains.Select(c => c.ToString()).ToList();
                    _selectorChain.choices = _availableChains;
                    _selectorChain.value = _selectorChain.choices[0];
                })
                .OnFail(ViewManager.Exception.Show)
                .Finally(ViewManager.Loader.Hide);
        }
        else
        {
            _selectorChain.choices = _availableChains;
            _selectorChain.value = _selectorChain.choices[0];
        }

    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();
        _userProfile = null;
    }
    #endregion

    #region EVENTS
    private void OnClick_CreateAddress()
    {
        //Validate Data
        if (!ValidateData(_userProfile, DATAKEY_USERPROFILE)) return;

        //Execute
        ViewManager.Loader.Show("Creating Deposit Address...");

        var request = new VyCreateDepositAddressRequest
        {
            Chain = _selectorChain.GetValue<eVyChain>()
        };

        VenlyAPI.Market.CreateDepositAddress(_userProfile.Id, request)
            .OnSuccess(FinishSelection)
            .OnFail(FailSelection)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
