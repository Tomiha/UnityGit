using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Venly.Editor.Utils
{
    internal class VenlySDKUpdater
    {
        public static VenlySDKUpdater Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VenlySDKUpdater();
                }

                return _instance;
            }
        }

        private static VenlySDKUpdater _instance;

        [InitializeOnLoadMethod]
        private static void InitializeStatic()
        {
            Instance.HandlePostUpdate();
        }

        public VenlySDKUpdater()
        {
            Debug.Log("SDK Editor Created");

            //Subscribe to PackageManager Events
            Events.registeringPackages += PackageManager_registeringPackages;
        }

        private void PackageManager_registeringPackages(PackageRegistrationEventArgs eventArgs)
        {
            bool sdkChanged = eventArgs.added.Any(p => p.name.Equals("com.venly.sdk")) ||
                              eventArgs.changedFrom.Any(p => p.name.Equals("com.venly.sdk")) ||
                              eventArgs.changedTo.Any(p => p.name.Equals("com.venly.sdk")) ||
                              eventArgs.removed.Any(p => p.name.Equals("com.venly.sdk"));

            if(sdkChanged)
                HandlePostUpdate();
        }

        private void HandlePostUpdate()
        {
            if (!EditorPrefs.HasKey("com.venly.sdk.update")) return;

            Debug.Log("Should Refresh Settings - SDK was updated!");

            Debug.Log($"Updating From: {EditorPrefs.HasKey("com.venly.sdk.prevVersion")}");
            Debug.Log($"Updating From: {EditorPrefs.HasKey("com.venly.sdk.update")}");

            //Clear Keys
            EditorPrefs.DeleteKey("com.venly.sdk.update");
            EditorPrefs.DeleteKey("com.venly.sdk.prevVersion");
        }

        public void UpdateSDK(string newVersion)
        {
            EditorPrefs.SetString("com.venly.sdk.update", newVersion);
            EditorPrefs.SetString("com.venly.sdk.prevVersion", VenlySettingsEd.Instance.EditorData.Version);
        }
    }
}
