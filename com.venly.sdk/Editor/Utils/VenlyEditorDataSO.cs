using System;
using UnityEngine;
using Venly.Models.Shared;

namespace Venly.Editor.Utils
{
    internal class VenlyEditorDataSO : ScriptableObject
    {
#if UNITY_EDITOR
        [Serializable]
        public class SDKManagerData
        {
            public bool UnappliedSettings = false;
            public string CurrentClientId = null;
            public eVyBackendProvider SelectedBackend;
        }

        [Header("General")]
        public string PublicResourceRoot;
        public string Version;

        [Header("ChainData")]
        public eVyChain[] SupportedChainsWallet;
        public eVyChain[] SupportedChainsNft;

        [Header("SDK Manager")] 
        public SDKManagerData SDKManager = new ();
#endif
    }
}
