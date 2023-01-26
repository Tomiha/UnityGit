using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LabelField : VisualElement
{
    public string Label { get; set; }
    public string Text { get; set; }
    public bool BoldLabel { get; set; }

    public Label _labelElement;
    public Label _textElement;

    public new class UxmlFactory : UxmlFactory<LabelField, UxmlTraits>
    { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription _label = new() { name = "label", defaultValue = "" };
        UxmlStringAttributeDescription _text = new() { name = "text", defaultValue = "" };
        private UxmlBoolAttributeDescription _boldLabel = new() {name = "bold-label", defaultValue = true};

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            var element = ve as LabelField;

            element?.UpdateLabel(_label.GetValueFromBag(bag, cc), _boldLabel.GetValueFromBag(bag,cc));
            element?.UpdateText(_text.GetValueFromBag(bag, cc));
        }
    }

    public LabelField()
    {
        var tree = Resources.Load<VisualTreeAsset>("LabelField");
        tree.CloneTree(this);

        _labelElement = this.Q<Label>("lbl-label");
        _textElement = this.Q<Label>("lbl-text");
    }

    public void UpdateLabel(string lbl, bool bold = true)
    {
        Label = lbl;
        BoldLabel = bold;

        _labelElement.text = lbl;

        var fontStyle = _labelElement.style.unityFontStyleAndWeight;
        fontStyle.value = BoldLabel ? FontStyle.Bold : FontStyle.Normal;
        _labelElement.style.unityFontStyleAndWeight = fontStyle;
    }

    public void UpdateText(string txt)
    {
        Text = txt;
        _textElement.text = txt;
    }
}
