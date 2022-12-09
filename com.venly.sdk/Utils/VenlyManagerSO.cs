using System;
using UnityEngine;

public class VenlyManagerSO : ScriptableObject
{
    public bool IsSdkInstalled;
    public string SdkVersionStr;
    public Version SdkVersion;
    public string ManagerPackageRoot;
}
