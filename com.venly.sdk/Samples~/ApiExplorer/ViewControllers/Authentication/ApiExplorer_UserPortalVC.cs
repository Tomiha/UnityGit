using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Market;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_UserPortalVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_USER_ID = "user-id";
    public const string DATAKEY_PINCODE = "pincode";

    //DATA
    private bool _hasWallet = false;
    private VyWalletDto _wallet;

    private bool _hasMarketUser = false;
    private VyUserProfileDto _marketUser;

    public ApiExplorer_UserPortalVC() :
        base(eApiExplorerViewId.Auth_UserPortal)
    { }

    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-wallet", OnClick_Wallet);
        BindButton("btn-market", OnClick_Market);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = false;
    }

    protected override async Task OnRefreshData()
    {
        try
        {
            //Check if Wallet is present
            ViewManager.Loader.Show("Checking for Wallet...");
            _hasWallet = await VenlyAPI.ProviderExtensions.HasWallet().AwaitResult();
            if (_hasWallet)
            {
                ViewManager.Loader.Show("Retrieving Wallet...");
                _wallet = await VenlyAPI.ProviderExtensions.GetWalletForUser().AwaitResult();
            }
            else
            {
                ViewManager.Loader.Show("Creating Wallet...");
                if (TryGetBlackboardData(out string pincode, localKey: DATAKEY_PINCODE))
                {
                    TryGetBlackboardDataRaw(out object providerUserId, localKey: DATAKEY_USER_ID);

                    //Create Wallet Params
                    var request = new VyCreateWalletRequest
                    {
                        Chain = eVyChain.Matic,
                        Description = $"Wallet created by Unity API Explorer. (provider={VenlySettings.BackendProvider.GetMemberName()}, id={providerUserId?.ToString()})",
                        Identifier = $"api-explorer-created ({VenlySettings.BackendProvider.GetMemberName()})",
                        Pincode = pincode,
                        WalletType = eVyWalletType.WhiteLabel
                    };

                    _wallet = await VenlyAPI.ProviderExtensions.CreateWalletForUser(request).AwaitResult();
                    _hasWallet = true;
                }
                else ViewManager.Loader.Show($"[UserPortalVC] DATAKEY \'{DATAKEY_PINCODE}\' is not set.");
            }

            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_UserWallet, _wallet);
            ViewManager.Loader.Show("Checking for Market User...");

            //Check if MarketUser is present
            _hasMarketUser = await VenlyAPI.ProviderExtensions.HasMarketUser().AwaitResult();
            if (_hasMarketUser)
            {
                ViewManager.Loader.Show("Retrieving Market User...");
                _marketUser = await VenlyAPI.ProviderExtensions.GetMarketUserForUser().AwaitResult();
                ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_UserMarketProfile, _marketUser);
            }
        }
        catch (Exception ex)
        {
            ViewManager.HandleException(ex);
        }
        finally
        {
            ViewManager.Loader.Hide();
        }
    }

    protected override void OnRefreshUI()
    {
        base.OnRefreshUI();

        SetLabel("btn-wallet", _hasWallet?"Show Wallet":"Create Wallet");
        SetLabel("btn-market", _hasMarketUser?"Show Market User":"Create Market User");
    }

    private void OnClick_Wallet()
    {
        if (_hasWallet)
        {
            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_UserWallet, _wallet);
            ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, this, false);
        }
    }

    private async void OnClick_Market()
    {
        try
        {
            if (!_hasMarketUser)
            {
                ViewManager.Loader.Show("Creating Market User...");
                TryGetBlackboardDataRaw(out object providerUserId, localKey: DATAKEY_USER_ID);
                if (providerUserId == null) providerUserId = "generic";

                var request = new VyCreateSubUserRequest
                {
                    Nickname = $"{VenlySettings.BackendProvider.GetMemberName()}_{providerUserId}",
                    Type = eVyUserType.SubUser
                };

                _marketUser = await VenlyAPI.ProviderExtensions.CreateMarketUserForUser(request).AwaitResult();
                _hasMarketUser = true;
                ViewManager.Loader.Hide();
            }
        }
        catch (Exception ex)
        {
            ViewManager.Loader.Hide();
            ViewManager.HandleException(ex);
            return;
        }

        ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_UserMarketProfile, _marketUser);
        ViewManager.SwitchView(eApiExplorerViewId.MarketApi_SubUserDetails, this, false);
    }
}
