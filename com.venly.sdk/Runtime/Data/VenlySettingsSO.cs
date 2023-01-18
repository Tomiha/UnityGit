using System;
using UnityEngine;
using VenlySDK.Models;

[Serializable]
public class VenlySettingsSO : ScriptableObject
{
    #region BackendSettings

    [Serializable]
    public class BackendSettings_PlayFab
    {
        public string VenlyAzureFunction;
    }

    [Serializable]
    public class BackendSettings_Custom
    {
        public int CustomId;
    }

    #endregion

#if UNITY_EDITOR
    public string ClientId;
    public string ClientSecret;

    public bool HasNftApiAccess;
    public bool HasWalletApiAccess;
    public bool HasMarketApiAccess;

    [HideInInspector] public string SdkPackageRoot;

    [HideInInspector] public string PublicResourceRoot;
#endif

    public eVyEnvironment Environment;
    public eVyBackendProvider BackendProvider;

    //Backend Settings
    public BackendSettings_PlayFab PlayFabBackendSettings = new();
    public BackendSettings_Custom CustomBackendSettings = new();


    public void ConfigureBackendProvider()
    {
        Debug.Log("Todo");
        //VenlySettings.ConfigureForBackend();
    }
}