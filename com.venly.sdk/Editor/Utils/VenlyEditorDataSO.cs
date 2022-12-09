using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;
using Venly.Models;

namespace Venly.Editor.Utils
{
    internal class VenlyEditorDataSO : ScriptableObject
    {
        [Serializable]
        public class SDKManagerData
        {
            public string UpdateURL;

            public bool UnappliedSettings = false;
            public string CurrentClientId = null;
            public eVyBackendProvider SelectedBackend;
            public List<string> AvailableAppIds = new();
        }

        [Header("Paths")] 
        public string SdkPackageRoot;
        public string PublicResourceRoot;

        [JsonIgnore][HideInInspector]public PackageInfo PackageInfo;

        [Header("SDK Manager")] 
        public SDKManagerData SDKManager = new ();
    }
}
