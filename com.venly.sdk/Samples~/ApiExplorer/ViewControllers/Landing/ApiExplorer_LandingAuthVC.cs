public class ApiExplorer_LandingAuthVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_LandingAuthVC() : 
        base(eApiExplorerViewId.Landing_Auth) { }

    protected override void OnActivate()
    {
        ShowNavigateBack = false;
        ShowNavigateHome = false;
        ShowRefresh = false;

        BindButton("btn-login-user", onClick_LoginUser);
        BindButton("btn-create-user", onClick_CreateUser);

        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
    }

    private void onClick_LoginUser()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Auth_Login);
    }

    private void onClick_CreateUser()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Auth_Create);
    }
}
