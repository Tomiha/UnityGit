using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Data;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.ContractManager
{
    public class ContractListView : VisualElement
    {
        public List<VyContractSO> VisibleContracts = new();
        private Dictionary<VyItemSO, VisualElement> _itemToElement = new();

        public event Action<VyItemSO> OnItemSelected;

        private VisualTreeAsset _contractItemProto;
        private VisualTreeAsset _tokenTypeItemProto;
        private VisualTreeAsset _rootProto;

        private VisualElement _itemContainer;

        //private VyItemSO _selectedItem = null;
        private VisualElement _selectedElement = null;
        private VyItemSO _selectedItem => _selectedElement?.userData as VyItemSO;

        #region Cstr

        [MenuItem("Window/Venly/Contract Manager")]
        public static void OpenWindows()
        {
            ContractManager.Instance.MainView = EditorWindow.GetWindow<ContractManagerView>();
            ContractManager.Instance.MainView.titleContent = new GUIContent("Venly Contract Manager");
        }

        public new class UxmlFactory : UxmlFactory<ContractListView, UxmlTraits> {}

        public ContractListView()
        {
            _rootProto = VenlyEditorUtils.GetUXML_ContractManager("ContractListView");
            _contractItemProto = VenlyEditorUtils.GetUXML_ContractManager("ContractItemProto");
            _tokenTypeItemProto = VenlyEditorUtils.GetUXML_ContractManager("TokenTypeItemProto");

            //Build View
            RefreshView();
        }

        #endregion

        public void SelectFirst()
        {
            if (VisibleContracts.Count > 0)
            {
                Select(VisibleContracts[0]);
            }
            else
            {
                Select(null);
            }
        }

        public void RefreshView()
        {
            //Load MockData
            VisibleContracts = new List<VyContractSO>(Resources.LoadAll<VyContractSO>(""));

            //Clear Tree
            Clear();
            _itemToElement.Clear();

            _rootProto.CloneTree(this);
            _itemContainer = this.Q<VisualElement>("container-list");
            this.Q<Button>("contract-action-add").clicked += () => { OnAddContract(); };

            //Rebuild Tree
            foreach (var contract in VisibleContracts)
            {
                //Create Contract Item
                AppendContractItem(contract);

                //Build TokenType Tree
                RebuildTokenTypeList(contract);
            }
        }

        #region ListView Helpers

        private void AppendContractItem(VyContractSO contract)
        {
            AppendContractItem(contract, _itemContainer);
        }

        private void AppendContractItem(VyContractSO contract, VisualElement root)
        {
            //Clear from LUT if present + remove from hierarchy
            RemoveContractItem(contract);

            var contractItem = _contractItemProto.CloneTree();
            contractItem.Bind(new SerializedObject(contract));

            //STATE UI Logic
            UpdateItemState(contract, contractItem);

            //Bind Events
            contractItem.AddManipulator(new Clickable(OnClick_ItemSelected));
            contractItem.Q<Button>("contract-action-delete").clicked += () => { OnDeleteContract(contract); };
            contractItem.Q<Button>("tokentype-action-add").clicked += () => { OnAddTokenType(contract); };

            //Set Userdata
            contractItem.userData = contract;

            root.Add(contractItem);

            //Add to LUT
            _itemToElement.Add(contract, contractItem);
        }

        private void RemoveContractItem(VyContractSO contract)
        {
            if (!_itemToElement.ContainsKey(contract))
                return;

            //Remove TokenTypes
            contract.TokenTypes.ForEach(RemoveTokenTypeItem);

            //Remove Contract
            _itemToElement[contract].RemoveFromHierarchy();
            _itemToElement.Remove(contract);

            //Remove From VisibleContracts
            VisibleContracts.Remove(contract);
        }

        private void RebuildTokenTypeList(VyContractSO contract)
        {
            var contractContainer = _itemToElement[contract];

            var tokenTypeContainer = contractContainer.Q<VisualElement>("container-tokentypes");
            tokenTypeContainer.Clear();

            //Clear TokenType References + Rebuild Tree
            contract.TokenTypes.ForEach(tt =>
            {
                //Remove from LUT
                if (_itemToElement.ContainsKey(tt))
                    _itemToElement.Remove(tt);

                //Rebuild Item
                AppendTokenTypeItem(tt, tokenTypeContainer);
            });
        }

        private void AppendTokenTypeItem(VyTokenTypeSO tokenType, VyContractSO contract)
        {
            var contractItem = _itemToElement[contract];
            var tokenTypeContainer = contractItem.Q<VisualElement>("container-tokentypes");
            AppendTokenTypeItem(tokenType, tokenTypeContainer);
        }

        private void AppendTokenTypeItem(VyTokenTypeSO tokenType, VisualElement root)
        {
            //Clear from LUT if present + remove from hierarchy
            RemoveTokenTypeItem(tokenType);

            //Build new item
            var tokenTypeItem = _tokenTypeItemProto.CloneTree();
            tokenTypeItem.Bind(new SerializedObject(tokenType));

            //STATE UI Logic
            UpdateItemState(tokenType, tokenTypeItem);

            //Bind Events
            tokenTypeItem.AddManipulator(new Clickable(OnClick_ItemSelected));
            tokenTypeItem.Q<Button>("tokentype-action-delete").clicked += () => { OnDeleteTokenType(tokenType); };

            //Set Userdata
            tokenTypeItem.userData = tokenType;

            root.Add(tokenTypeItem);

            //Add to LUT
            _itemToElement.Add(tokenType, tokenTypeItem);
        }

        private void RemoveTokenTypeItem(VyTokenTypeSO tokenType)
        {
            if (!_itemToElement.ContainsKey(tokenType))
                return;

            _itemToElement[tokenType].RemoveFromHierarchy();
            _itemToElement.Remove(tokenType);
        }

        public void UpdateItemState(VyItemSO item, VisualElement element = null)
        {
            if (element == null)
                element = _itemToElement[item];

            var statusContainer = element.Q<VisualElement>("item-status");
            var statusLabel = statusContainer.Q<Label>("item-status-text");

            statusLabel.text = Enum.GetName(typeof(eVyItemState), item.ItemState)?.ToUpper();
            statusContainer.EnableInClassList("item-status--LOCAL", item.IsLocal);
            statusContainer.EnableInClassList("item-status--LIVE-EDIT", item.IsEdit);
        }

        #endregion

        public void Select(VyItemSO item)
        {
            if (item == null) ToggleSelection(null);
            else ToggleSelection(_itemToElement[item]);

            OnItemSelected?.Invoke(item);
        }

        public void SaveCurrentItem()
        {
            if(_selectedItem != null)
                ItemSO_Utils.SaveItem(_selectedItem);
        }

        private void OnClick_ItemSelected(EventBase evt)
        {
            evt.StopImmediatePropagation();

            if (evt.currentTarget is TemplateContainer container)
            {
                Select(container.userData as VyItemSO);
            }
        }

        private void OnDeleteTokenType(VyTokenTypeSO tokenType)
        {
            //Remove from chain
            ContractManager.Instance.ArchiveItem(tokenType);

            //Remove From Hierarchy
            RemoveTokenTypeItem(tokenType);

            //Select Parent Contract
            Select(tokenType.Contract);

            //Remove Token
            ItemSO_Utils.RemoveTokenType(tokenType);
        }

        private void OnDeleteContract(VyContractSO contract)
        {
            //Remove from chain
            ContractManager.Instance.ArchiveItem(contract);

            //Remove From Hierarchy
            RemoveContractItem(contract);

            //Select First (for now)
            SelectFirst();

            //Remove Contract
            ItemSO_Utils.RemoveContract(contract);
        }

        private void OnAddTokenType(VyContractSO contract)
        {
            //Create new token
            var newToken = ItemSO_Utils.CreateTokenType(contract);

            //Append To Tree
            AppendTokenTypeItem(newToken, contract);

            //Select new TokenType
            Select(newToken);
        }

        private void OnAddContract()
        {
            //Create new Contract
            var newContract = ItemSO_Utils.CreateContract();

            //Append To Tree
            AppendContractItem(newContract);

            //Select new Contract
            Select(newContract);
        }

        private void ToggleSelection(VisualElement element)
        {
            SaveCurrentItem();

            //Clear Previous Selection
            _selectedElement?.Q<VisualElement>("root-item")?.ToggleInClassList("item--selected");

            //Create New Selection
            _selectedElement = element;

            if(element != null)
                _selectedElement.Q<VisualElement>("root-item")?.ToggleInClassList("item--selected");
        }
    }
}