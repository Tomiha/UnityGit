using System;
using UnityEngine.UIElements;
using Venly;
using Venly.Utils;

#if ENABLE_VENLY_PLAYFAB
using Venly.Backends.PlayFab;
#elif ENABLE_VENLY_BEAMABLE
using Beamable;
using Beamable.Player;
using Venly.Backends.Beamable;
#endif

public class ApiExplorer_CreateUserVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_CreateUserVC() : 
        base(eApiExplorerViewId.Auth_Create) { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-create", onClick_CreateUser);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowNavigateHome = false;
        ShowRefresh = false;

        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
    }

    private void onClick_CreateUser()
    {
        if (!ValidateInput("txt-email")) return;
        if (!ValidateInput("txt-password")) return;
        if (!ValidateInput("txt-pincode")) return;


#if ENABLE_VENLY_BEAMABLE
        Beamable_CreateUser();
#elif ENABLE_VENLY_PLAYFAB
        PlayFab_CreateUser();
#else
    ViewManager.Exception.Show($"No Implementation for selected Backend Provider ({VenlySettings.BackendProvider.GetMemberName()})");
#endif
    }


#if ENABLE_VENLY_BEAMABLE
    private async void Beamable_CreateUser()
    {
        try
        {
            ViewManager.Loader.Show("Creating User...");

            //Retrieve BeamContext
            var ctx = BeamContext.Default;
            await ctx.OnReady;

            //Provide BeamContext to Beamable Provider
            VenlyAPI.SetProviderData(VyProvider_Beamable.BeamContextDataKey, ctx);

            //Check if email is available
            var isAvailable = await ctx.Accounts.IsEmailAvailable(GetValue("txt-email"));
            if (!isAvailable) throw new Exception("Email is already used.");

            //Login with Credentials
            var newAccount = await ctx.Accounts.CreateNewAccount();

            var operation = await newAccount.AddEmail(GetValue("txt-email"), GetValue("txt-password"));
            if (!operation.isSuccess)
            {
                switch (operation.error)
                {
                    case PlayerRegistrationError.ALREADY_HAS_CREDENTIAL:
                        throw new Exception("Account already registered");
                    case PlayerRegistrationError.CREDENTIAL_IS_ALREADY_TAKEN:
                        throw new Exception("Credentials already used");
                    default:
                        throw operation.innerException;
                }
            }

            await newAccount.SwitchToAccount();
            ViewManager.Loader.Hide();

            SwitchToPortal(ctx.PlayerId.ToString());
        }
        catch (Exception ex)
        {
            ViewManager.Loader.Hide();
            ViewManager.HandleException(ex);
        }
    }
#endif

#if ENABLE_VENLY_PLAYFAB
    private async void PlayFab_CreateUser()
    {
        ViewManager.Loader.Show("Logging in...");
        var result = await PlayFabAuth.SignUp(GetValue("txt-email"), GetValue("txt-password"));
        ViewManager.Loader.Hide();

        if (!result.Success)
        {
            ViewManager.HandleException(result.Exception);
            return;
        }

        VenlyAPI.SetProviderData(VyProvider_PlayFab.AuthContextDataKey, result.Data.AuthenticationContext);
        SwitchToPortal(result.Data.PlayFabId);
    }
#endif

    private void SwitchToPortal(string userId)
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.Auth_UserPortal, ApiExplorer_UserPortalVC.DATAKEY_PINCODE, GetValue("txt-pincode"));
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.Auth_UserPortal, ApiExplorer_UserPortalVC.DATAKEY_USER_ID, userId);
        ViewManager.SwitchView(eApiExplorerViewId.Auth_UserPortal);
    }
}