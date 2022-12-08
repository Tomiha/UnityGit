using System.Linq;
using Proto.Promises;
using UnityEngine;
using Venly.Data;
using Venly.Models;
using Venly.Utils;

namespace Venly.Editor.Tools.ContractManager
{
    internal class ContractManager
    {
        #region Cstr
        private static ContractManager _instance;
        public static ContractManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ContractManager();
                    _instance.Initialize();
                }


                return _instance;
            }
        }
        #endregion

        private bool _isInitialize = false;
        public bool IsInitialize => _isInitialize;

        private VenlyEditorRequester _requester;
        public ContractManagerView MainView { get; internal set; }

        private void Initialize()
        {
            if (_isInitialize) return;

            _requester = new VenlyEditorRequester();

            _isInitialize = true;
        }

        public void Sync()
        {
            var storedContracts = Resources.LoadAll<VyContractSO>("");
            var pulledContracts = VenlyEditorAPI.GetContracts(VenlySettings.ApplicationId).WaitForResult();

            foreach (var pulledContract in pulledContracts)
            {
                var tokenTypes = VenlyEditorAPI.GetTokenTypes(VenlySettings.ApplicationId, (int)pulledContract.Id).WaitForResult();
                var sameContract = storedContracts.FirstOrDefault(c => c.Id == pulledContract.Id);

                //Create Contract
                if (sameContract == null)
                {
                    var newContract = ItemSO_Utils.CreateContract();
                    newContract.ChangeItemState(eVyItemState.Live);
                    newContract.FromModel(pulledContract);

                    foreach (var pulledTokenType in tokenTypes)
                    {
                        var newTokenType = ItemSO_Utils.CreateTokenType(newContract);
                        newTokenType.ChangeItemState(eVyItemState.Live);
                        newTokenType.FromModel(pulledTokenType);
                        
                        //ItemSO_Utils.SaveItem(newTokenType, true);
                    }

                    ItemSO_Utils.SaveItem(newContract, true);
                }
                //Update Contract
                else
                {
                    if(!sameContract.IsEdit) sameContract.FromModel(pulledContract);
                    else sameContract.UpdateLiveModel(pulledContract);

                    foreach (var pulledTokenType in tokenTypes)
                    {
                        var sameTokenType = sameContract.TokenTypes.FirstOrDefault(t => t.Id == pulledTokenType.Id);

                        if (sameTokenType == null) //Create TokenType
                        {
                            var newTokenType = ItemSO_Utils.CreateTokenType(sameContract);
                            newTokenType.ChangeItemState(eVyItemState.Live);
                            newTokenType.FromModel(pulledTokenType);

                            //ItemSO_Utils.SaveItem(newTokenType, true);
                        }
                        else //Update TokenType
                        {
                            if(!sameTokenType.IsEdit) sameTokenType.FromModel(pulledTokenType);
                            else sameTokenType.UpdateLiveModel(pulledTokenType);
                        }
                    }

                    ItemSO_Utils.SaveItem(sameContract, true);
                }
            }
        }

        public void PushItem(VyItemSO item)
        {
            if (item.IsContract) PushContract(item.AsContract());
            else if (item.IsTokenType) PushTokenType(item.AsTokenType());
        }

        public void RefreshItem(VyItemSO item)
        {
            if (item.IsLocal) return;

            if (item.IsContract) RefreshContract(item.AsContract());
            else if (item.IsTokenType) RefreshTokenType(item.AsTokenType());
        }

        public void UpdateItem(VyItemSO item)
        {
            if (item.IsLocal) return;

            if (item.IsContract) UpdateContract(item.AsContract());
            else if (item.IsTokenType) UpdateTokenType(item.AsTokenType());
        }

        public void RevertItem(VyItemSO item)
        {
            if (item.IsLocal) return;

            if (item.HasLiveModel)
            {
                item.ChangeItemState(eVyItemState.Live);
                item.Revert();
            }
            else
            {
                if(item.IsContract)RefreshContract(item.AsContract());
                else if(item.IsTokenType)RefreshTokenType(item.AsTokenType());
            }
        }

        public void ArchiveItem(VyItemSO item)
        {
            if (item.IsLocal) return;

            if (item.IsContract) ArchiveContract(item.AsContract());
            else if (item.IsTokenType) ArchiveTokenType(item.AsTokenType());
        }

        #region Implementations
        private void ArchiveTokenType(VyTokenTypeSO tokenType)
        {
            VenlyEditorAPI.ArchiveTokenType(VenlySettings.ApplicationId, tokenType.Contract.Id, tokenType.Id)
                .Then(() =>
                {
                    Debug.Log($"TokenType (id={tokenType.Id}) successfully archived!");
                })
                .CatchAndForget();
        }

        private void ArchiveContract(VyContractSO contract)
        {
            VenlyEditorAPI.ArchiveContract(VenlySettings.ApplicationId, contract.Id)
                .Then(()=>
                {
                    Debug.Log($"Contract (id={contract.Id}) successfully archived!");
                })
                .CatchAndForget();
        }

        private void UpdateTokenType(VyTokenTypeSO tokenType)
        {
            var model = tokenType.ToModel();
            VyParam_UpdateTokenTypeMetadata data = new()
            {
                ApplicationId = VenlySettings.ApplicationId,
                ContractId = tokenType.Contract.Id,
                Name = model.Name,
                AnimationUrls = model.AnimationUrls,
                Atrributes = model.Attributes,
                BackgroundColor = model.BackgroundColor,
                Description = model.Description,
                ExternalUrl = model.ExternalUrl,
                ImageUrl = model.Image,
                TokenTypeId = (int)model.Id //todo fix type
            };

            VenlyEditorAPI.UpdateTokenTypeMetadata(data)
                .WaitAsync(SynchronizationOption.Foreground)
                .Then(tokenTypeMetadata =>
                {
                    tokenType.ChangeItemState(eVyItemState.Live);
                    tokenType.FromMetadata(tokenTypeMetadata);
                    tokenType.RefreshTokenTexture();
                })
                .CatchAndForget();
        }

        private void UpdateContract(VyContractSO contract)
        {
            var model = contract.ToModel();
            VyParam_UpdateContractMetadata data = new()
            {
                ApplicationId = VenlySettings.ApplicationId,
                ContractId = (int)model.Id, //todo fix type
                Name = model.Name,
                Description = model.Description,
                ExternalUrl = model.ExternalUrl,
                ImageUrl = model.Image,
                Media = model.Media,
                Symbol = model.Symbol
            };

            VenlyEditorAPI.UpdateContractMetadata(data)
                .WaitAsync(SynchronizationOption.Foreground)
                .Then(contractMetadata =>
                {
                    contract.ChangeItemState(eVyItemState.Live);
                    contract.FromMetadata(contractMetadata);
                })
                .CatchAndForget();
        }

        private void RefreshContract(VyContractSO contract)
        {
            VenlyEditorAPI.GetContract(VenlySettings.ApplicationId, contract.Id)
                .WaitAsync(SynchronizationOption.Foreground)
                .Then(updatedContract =>
                {
                    contract.ChangeItemState(eVyItemState.Live);
                    contract.FromModel(updatedContract);
                })
                .CatchAndForget();
        }

        private void RefreshTokenType(VyTokenTypeSO tokenType)
        {
            VenlyEditorAPI.GetTokenType(VenlySettings.ApplicationId, tokenType.Contract.Id, tokenType.Id)
                .WaitAsync(SynchronizationOption.Foreground)
                .Then(updatedTokenType =>
                {
                    tokenType.ChangeItemState(eVyItemState.Live);
                    tokenType.FromModel(updatedTokenType);
                    tokenType.RefreshTokenTexture();
                })
                .CatchAndForget();
        }

        private void PushContract(VyContractSO contract)
        {
            var model = contract.ToModel();
            var data = new VyParam_CreateContract
            {
                Name = model.Name,
                ApplicationId = VenlySettings.ApplicationId,
                Chain = model.SecretType,
                Description = model.Description,
                ExternalUrl = model.ExternalUrl,
                Media = model.Media,
                Owner = model.Owner,
                Symbol = model.Symbol,
                ImageUrl = model.Image //todo: fix image vs imageUrl
            };

            VenlyEditorAPI.CreateContract(data)
                 .WaitAsync(SynchronizationOption.Foreground)
                .Then(newContract =>
                {
                    contract.ChangeItemState(eVyItemState.Live);
                    contract.FromModel(newContract);
                })
                .CatchAndForget();
        }

        private void PushTokenType(VyTokenTypeSO tokenType)
        {
            var model = tokenType.ToModel();
            var data = new VyParam_CreateTokenType
            {
                Name = model.Name,
                ApplicationId = VenlySettings.ApplicationId,
                AnimationUrls = model.AnimationUrls,
                Attributes = model.Attributes,
                BackgroundColor = model.BackgroundColor,
                Burnable = model.Burnable,
                ContractId = tokenType.Contract.Id,
                Description = model.Description,
                ExternalUrl = model.ExternalUrl,
                Destinations = null,
                Fungible = model.Fungible,
                ImageUrl = model.Image,
                MaxSupply = (int)model.MaxSupply //todo fix types
            };

            VenlyEditorAPI.CreateTokenType(data)
                .WaitAsync(SynchronizationOption.Foreground)
                .Then(newTokenType =>
                {
                    tokenType.ChangeItemState(eVyItemState.Live);
                    tokenType.FromModel(newTokenType);
                    tokenType.RefreshTokenTexture();
                })
                .CatchAndForget();
        }
        #endregion
    }
}