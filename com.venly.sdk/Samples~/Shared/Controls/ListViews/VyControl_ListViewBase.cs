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

    protected VyControl_ListViewItemBase(){}
    protected VyControl_ListViewItemBase(string itemUxml)
    {
        if (_itemPrototype == null)
            _itemPrototype = Resources.Load<VisualTreeAsset>(itemUxml);

        _itemPrototype.CloneTree(this);
    }

    public abstract void BindItem(T sourceItem);
    public abstract void BindMockItem();

    protected void SetLabel(string elementName, string txt)
    {
        this.Q<Label>(elementName).text = txt;
    }

    protected void SetLabel(string elementName, bool state)
    {
        this.Q<Label>(elementName).text = state?"YES":"NO";
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

        selectionType = isSelectable ? SelectionType.Single : SelectionType.None;
        showAlternatingRowBackgrounds =
            isSelectable ? AlternatingRowBackground.None : AlternatingRowBackground.ContentOnly;
    }

    public void SetItemSource(TItem[] itemSource)
    {
        SetItemSource(itemSource.ToList());
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
