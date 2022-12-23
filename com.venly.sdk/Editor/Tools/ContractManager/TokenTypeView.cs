using UnityEngine;
using UnityEngine.UIElements;
using VenlySDK.Data;
using VenlySDK.Editor.Tools.Controls;

namespace VenlySDK.Editor.Tools.ContractManager
{
    public class TokenTypeView : ItemViewBase
    {
        private TokenAttributeList _attributeList;
        private TypeValueList _animationUrlList;
        private VisualElement _containerTexture;

        private Label _headerTitle;
        private Label _headerSubTitle;

        #region Cstr
        public new class UxmlFactory : UxmlFactory<TokenTypeView, UxmlTraits> {}

        public TokenTypeView() : base("TokenTypeView")
        {
            _attributeList = this.Q<TokenAttributeList>("attributes");
            _animationUrlList = this.Q<TypeValueList>("animation-urls");
            _containerTexture = this.Q<VisualElement>("container-image");

            _headerSubTitle = this.Q<Label>("header-sub-title");
            _headerTitle = this.Q<Label>("header-title");
        }
        #endregion

        protected override void OnBindingRelease(VyItemSO item)
        {
            item.AsTokenType().OnTextureChanged -= onTokenTexture_Changed;
        }

        protected override void OnBindingUpdate(VyItemSO item)
        {
            item.AsTokenType().OnTextureChanged += onTokenTexture_Changed;

            _attributeList.BindItemSource(item.AsTokenType());
            _animationUrlList.BindItemSource(item);

            UpdateHeader();
            SetTokenTexture();
            item.AsTokenType().RefreshTokenTexture();
        }

        private void onTokenTexture_Changed(Texture2D texture)
        {
            SetTokenTexture();
        }

        private void UpdateHeader()
        {
            var tokenType = _item.AsTokenType();
            _headerTitle.text = tokenType.Name;

            if (!tokenType.Confirmed)
            {
                _headerSubTitle.text = "Token Type";
            }
            else
            {
                if (tokenType.Fungible) _headerSubTitle.text = "Fungible Token Type (FT)";
                else _headerSubTitle.text = "Non-Fungible Token Type (NFT)";
            }
        }

        private void SetTokenTexture()
        {
            var tokenType = _item.AsTokenType();
            var currTexture = style.backgroundImage.value.texture;

            if (tokenType.TokenTexture == null)
            {
                _containerTexture.style.backgroundImage = new StyleBackground();
                //Reset to default texture
            }
            else if(tokenType.TokenTexture != currTexture)
            {
                _containerTexture.style.backgroundImage = new StyleBackground(tokenType.TokenTexture);
            }
        }
    }
}