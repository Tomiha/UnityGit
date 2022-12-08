using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Venly.Models;

namespace Venly.Editor.Utils
{
    internal class VenlyEditorDataSO : ScriptableObject
    {
        [Header("Paths")]
        public string SdkRootPath;
        public string SdkResourcesPath;
        public string SdkEditorRootPath;
        public string SdkEditorResourcesPath;

        [Header("SDK Settings")] 
        public bool UnappliedSettings = false;
        public string CurrentClientId = null;
        public List<string> AvailableAppIds = new();
        public eVyBackendProvider SelectedBackend;
    }
}
