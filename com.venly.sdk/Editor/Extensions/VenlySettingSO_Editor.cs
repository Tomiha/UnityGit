using UnityEditor;
using UnityEngine;
using Venly.Editor;
using Venly.Editor.Tools.SDKManager;

[CustomEditor(typeof(VenlySettingsSO))]
public class VenlySettingSO_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);
        //todo: check current config first, otherwise disable
        if (GUILayout.Button("Configure For Backend"))
        {
            var so = (VenlySettingsSO)serializedObject.targetObject;
            SDKManager.Instance.ConfigureForBackend(so.BackendProvider);
        }
    }
}
