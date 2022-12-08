using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Venly.Models;

namespace Venly.Data
{
    public class VyContractSO : VyItemSO
    {
        internal override eVyItemType ItemType => eVyItemType.Contract;

        //TOKENS
        public List<VyTokenTypeSO> TokenTypes = new();

        #region Contract Fields
        [VyItemField(eVyItemTrait.LiveReadOnly)] public string Address;
        [VyItemField] public eVySecretType Chain = eVySecretType.Matic;

        [VyItemField(eVyItemTrait.Updateable)] public string Owner;
        [VyItemField(eVyItemTrait.Updateable)] public string Symbol;
        [VyItemField(eVyItemTrait.Updateable)] public List<_ItemTypeValue> Media = new ();
        #endregion

        public VyContract ToModel()
        {
            return new VyContract
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
                SecretType = Chain,
                TransactionHash = TransactionHash,
                Symbol = Symbol
            };
        }

        public void UpdateLiveModel(VyContract model)
        {
            LiveModel = JsonConvert.SerializeObject(model??ToModel());
        }
    }
}