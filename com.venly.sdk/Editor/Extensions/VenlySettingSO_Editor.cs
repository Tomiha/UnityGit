using UnityEditor;
using UnityEngine;
using Venly.Editor;

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
            VenlySettingsEd.Instance.ConfigureForBackend(so.BackendProvider);
            //var so = (VenlySettingsSO) serializedObject.targetObject;
            //so.ConfigureBackendProvider();
        }
    }
}
