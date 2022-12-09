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
            public string GitReleaseURL;
            public string GitSdkURL;

            public bool UnappliedSettings = false;
            public string CurrentClientId = null;
            public eVyBackendProvider SelectedBackend;
            public List<string> AvailableAppIds = new();
        }

        [Header("General")] 
        public string SdkPackageRoot;
        public string PublicResourceRoot;
        public string Version;

        [JsonIgnore][HideInInspector]public PackageInfo PackageInfo;

        [Header("SDK Manager")] 
        public SDKManagerData SDKManager = new ();
    }
}
