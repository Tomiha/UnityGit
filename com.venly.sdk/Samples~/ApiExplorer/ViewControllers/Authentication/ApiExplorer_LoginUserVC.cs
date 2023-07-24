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

public class ApiExplorer_LoginUserVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_LoginUserVC() :
        base(eApiExplorerViewId.Auth_Login)
    { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-login", onClick_LoginUser);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowNavigateHome = false;
        ShowRefresh = false;

        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
    }

    private void onClick_LoginUser()
    {
        if (!ValidateInput("txt-email")) return;
        if (!ValidateInput("txt-password")) return;

#if ENABLE_VENLY_BEAMABLE
        Beamable_LoginUser();
#elif ENABLE_VENLY_PLAYFAB
        PlayFab_LoginUser();
#else
    ViewManager.Exception.Show($"No Implementation for selected Backend Provider ({VenlySettings.BackendProvider.GetMemberName()})");
#endif
    }

#if ENABLE_VENLY_BEAMABLE
    private async void Beamable_LoginUser()
    {
        try
        {
            ViewManager.Loader.Show("Logging in...");

            //Retrieve BeamContext
            var ctx = BeamContext.Default;
            await ctx.OnReady;

            //Provide BeamContext to Beamable Provider
            VenlyAPI.SetProviderData(VyProvider_Beamable.BeamContextDataKey, ctx);

            //Login with Credentials
            var operation = await ctx.Accounts.RecoverAccountWithEmail(GetValue("txt-email"), GetValue("txt-password"));
            if (operation.isSuccess)
            {
                await operation.SwitchToAccount();
            }
            else if (operation.error == PlayerRecoveryError.UNKNOWN_CREDENTIALS)
            {
                throw new Exception("Unknown Credentials");
            }
            else
            {
                var ex = operation.GetException();
                throw new Exception($"Failed to recover the account. (inner-error={ex.Message})");
            }

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
    private async void PlayFab_LoginUser()
    {
        ViewManager.Loader.Show("Logging in...");
        var result = await PlayFabAuth.SignIn(GetValue("txt-email"), GetValue("txt-password"));
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
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.Auth_UserPortal, ApiExplorer_UserPortalVC.DATAKEY_USER_ID, userId);
        ViewManager.SwitchView(eApiExplorerViewId.Auth_UserPortal);
    }
}
