using UnityEngine.UIElements;
using Venly;
using Venly.Core;

#if ENABLE_VENLY_PLAYFAB
using Venly.Backends.PlayFab;
#endif

public class ApiExplorer_LoginUserVC : SampleViewBase<eApiExplorerViewId>
{
    private TextField _txtEmail;
    private TextField _txtPassword;

    public ApiExplorer_LoginUserVC() :
        base(eApiExplorerViewId.Auth_Login)
    { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-login", onClick_LoginUser);

        GetElement(out _txtEmail, "txt-email");
        GetElement(out _txtPassword, "txt-password");
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowNavigateHome = false;
        ShowRefresh = false;

        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
    }

    protected override void OnDeactivate()
    {

    }

#if ENABLE_VENLY_PLAYFAB
    private void onClick_LoginUser()
    {
        ViewManager.Loader.Show("Logging in...");

        //Helper Task
        var taskNotifier = VyTask.Create();
        var combinedTask = taskNotifier.Task;

        PlayFabAuth.SignIn(_txtEmail.text, _txtPassword.text)
            .OnSuccess(loginResult =>
            {
                //Set Authentication Context for this User
                VenlyAPI.SetProviderData(VyProvider_PlayFab.AuthContextDataKey, loginResult.AuthenticationContext);

                //Retrieve User Wallet
                VenlyAPI.ProviderExtensions.GetWalletForUser()
                    .OnSuccess(wallet =>
                    {
                        //Set Wallet Data
                        ViewManager.SetViewBlackboardData(
                            eApiExplorerViewId.WalletApi_WalletDetails,
                            ApiExplorer_WalletDetailsVC.DATAKEY_WALLET,
                            wallet);

                        //Success!
                        taskNotifier.NotifySuccess();
                    })
                    .OnFail(taskNotifier.NotifyFail);
            })
            .OnFail(taskNotifier.NotifyFail);

        //Task that triggers when all related sub-tasks are finished (fail or success)
        combinedTask
                .OnSuccess(() =>
                {
                    ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails);
                })
                .OnFail(ViewManager.HandleException)
                .Finally(ViewManager.Loader.Hide);
    }
#else
    private void onClick_LoginUser(){}
#endif
}
