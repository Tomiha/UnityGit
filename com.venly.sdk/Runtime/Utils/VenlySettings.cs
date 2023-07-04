using UnityEditor;
using UnityEngine;
using Venly.Models.Shared;
using Venly.Utils;

public class VenlySettings
{
    private static VenlySettingsSO _settingsSO;

    internal static VenlySettingsSO Settings
    {
        get
        {
            if (_settingsSO == null)
                RetrieveSettings();

            return _settingsSO;
        }
    }

#if UNITY_EDITOR
    public static bool HasCredentials => !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret);
    public static string ClientId => Settings.ClientId;
    public static string ClientSecret => Settings.ClientSecret;
    public static bool PrintRemoteApiInfo => Settings.PrintRemoteApiInfo;
    public static string PublicResourceRoot => Settings.PublicResourceRoot;

    public static bool HasNftApiAccess => Settings.HasNftApiAccess;
    public static bool HasWalletApiAccess => Settings.HasWalletApiAccess;
    public static bool HasMarketApiAccess => Settings.HasMarketApiAccess;


    public static void SetCredentials(string clientId, string clientSecret)
    {
        Settings.ClientId = clientId;
        Settings.ClientSecret = clientSecret;
        AssetDatabase.SaveAssetIfDirty(Settings);
    }

    public static void SetAccessAndEnvironment(VyJwtInfo jwtInfo)
    {
        Settings.HasWalletApiAccess = jwtInfo.HasWalletAccess;
        Settings.HasNftApiAccess = jwtInfo.HasNftAccess;
        Settings.HasMarketApiAccess = jwtInfo.HasWalletAccess;
        Settings.Environment = jwtInfo.Environment;
    }

    public static void Load(VenlySettingsSO targetSO = null)
    {
        if (_settingsSO != null)
            return;

        if (targetSO != null)
        {
            _settingsSO = targetSO;
            return;
        }

        if(_settingsSO == null)
            RetrieveSettings();
    } //Force Load :p
#endif

    public static eVyEnvironment Environment => Settings.Environment;
    public static eVyBackendProvider BackendProvider => Settings.BackendProvider;

    public static VenlySettingsSO.BackendSettings_PlayFab PlayFabBackendSettings => Settings.PlayFabBackendSettings;
    public static VenlySettingsSO.BackendSettings_Custom CustomBackendSettings => Settings.CustomBackendSettings;
    public static VenlySettingsSO.BackendSettings_Beamable BeamableBackendSettings => Settings.BeamableBackendSettings;

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