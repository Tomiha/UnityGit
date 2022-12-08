using System.IO;
using System.Linq;
using UnityEngine;
using Venly.Models;

public class VenlySettings
{
    private static VenlySettingsSO _settingsSO;
    internal static VenlySettingsSO Settings
    {
        get
        {
            if(_settingsSO == null) 
                RetrieveSettings();

            return _settingsSO;
        }
    }

#if UNITY_EDITOR
    public static string ClientId => Settings.ClientId;
    public static string ClientSecret => Settings.ClientSecret;
    public static string SdkPackageRoot => Settings.SdkPackageRoot;
    public static string PublicResourceRoot => Settings.PublicResourceRoot;
#endif

    public static string ApplicationId => Settings.ApplicationId;
    public static eVyEnvironment Environment => Settings.Environment;
    public static eVyBackendProvider BackendProvider => Settings.BackendProvider;

    public static VenlySettingsSO.BackendSettings_PlayFab PlayFabBackendSettings => Settings.PlayFabBackendSettings;
    public static VenlySettingsSO.BackendSettings_Custom CustomBackendSettings => Settings.CustomBackendSettings;

    private static void RetrieveSettings()
    {
        var settingsList = Resources.LoadAll<VenlySettingsSO>("");
        if (settingsList.Length != 1)
        {
            Debug.LogWarning("The number of VenlySettings objects should be 1: " + settingsList.Length);
            _settingsSO = null;

            //todo: remove or create the file??
            return;
        }

        _settingsSO = settingsList[0];
    }
}
