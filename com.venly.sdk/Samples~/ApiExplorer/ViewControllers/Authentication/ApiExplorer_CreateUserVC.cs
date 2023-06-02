using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;

#if ENABLE_VENLY_PLAYFAB
using UnityEngine;
using Venly.Backends.PlayFab;
#endif

public class ApiExplorer_CreateUserVC : SampleViewBase<eApiExplorerViewId>
{
    private TextField _txtEmail;
    private TextField _txtPassword;
    private TextField _txtPincode;

    public ApiExplorer_CreateUserVC() : 
        base(eApiExplorerViewId.Auth_Create) { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-create", onClick_CreateUser);

        GetElement(out _txtEmail, "txt-email");
        GetElement(out _txtPassword, "txt-password");
        GetElement(out _txtPincode, "txt-pincode");
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
    private void onClick_CreateUser()
    {
        ViewManager.Loader.Show("Logging in...");

        //Helper Task
        var taskNotifier = VyTask.Create();
        var combinedTask = taskNotifier.Task;

        PlayFabAuth.SignUp(_txtEmail.text, _txtPassword.text)
            .OnSuccess(loginResult =>
            {
                //Set Authentication Context for this User
                VenlyAPI.SetProviderData(VyProvider_PlayFab.AuthContextDataKey, loginResult.AuthenticationContext);

                //Wallet Creation Params
                var createParams = new VyCreateWalletRequest()
                {
                    Chain = eVyChain.Matic,
                    Description = $"API Explorer wallet created for \'{VenlySettings.BackendProvider}\' user.\n(PlayFabId={loginResult.PlayFabId})",
                    Identifier = $"{VenlySettings.BackendProvider}-provider-wallet",
                    Pincode = _txtPincode.text,
                    WalletType = eVyWalletType.WhiteLabel
                };

                //Create Wallet for User
                VenlyAPI.ProviderExtensions.CreateWalletForUser(createParams)
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
    private void onClick_CreateUser(){}
#endif
}
