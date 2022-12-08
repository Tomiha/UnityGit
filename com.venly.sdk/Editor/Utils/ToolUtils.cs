using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Venly.Editor.Utils
{
    internal static class ToolUtils
    {
        private static readonly StyleEnum<DisplayStyle> _hiddenStyle = new(DisplayStyle.None);
        private static readonly StyleEnum<DisplayStyle> _visibleStyle = new(DisplayStyle.Flex);

        public static void HideElement(this VisualElement el){ el.style.display = _hiddenStyle;}
        public static void ShowElement(this VisualElement el) { el.style.display = _visibleStyle; }
        public static void ToggleElement(this VisualElement el, bool visible) { el.style.display = visible?_visibleStyle:_hiddenStyle; }

        public static VisualTreeAsset GetControlUXML(string uxmlName)
        {
            return Resources.Load<VisualTreeAsset>($"Controls/{uxmlName}");
        }

        public static VisualTreeAsset GetContractManagerUXML(string uxmlName)
        {
            return Resources.Load<VisualTreeAsset>($"ContractManager/{uxmlName}");
        }

        public static VisualTreeAsset GetSDKManagerUXML(string uxmlName)
        {
            return Resources.Load<VisualTreeAsset>($"SDKManager/{uxmlName}");

            //return GetUXML<VisualTreeAsset>("SDKManager", uxmlName);
        }

        //public static T GetUXML<T>(string identifier, string uxmlName) where T : UnityEngine.Object
        //{
        //    var path = $"{VenlySettingsEd.Instance.SdkRootPath}Editor/Tools/{identifier}/UIDocuments/{uxmlName}.uxml";
        //    return AssetDatabase.LoadAssetAtPath<T>(path);
        //}

        //public static string ToFullPath(string relPath)
        //{
        //    var firstSlash = relPath.IndexOf("\\");
        //    return $"{Application.dataPath}{relPath.Substring(firstSlash)}";
        //}

        public static string FindAssetFolder(string folderName)
        {
            var foundDirectories = Directory.EnumerateDirectories(Application.dataPath, folderName, SearchOption.AllDirectories);

            var directories = foundDirectories as string[] ?? foundDirectories.ToArray();
            if (directories.Any())
            {
                var fullPath = directories.First();
                var assetsIndex = fullPath.IndexOf("Assets");
                return fullPath.Substring(assetsIndex) + "\\";
            }

            return null;
        }
    }
}