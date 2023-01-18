public class ApiExplorer_LandingRuntimeModeVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_LandingRuntimeModeVC() : 
        base(eApiExplorerViewId.Landing_RuntimeMode) { }

    protected override void OnActivate()
    {
        ShowNavigateBack = false;
        ShowNavigateHome = false;
        ShowRefresh = false;

        BindButton("btn-admin", onClick_Admin);
        BindButton("btn-user-wallet", onClick_ShowUserWallet);

        SetLabel("lbl-user-mail", GetBlackBoardData<string>("user-mail"));
        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
    }

    protected override void OnDeactivate()
    {
        
    }

    private void onClick_Admin()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Landing_DevMode);
    }

    private void onClick_ShowUserWallet()
    {
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails);
    }
}
