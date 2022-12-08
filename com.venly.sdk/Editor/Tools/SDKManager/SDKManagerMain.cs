using UnityEngine;
using UnityEngine.UIElements;
using Venly.Editor.Utils;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManagerMain : VisualElement
    {
        public SDKManagerMain()
        {
            ToolUtils.GetSDKManagerUXML("SDKManagerMain").CloneTree(this);
        }
    }
}