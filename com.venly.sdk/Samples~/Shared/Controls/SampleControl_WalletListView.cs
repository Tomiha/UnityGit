using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK;
using VenlySDK.Models;
using VenlySDK.Utils;

public class SampleWalletItem : ScriptableObject
{
    public string Id;
    public string Address;
    public eVyChain Chain;

    public VyWalletDto Data;

    public static SampleWalletItem FromDto(VyWalletDto wallet)
    {
        var so = ScriptableObject.CreateInstance<SampleWalletItem>();
        so.Data = wallet;

        so.Id = wallet.Id;
        so.Address = wallet.Address;
        so.Chain = wallet.Chain;

        return so;
    }

    public static SampleWalletItem CreateMock()
    {
        var so = ScriptableObject.CreateInstance<SampleWalletItem>();

        so.Id = "91c59db2-dbf9-480e-a709-be8558ff1109";
        so.Address = "0xD1424eD3a1E14A5403f91BE07355e05c5826A9af";
        so.Chain = eVyChain.Matic;

        return so;
    }
}

public class SampleControl_WalletListView : VisualElement
{
    private VisualTreeAsset _walletItemProto;
    private ListView _listView;

    public event Action<VyWalletDto> OnItemSelected;

    public new class UxmlFactory : UxmlFactory<SampleControl_WalletListView, UxmlTraits>
    { }

    public SampleControl_WalletListView()
    {
        var tree = Resources.Load<VisualTreeAsset>("SampleControl_WalletListView");
        tree.CloneTree(this);

        _walletItemProto = Resources.Load<VisualTreeAsset>("SampleControl_WalletListItem");
        _listView = this.Q<ListView>();
        _listView.onItemsChosen += (itemList) =>
        {
            OnItemSelected?.Invoke(itemList.First() as VyWalletDto);
        };

        //PopulateListViewMock();
    }

    public void PopulateListView(List<VyWalletDto> wallets)
    {
        _listView.makeItem = () => _walletItemProto.Instantiate();
        _listView.bindItem = (e, i) =>
        {
            var walletData = wallets[i];
            e.SetLabel("lbl-wallet-chain", walletData.Chain.GetMemberName());
            e.SetLabel("lbl-wallet-id", walletData.Id);
            e.SetLabel("lbl-wallet-address", walletData.Address);
            e.SetDisplay("lbl-wallet-archived", walletData.Archived);
        };
        _listView.itemsSource = wallets;
        _listView.RefreshItems();
    }

    public void PopulateListViewMock()
    {
        var wallets = new List<VyWalletDto>
        {
            new VyWalletDto(),
            new VyWalletDto(),
            new VyWalletDto()
        };

        _listView.makeItem = () => _walletItemProto.Instantiate();
        _listView.bindItem = (e, i) =>
        {
            var walletSO = SampleWalletItem.CreateMock();
            e.userData = walletSO;
            e.Bind(new SerializedObject(walletSO));
        };
        _listView.itemsSource = wallets;
    }
}
