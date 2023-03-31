using System.Linq;
using Packages.com.venly.sdk.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEngine;

namespace VenlySDK.Editor.Utils
{
    //internal class VenlySDKUpdater
    //{
    //    public static VenlySDKUpdater Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //            {
    //                _instance = new VenlySDKUpdater();
    //            }

    //            return _instance;
    //        }
    //    }

    //    private static VenlySDKUpdater _instance;

    //    //[InitializeOnLoadMethod]
    //    //private static void InitializeStatic()
    //    //{
    //    //    Instance.HandlePostUpdate();
    //    //}

    //    ~VenlySDKUpdater()
    //    {
    //        VenlyDebugEd.LogDebug("SDK Updater DESTROYED");
    //    }

    //    public VenlySDKUpdater()
    //    {
    //        VenlyDebugEd.LogDebug("SDK Updater CREATED");

    //        //Subscribe to PackageManager Events
    //        Events.registeringPackages += PackageManager_registeringPackages;
    //    }

    //    private void PackageManager_registeringPackages(PackageRegistrationEventArgs eventArgs)
    //    {
    //        bool sdkChanged = eventArgs.added.Any(p => p.name.Equals("com.venly.sdk")) ||
    //                          eventArgs.changedFrom.Any(p => p.name.Equals("com.venly.sdk")) ||
    //                          eventArgs.changedTo.Any(p => p.name.Equals("com.venly.sdk")) ||
    //                          eventArgs.removed.Any(p => p.name.Equals("com.venly.sdk"));

    //        if(sdkChanged)
    //            HandlePostUpdate();
    //    }

    //    private void HandlePostUpdate()
    //    {
    //        if (!EditorPrefs.HasKey("com.venly.sdk.update")) return;

    //        VenlyDebugEd.LogDebug("Should Refresh Settings - SDK was updated!");

    //        VenlyDebugEd.LogDebug($"Updating From: {EditorPrefs.GetString("com.venly.sdk.prevVersion")}");
    //        VenlyDebugEd.LogDebug($"Updating From: {EditorPrefs.GetString("com.venly.sdk.update")}");

    //        //Clear Keys
    //        EditorPrefs.DeleteKey("com.venly.sdk.update");
    //        EditorPrefs.DeleteKey("com.venly.sdk.prevVersion");
    //    }

    //    public void UpdateSDK(string newVersion)
    //    {
    //        EditorPrefs.SetString("com.venly.sdk.update", newVersion);
    //        EditorPrefs.SetString("com.venly.sdk.prevVersion", VyEditorData.EditorSettings.Version);
    //    }
    //}
}
