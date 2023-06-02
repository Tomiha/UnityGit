using System;
using UnityEngine;
using Venly.Models.Shared;

namespace Venly.Data
{
    public enum eVyItemState
    {
        Edit, //Live, but local edits (push)
        Live, //Live, and in sync
        Local //Local, go wild (push)
    }

    public enum eVyItemType
    {
        None,
        Contract,
        TokenType
    }

    public abstract class VyItemSO : ScriptableObject
    {
        [Serializable]
        public class _ItemTypeValue
        {
            public string Type = "";
            public string Value = "";

            public VyTypeValueDto ToModel()
            {
                return new VyTypeValueDto()
                {
                    Type = Type,
                    Value = Value
                };
            }

            public static _ItemTypeValue FromModel(VyTypeValueDto pair)
            {
                return new _ItemTypeValue()
                {
                    Type = pair.Type,
                    Value = pair.Value
                };
            }
        }

#if UNITY_EDITOR
        public bool HasLiveModel => !string.IsNullOrEmpty(LiveModel);
        public string LiveModel = null;
#endif

        internal virtual eVyItemType ItemType => eVyItemType.None;
        public bool IsContract => ItemType == eVyItemType.Contract;
        public bool IsTokenType => ItemType == eVyItemType.TokenType;

        public eVyItemState ItemState;
        public bool IsEdit => ItemState == eVyItemState.Edit;
        public bool IsLive => ItemState == eVyItemState.Live;
        public bool IsLocal => ItemState == eVyItemState.Local;

        public event Action OnItemUpdated;

        #region Shared Fields

        [VyItemField(eVyItemTrait.LiveReadOnly)]
        public bool Confirmed;

        [VyItemField(eVyItemTrait.LiveReadOnly)]
        public int Id;

        [VyItemField(eVyItemTrait.LiveReadOnly)]
        public string TransactionHash;

        [VyItemField(eVyItemTrait.Updateable)] public string Name;
        [VyItemField(eVyItemTrait.Updateable)] public string Description;
        [VyItemField(eVyItemTrait.Updateable)] public string ExternalUrl;
        [VyItemField(eVyItemTrait.Updateable)] public string ImageUrl;

        //Storage

        #endregion

        public void NotifyItemUpdated()
        {
            OnItemUpdated?.Invoke();
        }

        public void ChangeItemState(eVyItemState newState)
        {
            ItemState = newState;
        }

        public VyTokenTypeSO AsTokenType()
        {
            if (!IsTokenType) return null;
            return this as VyTokenTypeSO;
        }

        public VyContractSO AsContract()
        {
            if (!IsContract) return null;
            return this as VyContractSO;
        }
    }
}