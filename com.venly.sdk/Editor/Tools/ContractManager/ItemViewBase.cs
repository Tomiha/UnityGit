using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VenlySDK.Data;
using VenlySDK.Editor.Tools.Controls;
using VenlySDK.Editor.Utils;

namespace VenlySDK.Editor.Tools.ContractManager
{
    public abstract class ItemViewBase : VisualElement
    {
        public event Action<VyItemSO> OnItemStateChange;

        protected SerializedObject _serializedItem;
        protected VyItemSO _item;

        protected VisualElement _itemDataContainer { get; set; }

        protected Button _btnPush { get; set; }
        protected Button _btnEdit { get; set; }
        protected Button _btnUpdate { get; set; }
        protected Button _btnCancel { get; set; }
        protected Button _btnRefresh { get; set; }

        protected ItemViewBase(string parentUxml)
        {
            style.flexGrow = new StyleFloat(1);

            var visualElement = VenlyEditorUtils.GetUXML_ContractManager(parentUxml);
            visualElement.CloneTree(this);

            _itemDataContainer = this.Q<VisualElement>("item-data");
            _btnPush = this.Q<Button>("btn-push");
            _btnEdit = this.Q<Button>("btn-edit");
            _btnUpdate = this.Q<Button>("btn-update");
            _btnCancel = this.Q<Button>("btn-cancel");
            _btnRefresh = this.Q<Button>("btn-refresh");

            _btnRefresh.clickable.clicked += () =>
            {
                ContractManager.Instance.RefreshItem(_item);
            };

            _btnPush.clickable.clicked += () =>
            {
                ContractManager.Instance.PushItem(_item);
            };

            _btnEdit.clickable.clicked += () =>
            {
                _item.ChangeItemState(eVyItemState.Edit);
                OnItemStateChange?.Invoke(_item);

                RefreshView();
            };

            _btnCancel.clickable.clicked += () =>
            {
                ContractManager.Instance.RevertItem(_item);
            };

            _btnUpdate.clickable.clicked += () =>
            {
                ContractManager.Instance.UpdateItem(_item);
            };
        }

        public void BindItem(VyItemSO item)
        {
            if (_item != null)
            {
                _item.OnItemUpdated -= OnItemUpdated;
                OnBindingRelease(_item);
                this.Unbind();
            }

            _item = item;
            _item.OnItemUpdated += OnItemUpdated;

            _serializedItem = new SerializedObject(item);

            this.userData = item;
            this.Bind(_serializedItem);

            OnBindingUpdate(item);

            RefreshView();
        }

        protected virtual void OnBindingRelease(VyItemSO item) {}

        protected virtual void OnBindingUpdate(VyItemSO item){}

        private void OnItemUpdated()
        {
            OnItemStateChange?.Invoke(_item);
            OnBindingUpdate(_item); //little hack to force list update

            RefreshView();
        }

        private void RefreshView()
        {
            RefreshHeader();
            RefreshFields();
        }

        private void RefreshHeader()
        {
            _btnPush.ToggleElement(_item.IsLocal);
            _btnEdit.ToggleElement(_item.IsLive);
            _btnUpdate.ToggleElement(_item.IsEdit);
            _btnCancel.ToggleElement(_item.IsEdit);
            _btnRefresh.ToggleElement(_item.IsLive);
        }


        private void RefreshFields()
        {
            var itemType = _item?.GetType();
            if (itemType == null) return;

            foreach (var element in _itemDataContainer.Children())
            {
                if (element is not ControlBaseRW) continue;

                var bindingName = element.GetType().GetProperty("BindingPath")?.GetValue(element) as string;

                if (string.IsNullOrEmpty(bindingName)) continue;

                var fieldInfo = itemType.GetField(bindingName);
                var itemAttr = fieldInfo?.GetCustomAttribute<VyItemFieldAttribute>();

                if (itemAttr == null) continue;

                (element as ControlBaseRW).RefreshMode(GetEditMode(itemAttr));
            }
        }

        private ControlBaseRW.eEditMode GetEditMode(VyItemFieldAttribute attr)
        {
            if (_item.IsLive) return ControlBaseRW.eEditMode.Read;

            if (_item.IsEdit)
            {
                if (attr.IsUpdateable) return ControlBaseRW.eEditMode.Write;
                return ControlBaseRW.eEditMode.Read;
            }
            
            if (_item.IsLocal)
            {
                if (attr.IsLiveOnly) return ControlBaseRW.eEditMode.Hidden;
                if (attr.IsReadOnly) return ControlBaseRW.eEditMode.Read;
            }

            return ControlBaseRW.eEditMode.Write;
        }
    }
}
