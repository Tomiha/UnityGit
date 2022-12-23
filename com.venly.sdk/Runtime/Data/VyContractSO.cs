using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VenlySDK.Models;

namespace VenlySDK.Data
{
    public class VyContractSO : VyItemSO
    {
        internal override eVyItemType ItemType => eVyItemType.Contract;

        //TOKENS
        public List<VyTokenTypeSO> TokenTypes = new();

        #region Contract Fields

        [VyItemField(eVyItemTrait.LiveReadOnly)]
        public string Address;

        [VyItemField] public eVyChain Chain = eVyChain.Matic;

        [VyItemField(eVyItemTrait.Updateable)] public string Owner;
        [VyItemField(eVyItemTrait.Updateable)] public string Symbol;
        [VyItemField(eVyItemTrait.Updateable)] public List<_ItemTypeValue> Media = new();

        #endregion

        public VyContractDto ToModel()
        {
            return new VyContractDto
            {
                Address = Address,
                Confirmed = Confirmed,
                Description = Description,
                ExternalUrl = ExternalUrl,
                Id = Id,
                Image = ImageUrl,
                Media = Media.Select(e => e.ToModel()).ToArray(),
                Name = Name,
                Owner = Owner,
                Chain = Chain,
                TransactionHash = TransactionHash,
                Symbol = Symbol
            };
        }

#if UNITY_EDITOR
        public void UpdateLiveModel(VyContractDto model)
        {
            LiveModel = JsonConvert.SerializeObject(model ?? ToModel());
        }
#endif
    }
}