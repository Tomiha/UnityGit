public class ApiExplorer_LandingDevModeVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_LandingDevModeVC() : 
        base(eApiExplorerViewId.Landing_DevMode) { }

    protected override void OnActivate()
    {
        BindButton("btn-wallet-api", onClick_WalletAPI);
        BindButton("btn-nft-api", onClick_NftAPI);

        ShowNavigateBack = false;

#if ENABLE_VENLY_DEV_MODE
        SetLabel("lbl-client-id", VenlySettings.ClientId);
        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
#endif
    }

    protected override void OnDeactivate()
    {
        
    }

    private void onClick_WalletAPI()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Main_WalletApi);
    }

    private void onClick_NftAPI()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Main_NftApi);
    }

    private void onClick_LoginUser()
    {

    }

    private void onClick_CreateUser()
    {

    }
}
