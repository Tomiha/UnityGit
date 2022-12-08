using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Venly.Data;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.ContractManager
{
    public class ContractManagerView : EditorWindow
    {
        private ContractListView _contractListView;
        private VisualElement _itemDetailsPanel;

        private ItemViewBase _contractView;
        private ItemViewBase _tokenTypeView;

        public void CreateGUI()
        {
            // Import UXML 
            var visualTree = ToolUtils.GetContractManagerUXML("ContractManagerView");
            visualTree.CloneTree(rootVisualElement);

            //Set ClientID & AppID
            rootVisualElement.Q<Label>("lbl-client-id").text = $"Client ID: {VenlySettings.ClientId}";
            rootVisualElement.Q<Label>("lbl-app-id").text = $"App ID: {VenlySettings.ApplicationId}";

            //Bind Sync
            rootVisualElement.Q<Button>("btn-sync").clickable.clicked += () =>
            {
                ContractManager.Instance.Sync();
                _contractListView.RefreshView();
                _contractListView.SelectFirst();
            };

            _itemDetailsPanel = rootVisualElement.Q<VisualElement>("item-details-panel");

            _contractView = new ContractView();
            _contractView.OnItemStateChange += OnItemStateChanged;

            _tokenTypeView = new TokenTypeView();
            _tokenTypeView.OnItemStateChange += OnItemStateChanged;

            _contractListView = rootVisualElement.Q<ContractListView>("contract-list-view");
            if (_contractListView == null)
            {
                Debug.LogException(new Exception("Failed to grab ContactListView"));
                return;
            }

            _contractListView.OnItemSelected += (item) =>
            {
                if (item == null)
                {
                    _itemDetailsPanel.Clear();

                    _contractView.Unbind();
                    _contractView.userData = null;

                    _tokenTypeView.Unbind();
                    _tokenTypeView.userData = null;
                }
                else if (item.IsContract)
                {
                    _itemDetailsPanel.Clear();
                    _itemDetailsPanel.Add(_contractView);
                    _contractView.BindItem(item);
                }
                else if (item.IsTokenType)
                {
                    _itemDetailsPanel.Clear();
                    _itemDetailsPanel.Add(_tokenTypeView);
                    _tokenTypeView.BindItem(item);
                }
            };

            //todo: store prev selection
            _contractListView.SelectFirst();
        }

        private void OnItemStateChanged(VyItemSO item)
        {
            _contractListView.UpdateItemState(item);
        }
    }
}