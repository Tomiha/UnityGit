using System;
using UnityEngine;
using Venly.Models;

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

    [HideInInspector]
    public string SdkResourcePath;
#endif

    public string ApplicationId;
    public eVyEnvironment Environment;
    public eVyBackendProvider BackendProvider;

    //Backend Settings
    public BackendSettings_PlayFab PlayFabBackendSettings = new ();
    public BackendSettings_Custom CustomBackendSettings = new ();


    public void ConfigureBackendProvider()
    {
        Debug.Log("Todo");
        //VenlySettings.ConfigureForBackend();
    }
}
