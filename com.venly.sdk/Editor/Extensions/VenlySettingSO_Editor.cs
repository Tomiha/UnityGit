using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VenlySettingsSO))]
public class VenlySettingSO_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Keep this file inside a Resources folder at all times. Settings can only be changed through the SDK Manager.", MessageType.Info);
        GUILayout.Space(20);
        EditorGUI.BeginDisabledGroup(true);
        base.OnInspectorGUI();
        EditorGUI.EndDisabledGroup();
    }
}
