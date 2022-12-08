using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;
using Venly.Models;

namespace Venly.Editor.Utils
{
    internal class VenlyEditorDataSO : ScriptableObject
    {
        [Header("Paths")] 
        public string SdkPackageRoot;
        public string ManagerPackageRoot;
        public string PublicResourceRoot;

        [Header("SDK Settings")] 
        public PackageInfo PackageInfo;
        public bool UnappliedSettings = false;
        public string CurrentClientId = null;
        public List<string> AvailableAppIds = new();
        public eVyBackendProvider SelectedBackend;
    }
}
