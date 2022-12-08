using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Venly.Data;
using Venly.Models;

namespace Venly.Editor
{
    public static class ItemSO_Utils
    {
        #region Contract Utils
        public static VyContractSO CreateContract()
        {
            var newContract = ScriptableObject.CreateInstance<VyContractSO>();

            //Get Current Contracts
            var existingContracts = Resources.LoadAll<VyContractSO>("");

            //Name
            var contractName = "";
            var suffix = 0;
            while (true)
            {
                contractName = $"Contract_{suffix}";
                ++suffix;

                if (!existingContracts.Any(c => c.name.Equals(contractName))) break;
            }

            newContract.Name = contractName;
            newContract.name = contractName;
            newContract.ChangeItemState(eVyItemState.Local);

            //todo: move to editor assembly part
            AssetDatabase.CreateAsset(newContract, $"{VenlySettings.PublicResourceRoot}/{contractName}.asset");

            EditorUtility.SetDirty(newContract);
            AssetDatabase.SaveAssetIfDirty(newContract);

            return newContract;
        }

        public static void RemoveContract(VyContractSO target)
        {
            var path = AssetDatabase.GetAssetPath(target);

            if(!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(path);
        }

        public static void FromMetadata(this VyContractSO contract, VyContractMetadata model, bool updateLiveModel = true)
        {
            contract.Name = model.Name;
            contract.Symbol = model.Symbol;
            contract.Description = model.Description;
            contract.ExternalUrl = model.ExternalUrl;
            contract.ImageUrl = model.ImageUrl;

            //Update Media
            contract.Media.Clear();
            contract.Media.AddRange(model.Media.Select(VyItemSO._ItemTypeValue.FromModel));

            if (updateLiveModel)
                contract.UpdateLiveModel(null);

            SaveItem(contract);
            contract.NotifyItemUpdated();
        }

        public static void FromModel(this VyContractSO contract, VyContract model, bool updateLiveModel = true)
        {
            contract.Address = model.Address;
            contract.Confirmed = model.Confirmed;
            contract.Description = model.Description;
            contract.ExternalUrl = model.ExternalUrl;
            contract.Id = (int)model.Id; //todo fix types
            contract.ImageUrl = model.Image;
            contract.Name = model.Name;
            contract.Owner = model.Owner;
            contract.Chain = model.SecretType;
            contract.TransactionHash = model.TransactionHash;
            contract.Symbol = model.Symbol;

            //Update Media
            contract.Media.Clear();
            contract.Media.AddRange(model.Media.Select(VyItemSO._ItemTypeValue.FromModel));

            if (updateLiveModel)
                contract.UpdateLiveModel(model);

            SaveItem(contract);
            contract.NotifyItemUpdated();
        }
        #endregion

        #region TokenType Utils
        public static VyTokenTypeSO CreateTokenType(VyContractSO parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("Failed to create TokenType SO. (Contract is null)");
                return null;
            }

            var newTokenType = ScriptableObject.CreateInstance<VyTokenTypeSO>();

            //Name
            var tokenName = "";
            var tokenSuffix = 0;
            while (true)
            {
                tokenName = $"TokenType_{tokenSuffix}";
                ++tokenSuffix;

                if (!parent.TokenTypes.Any(t => t.Name.Equals(tokenName))) break;
            }

            newTokenType.Name = tokenName;
            newTokenType.name = tokenName;
            newTokenType.ChangeItemState(eVyItemState.Local);
            newTokenType.Contract = parent;

            parent.TokenTypes.Add(newTokenType);

            AssetDatabase.AddObjectToAsset(newTokenType, parent);

            EditorUtility.SetDirty(parent);
            AssetDatabase.SaveAssetIfDirty(parent);

            return newTokenType;
        }

        public static void RemoveTokenType(VyTokenTypeSO target)
        {
            var targetContract = target.Contract;
            if (targetContract == null)
            {
                Debug.LogWarning("Failed to remove TokenType SO. (Not associated with a Contract)");
                return;
            }

            if (!targetContract.TokenTypes.Contains(target))
            {
                Debug.LogWarning("Failed to remove TokenType SO. (TokenType not a part of the associated Contract)");
                return;
            }

            //Remove TokenType
            AssetDatabase.RemoveObjectFromAsset(target);

            var path = AssetDatabase.GetAssetPath(target);
            if(!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target));

            targetContract.TokenTypes.Remove(target);

            EditorUtility.SetDirty(targetContract);
            AssetDatabase.SaveAssetIfDirty(targetContract);
        
        }

        public static void RefreshTokenTexture(this VyTokenTypeSO tokenType)
        {
            if (tokenType.ImageUrl == tokenType.TextureUrl)
            {
                return;
            }

            if (string.IsNullOrEmpty(tokenType.ImageUrl))
            {
                tokenType.UpdateTokenTextureAsset(null);
            }
            else
            {
                var currUrl = tokenType.ImageUrl;
                VenlyUnityUtils.DownloadImage(currUrl)
                    .Then(tex => tokenType.UpdateTokenTextureAsset(tex, currUrl))
                    .Catch((Exception ex) =>
                    {
                        tokenType.UpdateTokenTextureAsset(null);
                    })
                    .Forget();
            }
        }

        private static void UpdateTokenTextureAsset(this VyTokenTypeSO tokenType, Texture2D newTexture, string textureUrl = "")
        {
            if (tokenType.TokenTexture == newTexture) return;

            if (tokenType.TokenTexture != null)
            {
                AssetDatabase.RemoveObjectFromAsset(tokenType.TokenTexture);
                tokenType.TokenTexture = null;
                tokenType.TextureUrl = textureUrl;
            }

            if (newTexture != null)
            {
                AssetDatabase.AddObjectToAsset(newTexture, tokenType);
                tokenType.TokenTexture = newTexture;
                tokenType.TextureUrl = textureUrl;
            }

            AssetDatabase.SaveAssetIfDirty(tokenType);
            tokenType.NotifyTextureUpdated();
        }

        public static void FromMetadata(this VyTokenTypeSO tokenType, VyTokenTypeMetadata model, bool updateLiveModel = true)
        {
            tokenType.Name = model.Name;
            tokenType.BackgroundColor = model.BackgroundColor;
            tokenType.Description = model.Description;
            tokenType.ExternalUrl = model.ExternalUrl;
            tokenType.ImageUrl = model.Image;
            tokenType.ImagePreview = model.ImagePreview;
            tokenType.ImageThumbnail = model.ImageThumbnail;
            
            //Attributes
            tokenType.Attributes.Clear();
            tokenType.Attributes.AddRange(model.Attributes.Select(VyTokenTypeSO._TokenAttribute.FromModel));

            //Update AnimationUrls
            tokenType.AnimationUrls.Clear();
            tokenType.AnimationUrls.AddRange(model.AnimationUrls.Select(VyItemSO._ItemTypeValue.FromModel));

            if (updateLiveModel)
                tokenType.UpdateLiveModel(null);

            tokenType.RefreshTokenTexture();

            SaveItem(tokenType, true);
            tokenType.NotifyItemUpdated();
        }

        public static void FromModel(this VyTokenTypeSO tokenType, VyTokenType model, bool updateLiveModel = true)
        {
            tokenType.Name = model.Name;
            tokenType.BackgroundColor = model.BackgroundColor;
            tokenType.Burnable = model.Burnable;
            tokenType.MaxSupply = (int)model.MaxSupply; //todo fix types
            tokenType.Confirmed = model.Confirmed;
            tokenType.CurrentSupply = model.CurrentSupply;
            tokenType.Description = model.Description;
            tokenType.ExternalUrl = model.ExternalUrl;
            tokenType.Fungible = model.Fungible;
            tokenType.Id = (int)model.Id; //todo fix types
            tokenType.ImageUrl = model.Image;
            tokenType.ImagePreview = model.ImagePreview;
            tokenType.ImageThumbnail = model.ImageThumbnail;
            tokenType.TransactionHash = model.TransactionHash;

            //Attributes
            tokenType.Attributes.Clear();
            tokenType.Attributes.AddRange(model.Attributes.Select(VyTokenTypeSO._TokenAttribute.FromModel));

            //Update AnimationUrls
            tokenType.AnimationUrls.Clear();
            tokenType.AnimationUrls.AddRange(model.AnimationUrls.Select(VyItemSO._ItemTypeValue.FromModel));

            if (updateLiveModel)
                tokenType.UpdateLiveModel(model);

            tokenType.RefreshTokenTexture();

            SaveItem(tokenType, true);
            tokenType.NotifyItemUpdated();
        }
        #endregion

        #region List Utils

        public static void Revert(this VyItemSO item)
        {
            if (item.HasLiveModel)
            {
                if (item.IsContract)
                {
                    var model = JsonConvert.DeserializeObject<VyContract>(item.LiveModel);
                    item.AsContract().FromModel(model, false);
                }
                else if (item.IsTokenType)
                {
                    var model = JsonConvert.DeserializeObject<VyTokenType>(item.LiveModel);
                    item.AsTokenType().FromModel(model, false);
                }
            }
        }
        #endregion

        #region Item Utils
        public static void SaveItem(VyItemSO item, bool force = false)
        {
            if (force || EditorUtility.IsDirty(item))
            {
                var expectedName = $"{item.Id}_{item.Name}";

                //Check if name needs an update
                if (!item.name.Equals(expectedName))
                {
                    if (item.IsContract)
                    {
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(item), expectedName);
                    }
                    else if (item.IsTokenType)
                    {
                        //AssetDatabase.ClearLabels(item);
                        item.name = expectedName;

                        //bug fix - rename update
                        //var tempToken = CreateTokenType((item as VyTokenTypeSO).Contract);
                        //AssetDatabase.Refresh();
                        //RemoveTokenType(tempToken);

                        AssetDatabase.ClearLabels(item);
                        AssetDatabase.SetLabels(item, new []{ expectedName });
                        AssetDatabase.Refresh();
                    }
                }

                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssetIfDirty(item);
            }
        }
        #endregion

        #region Misc Utils
        public static T CreateSubAsset<T>(VyItemSO parent, string name = "", HideFlags hideFlags = HideFlags.None) where T : ScriptableObject
        {
            var newSO = ScriptableObject.CreateInstance<T>();
            newSO.name = name;
            newSO.hideFlags = hideFlags;

            AssetDatabase.AddObjectToAsset(newSO, parent);

            EditorUtility.SetDirty(parent);
            AssetDatabase.SaveAssetIfDirty(parent);

            return newSO;
        }

        public static void RemoveSubAsset<T>(VyItemSO parent, T target) where T : ScriptableObject
        {
            AssetDatabase.RemoveObjectFromAsset(target);

            var path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target));

            EditorUtility.SetDirty(parent);
            AssetDatabase.SaveAssetIfDirty(parent);
        }
        #endregion
    }
}
