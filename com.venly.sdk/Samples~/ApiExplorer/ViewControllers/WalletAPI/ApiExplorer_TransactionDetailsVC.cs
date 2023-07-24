using System;
using Venly;
using Venly.Models.Shared;

public class ApiExplorer_TransactionDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEY
    public const string DATAKEY_TXCHAIN = "tx_chain";
    public const string DATAKEY_TXHASH = "tx_hash";

    //DATA
    private eVyChain _transactionChain;
    private string _transactionHash;

    public ApiExplorer_TransactionDetailsVC() : 
        base(eApiExplorerViewId.WalletApi_TransactionDetails) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowRefresh = true;
        ShowNavigateHome = false;

        if (TryGetBlackboardDataRaw(out object txChain, localKey: DATAKEY_TXCHAIN))
        {
            _transactionChain = (eVyChain)txChain;
        }

        TryGetBlackboardData(out _transactionHash, localKey: DATAKEY_TXHASH);
    }

    protected override void OnRefreshUI()
    {
        //Validate
        if (!ValidateData(_transactionChain, DATAKEY_TXCHAIN)) return;
        if (!ValidateData(_transactionHash, DATAKEY_TXHASH)) return;

        //Execute
        ViewManager.Loader.Show("Retrieving Transaction Info...");
        VenlyAPI.Wallet.GetTransactionInfo(_transactionChain, _transactionHash)
            .OnSuccess(info =>
            {
                SetLabel("lbl-hash", info.Hash);
                SetLabel("lbl-status", Enum.GetName(typeof(eVyTransactionState),info.Status));
                SetLabel("lbl-confirmations", info.Confirmations.ToString());
                SetLabel("lbl-blockHash", info.BlockHash);
                SetLabel("lbl-blockNumber", info.BlockNumber.ToString());
                SetLabel("lbl-reachedFinality", info.HasReachedFinality?"YES":"NO");
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
