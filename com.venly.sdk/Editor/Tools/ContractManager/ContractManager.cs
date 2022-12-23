using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proto.Promises;
using UnityEditor;
using UnityEngine;
using VenlySDK.Core;
using VenlySDK.Utils;
using VenlySDK.Data;
using VenlySDK.Models;

namespace VenlySDK.Editor.Tools.ContractManager
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

        public VyTask Sync()
        {
            var taskNotifier = VyTask.Create();

            //Retrieve Data (Async)
            var loadTaskNotifier = VyTask<Dictionary<VyContractDto, VyTokenTypeDto[]>>.Create();
            Task.Run(async () =>
            {
                Dictionary<VyContractDto, VyTokenTypeDto[]> liveContracts = new Dictionary<VyContractDto, VyTokenTypeDto[]>();

                var contractsResult = await VenlyEditorAPI.GetContracts();
                if (!contractsResult.Success)
                {
                    loadTaskNotifier.NotifyFail("[ContractManager] Failed to retrieve live contracts");
                    return;
                }

                foreach (var contractDto in contractsResult.Data)
                {
                    var tokenTypesResult = await VenlyEditorAPI.GetTokenTypes(contractDto.Id);
                    if (!tokenTypesResult.Success)
                    {
                        loadTaskNotifier.NotifyFail($"[ContractManager] Failed to retrieve live TokenTypes for Contract ({contractDto.Id})");
                        return;
                    }

                    liveContracts.Add(contractDto, tokenTypesResult.Data);
                }

                loadTaskNotifier.NotifySuccess(liveContracts);
            });

            loadTaskNotifier.Task
                .OnComplete((result) =>
                {
                    if (!result.Success)
                    {
                        taskNotifier.NotifyFail(result.Exception);
                        return;
                    }

                    var storedContracts = Resources.LoadAll<VyContractSO>("");

                    //Sync Contracts
                    foreach (var kvp in result.Data)
                    {
                        var pulledContract = kvp.Key;
                        var tokenTypes = kvp.Value;

                        //TODO: handle invalid contracts...
                        //Skip if Address is null
                        if (string.IsNullOrEmpty(pulledContract.Address))
                        {
                            Debug.LogWarning($"[Contract Manager] Skipping Contract (id={pulledContract.Id} | name={pulledContract.Name}) because it has an invalid address (address={pulledContract.Address}).");
                            continue;
                        }

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
                            if (!sameContract.IsEdit) sameContract.FromModel(pulledContract);
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
                                    if (!sameTokenType.IsEdit) sameTokenType.FromModel(pulledTokenType);
                                    else sameTokenType.UpdateLiveModel(pulledTokenType);
                                }
                            }

                            ItemSO_Utils.SaveItem(sameContract, true);
                        }
                    }

                    //AssetDatabase.Refresh();
                    taskNotifier.NotifySuccess();
                });

            return taskNotifier.Task;
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
            VenlyEditorAPI.ArchiveTokenType(tokenType.Contract.Id, tokenType.Id)
                .OnComplete(result =>
                {
                    if (result.Success) Debug.Log($"TokenType (id={tokenType.Id}) successfully archived!");
                    else
                        Debug.LogException(new Exception($"Failed to Archive TokenType (id={tokenType.Id})",
                            result.Exception));
                })
                .OnFail(Debug.LogException);
        }

        private void ArchiveContract(VyContractSO contract)
        {
            VenlyEditorAPI.ArchiveContract(contract.Id)
                .OnComplete(result =>
                {
                    if (result.Success) Debug.Log($"Contract (id={contract.Id}) successfully archived!");
                    else Debug.LogException(new Exception($"Failed to Archive Contract (id={contract.Id})", result.Exception));
                })
                .OnFail(Debug.LogException);
        }

        private void UpdateTokenType(VyTokenTypeSO tokenType)
        {
            var model = tokenType.ToModel();
            VyUpdateTokenTypeMetadataDto data = new()
            {
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
                .OnSucces(tokenTypeMetadata =>
                {
                    tokenType.ChangeItemState(eVyItemState.Live);
                    tokenType.FromMetadata(tokenTypeMetadata);
                    tokenType.RefreshTokenTexture();
                })
                .OnFail(Debug.LogException);
        }

        private void UpdateContract(VyContractSO contract)
        {
            var model = contract.ToModel();
            VyUpdateContractMetadataDto data = new()
            {
                ContractId = (int)model.Id, //todo fix type
                Name = model.Name,
                Description = model.Description,
                ExternalUrl = model.ExternalUrl,
                ImageUrl = model.Image,
                Media = model.Media,
                Symbol = model.Symbol
            };

            VenlyEditorAPI.UpdateContractMetadata(data)
                .OnSucces(contractMetadata =>
                {
                    contract.ChangeItemState(eVyItemState.Live);
                    contract.FromMetadata(contractMetadata);
                })
                .OnFail(Debug.LogException);
        }

        private void RefreshContract(VyContractSO contract)
        {
            VenlyEditorAPI.GetContract(contract.Id)
                .OnSucces(updatedContract =>
                {
                    contract.ChangeItemState(eVyItemState.Live);
                    contract.FromModel(updatedContract);
                })
                .OnFail(Debug.LogException);
        }

        private void RefreshTokenType(VyTokenTypeSO tokenType)
        {
            VenlyEditorAPI.GetTokenType(tokenType.Contract.Id, tokenType.Id)
                .OnSucces(updatedTokenType =>
                {
                    tokenType.ChangeItemState(eVyItemState.Live);
                    tokenType.FromModel(updatedTokenType);
                    tokenType.RefreshTokenTexture();
                })
                .OnFail(Debug.LogException);
        }

        private void PushContract(VyContractSO contract)
        {
            var model = contract.ToModel();
            var data = new VyCreateContractDto
            {
                Name = model.Name,
                Chain = model.Chain,
                Description = model.Description,
                ExternalUrl = model.ExternalUrl,
                Media = model.Media,
                Owner = model.Owner,
                Symbol = model.Symbol,
                ImageUrl = model.Image //todo: fix image vs imageUrl
            };

            VenlyEditorAPI.CreateContract(data)
                .OnSucces(newContract =>
                {
                    contract.ChangeItemState(eVyItemState.Live);
                    contract.FromModel(newContract);
                })
                ;//.OnFail(Debug.LogException);
        }

        private void PushTokenType(VyTokenTypeSO tokenType)
        {
            var model = tokenType.ToModel();
            var data = new VyCreateTokenTypeDto
            {
                Name = model.Name,
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
                .OnSucces(newTokenType =>
                {
                    tokenType.ChangeItemState(eVyItemState.Live);
                    tokenType.FromModel(newTokenType);
                    tokenType.RefreshTokenTexture();
                })
                .OnFail(Debug.LogException);
        }
        #endregion
    }
}