﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEngine;
using VenlySDK.Models;

namespace VenlySDK.Editor.Utils
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
        public string SdkPackageRoot;
        public string PublicResourceRoot;
        public string Version;

        [JsonIgnore] public PackageInfo PackageInfo { get; set; }

        [Header("ChainData")]
        public eVyChain[] SupportedChainsWallet;
        public eVyChain[] SupportedChainsNft;

        [Header("SDK Manager")] 
        public SDKManagerData SDKManager = new ();
#endif
    }
}
