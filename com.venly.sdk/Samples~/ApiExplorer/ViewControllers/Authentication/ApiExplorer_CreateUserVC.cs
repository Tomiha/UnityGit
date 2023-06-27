using System;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;

using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;

#if ENABLE_VENLY_PLAYFAB
using Venly.Backends.PlayFab;
#elif ENABLE_VENLY_BEAMABLE
using Beamable;
using Beamable.Player;
using Venly.Backends.Beamable;
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
#elif ENABLE_VENLY_BEAMABLE
    private void onClick_CreateUser()
    {
        ViewManager.Loader.Show("Logging in...");

        //Helper Task
        var taskNotifier = VyTask.Create();
        var combinedTask = taskNotifier.Task;

        var ctx = BeamContext.Default;

        taskNotifier.Scope(async () =>
            {
                await ctx.OnReady;

                //Provide BeamContext to Beamable Provider
                VenlyAPI.SetProviderData(VyProvider_Beamable.BeamContextDataKey, ctx);

                //Check if email is available
                var isAvailable = await ctx.Accounts.IsEmailAvailable(_txtEmail.text);
                if (!isAvailable) throw new Exception("Email is already used.");

                //Login with Credentials
                //await ctx.Accounts.OnReady;
                var newAccount = await ctx.Accounts.CreateNewAccount();

                var operation = await newAccount.AddEmail(_txtEmail.text, _txtPassword.text);
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

                //Wallet Creation Params
                var createParams = new VyCreateWalletRequest()
                {
                    Chain = eVyChain.Matic,
                    Description = $"API Explorer wallet created for \'{VenlySettings.BackendProvider}\' user.\n(BeamableId={ctx.AuthorizedUser.Value.id})",
                    Identifier = $"{VenlySettings.BackendProvider}-provider-wallet",
                    Pincode = _txtPincode.text,
                    WalletType = eVyWalletType.WhiteLabel
                };

                //Create Wallet for User
                var createResult = await VenlyAPI.ProviderExtensions.CreateWalletForUser(createParams);
                if (createResult.Success)
                {
                    //Set Wallet Data
                    ViewManager.SetViewBlackboardData(
                        eApiExplorerViewId.WalletApi_WalletDetails,
                        ApiExplorer_WalletDetailsVC.DATAKEY_WALLET,
                        createResult.Data);
                }
                else
                {
                    throw createResult.Exception;
                }
            });


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