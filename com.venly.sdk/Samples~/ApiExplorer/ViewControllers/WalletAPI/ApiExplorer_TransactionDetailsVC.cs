using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;

public class ApiExplorer_TransactionDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    private eVyChain _transactionChain;
    private string _transactionHash;

    public ApiExplorer_TransactionDetailsVC() : 
        base(eApiExplorerViewId.WalletApi_TransactionDetails) { }

    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-refresh", Refresh);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowRefresh = false;

        _transactionChain = (eVyChain)GetBlackboardDataRaw("tx_chain");
        _transactionHash = GetBlackBoardData<string>("tx_hash");
        Refresh();
    }

    protected override void OnDeactivate()
    {
    }

    private void Refresh()
    {
        ViewManager.Loader.Show("Retrieving Transaction Info...");
        Venly.WalletAPI.Client.GetTransactionInfo(_transactionChain, _transactionHash)
            .OnSuccess(info =>
            {
                SetLabel("lbl-hash", info.Hash);
                SetLabel("lbl-status", Enum.GetName(typeof(eVyTransactionStatus),info.Status));
                SetLabel("lbl-confirmations", info.Confirmations.ToString());
                SetLabel("lbl-blockHash", info.BlockHash);
                SetLabel("lbl-blockNumber", info.BlockNumber.ToString());
                SetLabel("lbl-reachedFinality", info.HasReachedFinality?"YES":"NO");
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
}
