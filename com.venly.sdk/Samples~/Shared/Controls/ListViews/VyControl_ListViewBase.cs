using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

//**************
//ITEM VIEW BASE
//**************
public abstract class VyControl_ListViewItemBase<T> : VisualElement
{
    private static VisualTreeAsset _itemPrototype;

    protected VyControl_ListViewItemBase()
    {
        BuildItemPrototype();
    }

    protected VyControl_ListViewItemBase(string itemUxml)
    {
        if (_itemPrototype == null)
            _itemPrototype = Resources.Load<VisualTreeAsset>(itemUxml);

        _itemPrototype.CloneTree(this);
    }

    private void BuildItemPrototype()
    {
        var root = new VisualElement
        {
            style =
            {
                flexGrow = new StyleFloat(1)
            }
        };
        Add(root);

        var subRoot = new VisualElement()
        {
            style =
            {
                flexGrow = new StyleFloat(1)
            }
        };
        root.Add(subRoot);
        subRoot.AddToClassList("list-view__item");

        GenerateTree(subRoot);
    }

    protected void AddLabelWithButton(VisualElement root, string labelName, string labelText, string buttonText,
        Action onButtonClick)
    {
        var main = new VisualElement
        {
            style =
            {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
            }
        };

        AddLabel(main, labelName, labelText);

        var buttonRoot = new VisualElement
        {
            style =
            {
                justifyContent = new StyleEnum<Justify>(Justify.Center),
                marginBottom = new StyleLength(3),
                flexShrink = new StyleFloat(0.0f)
            }
        };

        var btn = new Button(onButtonClick);
        btn.AddToClassList("button-select");
        btn.text = buttonText;

        buttonRoot.Add(btn);
        main.Add(buttonRoot);
        root.Add(main);
    }

    protected VisualElement AddLabel(VisualElement root, string elementName, string labelText)
    {
        VisualElement el = null;
        if (string.IsNullOrEmpty(labelText))
        {
            el = new Label(labelText);
            el.name = elementName;
            el.style.alignSelf = new StyleEnum<Align>(Align.Center);
        }
        else
        {
            var txtField = new TextField(labelText)
            {
                multiline = true,
                isReadOnly = true,
                name = elementName
            };
            el = txtField;
        }

        el.AddToClassList("texfield-readonly");
        el.AddToClassList("list-item");

        root.Add(el);

        return el;
    }

    public virtual void GenerateTree(VisualElement root){}
    public abstract void BindItem(T sourceItem);
    public abstract void BindMockItem();

    protected void ToggleElement(string elementName, bool visible)
    {
        this.Q<VisualElement>(elementName).ToggleDisplay(visible);
    }

    protected void SetLabel(string elementName, string txt)
    {
        SetLabelText(elementName, txt);
    }

    protected void SetLabel(string elementName, object txt)
    {
        SetLabelText(elementName, txt.ToString());
    }

    protected void SetLabel(string elementName, bool state)
    {
        SetLabelText(elementName, state ? "YES" : "NO");
    }

    public void SetLabelName(string elementName, string name)
    {
        var el = this.Q<VisualElement>(elementName);
        if (el == null) throw new ArgumentException($"element \'{elementName}\' not found");

        if (el is Label lbl) lbl.text = name;
        else if (el is TextField txtField) txtField.label = name;
    }

    private void SetLabelText(string elementname, string txt)
    {
        var el = this.Q<VisualElement>(elementname);
        if (el == null) throw new ArgumentException($"element \'{elementname}\' not found");

        if (el is Label lbl) lbl.text = txt;
        else if (el is LabelField lblfield) lblfield.UpdateText(txt);
        else if (el is TextField txtField) txtField.value = txt;
    }
}

//**************
//LIST VIEW BASE
//**************
public class VyControl_ListViewBase<TItem, TItemView> : ListView where TItemView : VisualElement
{
    private List<TItem> _itemSource;
    private List<TItem> _mockItemSource;

    public event Action<TItem> OnItemSelected;

    public bool GenerateMockItems { get; set; }
    public int MockItemCount { get; set; }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlBoolAttributeDescription _GenerateMockItems = new() { name = "generate-mock-items", defaultValue = true };
        UxmlIntAttributeDescription _MockItemCount = new() { name = "mock-item-count", defaultValue = 3 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            var element = ve as VyControl_ListViewBase<TItem, TItemView>;

            element.GenerateMockItems = _GenerateMockItems.GetValueFromBag(bag, cc);
            element.MockItemCount = _MockItemCount.GetValueFromBag(bag, cc);

            element.RefreshBindings();
        }
    }

    public VyControl_ListViewBase(bool isSelectable = true)
    {
        //This
        style.flexGrow = new StyleFloat(1);
        style.flexShrink = new StyleFloat(1);
        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        if (isSelectable)
        {
            onItemsChosen += (itemList) => { OnItemSelected?.Invoke((TItem) itemList.First()); };
        }

        //focusable = isSelectable;
        selectionType = isSelectable ? SelectionType.Single : SelectionType.None;
        //selectionType = SelectionType.Single;
        showAlternatingRowBackgrounds = AlternatingRowBackground.None;// isSelectable ? AlternatingRowBackground.None : AlternatingRowBackground.ContentOnly;
    }

    public void SetItemSource(TItem[] itemSource)
    {
        SetItemSource(itemSource?.ToList());
    }

    public void SetItemSource(List<TItem> itemSource)
    {
        GenerateMockItems = false;

        _itemSource = itemSource;
        RefreshBindings();
    }

    public void RefreshBindings()
    {
#if !UNITY_EDITOR
        GenerateMockItems = false;
#endif

        makeItem = () => Activator.CreateInstance(typeof(TItemView)) as VisualElement;
        bindItem = (e, i) =>
        {
            if (e is VyControl_ListViewItemBase<TItem> itemBase)
            {
                if (GenerateMockItems)
                {
                    itemBase.BindMockItem();
                }
                else
                {
                    var itemData = _itemSource[i];
                    itemBase.BindItem(itemData);
                }
            }
            else
                throw new ArgumentException(
                    $"ItemProtoType is not of Type VyControl_ListViewItemBase<{typeof(TItem).Name}>");
        };

        if (GenerateMockItems)
        {
            _mockItemSource ??= new List<TItem>();
            if (_mockItemSource.Count != MockItemCount)
            {
                _mockItemSource.Clear();
                for(var i = 0; i < MockItemCount; ++i) _mockItemSource.Add(default);
            }

            itemsSource = _mockItemSource;
        }
        else itemsSource = _itemSource;
        
        RefreshItems();
    }
}
