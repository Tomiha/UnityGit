using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VenlySDK.Editor.Utils
{
    internal static class VenlyEditorUtils
    {
        #region Extensions
        private static readonly StyleEnum<DisplayStyle> _hiddenStyle = new(DisplayStyle.None);
        private static readonly StyleEnum<DisplayStyle> _visibleStyle = new(DisplayStyle.Flex);

        public static void HideElement(this VisualElement el) { el.style.display = _hiddenStyle; }
        public static void ShowElement(this VisualElement el) { el.style.display = _visibleStyle; }
        public static void ToggleElement(this VisualElement el, bool visible) { el.style.display = visible ? _visibleStyle : _hiddenStyle; }
        #endregion

        #region UI Helpers
        public static VisualTreeAsset GetUXML_Controls(string uxmlName)
        {
            return Resources.Load<VisualTreeAsset>($"Controls/{uxmlName}");
        }

        public static VisualTreeAsset GetUXML_ContractManager(string uxmlName)
        {
            return Resources.Load<VisualTreeAsset>($"ContractManager/{uxmlName}");
        }

        public static VisualTreeAsset GetUXML_SDKManager(string uxmlName)
        {
            return Resources.Load<VisualTreeAsset>($"SDKManager/{uxmlName}");
        }
        #endregion

        #region DATA Handling
        public static void StoreBackup<T>(T dataSO) where T : ScriptableObject
        {
            VenlyDebugEd.LogDebug($"VenlyEditorSettings Store Backup called {typeof(T).Name}");
            var soType = typeof(T).Name.ToLower();
            var dataJson = JsonConvert.SerializeObject(dataSO);
            EditorPrefs.SetString($"com.venly.sdk.{soType}", dataJson);
        }
        
        public static void RestoreBackup<T>(T dataSo, bool removeAfterRestore = true) where T : ScriptableObject
        {
            VenlyDebugEd.LogDebug($"VenlyEditorSettings Restore Backup called {typeof(T).Name}");
            var soType = typeof(T).Name.ToLower();
            if (!EditorPrefs.HasKey($"com.venly.sdk.{soType}")) return;

            var jsonString = EditorPrefs.GetString($"com.venly.sdk.{soType}");
            var jsonData = JObject.Parse(jsonString);

            var serialializedData = new SerializedObject(dataSo);
            var soProperty = serialializedData.GetIterator();
            VenlyDebugEd.LogDebug("VenlyEditorSettings Restore Backup found");
            do
            {
                UpdateProperty(soProperty, jsonData);

            } while (soProperty.NextVisible(true));

            if(removeAfterRestore)
                EditorPrefs.DeleteKey($"com.venly.sdk.{soType}");

            serialializedData.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(dataSo);
        }
        
        private static void UpdateProperty(SerializedProperty property, JObject json)
        {
            if (!IsSupportedPropertyType(property.propertyType)) return;
            if (string.IsNullOrEmpty(property.propertyPath)) return;

            var pathSplit = property.propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (!json.ContainsKey(pathSplit[0])) return;

            var token = json[pathSplit[0]];
            for (var i = 1; i < pathSplit.Length; i++)
            {
                if (token is JArray) return;

                token = token[pathSplit[i]];
                if (token == null) return;
            }

            SetPropertyValue(property, token);
        }
        
        private static void UpdatePropertyArray(SerializedProperty property, JArray array)
        {
            property.ClearArray();
            foreach (var jToken in array)
            {
                property.InsertArrayElementAtIndex(property.arraySize);
                var arrayProperty = property.GetArrayElementAtIndex(property.arraySize - 1);

                do
                {
                    if(IsSupportedPropertyType(arrayProperty.propertyType))
                        SetPropertyValue(arrayProperty, jToken);

                } while (arrayProperty.NextVisible(true));
            }
        }

        private static bool IsSupportedPropertyType(SerializedPropertyType t)
        {
            return t is SerializedPropertyType.Boolean or SerializedPropertyType.String or SerializedPropertyType.Integer or SerializedPropertyType.Float or SerializedPropertyType.Enum or SerializedPropertyType.Generic;
        }

        private static void SetPropertyValue(SerializedProperty property, JToken token)
        {
            try
            {
                if (token is JArray array)
                    UpdatePropertyArray(property, array);

                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        property.boolValue = token.Value<bool>();
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = token.Value<string>();
                        break;
                    case SerializedPropertyType.Integer:
                        property.intValue = token.Value<int>();
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = token.Value<float>();
                        break;
                    case SerializedPropertyType.Enum:
                        property.enumValueIndex = token.Value<int>();
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        #endregion

        #region Helpers
        public static Version ParseSemVer(string version)
        {
            version = version.Replace("v", "");
            version = version.Split('-')[0];
            return Version.Parse(version);
        }

        public static string GetLatestSemVer(List<string> versions)
        {
            if (versions == null) return null;

            var tempDict = new Dictionary<Version, string>();
            versions.ForEach(v => tempDict.Add(ParseSemVer(v), v));

            return tempDict.OrderBy(kvp => kvp.Key).Last().Value;
        }
        #endregion
    }
}
