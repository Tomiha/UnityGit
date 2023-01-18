using System;
using UnityEngine.UIElements;

public class UIBindAttribute : Attribute
{
    public readonly string ElementName;
    public UIBindAttribute(string elementName)
    {
        ElementName = elementName;
    }
}

public static class SampleControl_Utils
{
    private static StyleEnum<DisplayStyle>  _styleVisible = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
    private static StyleEnum<DisplayStyle>  _styleHidden = new StyleEnum<DisplayStyle>(DisplayStyle.None);

    public static void ToggleVisibility(this VisualElement element, bool isVisible)
    {
        element.style.visibility = isVisible? Visibility.Visible : Visibility.Hidden;
    }

    public static void ToggleDisplay(this VisualElement element, bool isVisible)
    {
        element.style.display = isVisible ? _styleVisible : _styleHidden;
    }

    public static void SetDisplay(this VisualElement element, string elementName, bool isVisible)
    {
        var target = element.Q(elementName);
        target?.ToggleDisplay(isVisible);
    }

    public static void SetLabel(this VisualElement element, string elementName, string lblText)
    {
        var target = element.Q<Label>(elementName);
        if (target != null) target.text = lblText;
    }

    public static void SetButtonLabel(this VisualElement element, string buttonName, string lbl)
    {
        var target = element.Q<Button>(buttonName);
        if (target != null) target.text = lbl;
    }
}
