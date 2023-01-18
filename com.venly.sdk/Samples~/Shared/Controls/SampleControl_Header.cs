using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SampleControl_Header : VisualElement
{
    public string HeaderTitle { get; set; }
    public bool ShowNavigateBack { get; set; }
    public bool ShowNavigateHome { get; set; }
    public bool ShowRefresh { get; set; }

    public event Action OnNavigateBack;
    public event Action OnNavigateHome;
    public event Action OnRefresh;

    public Label _lblHeaderTitle;
    public VisualElement _btnNavigateBack;
    public VisualElement _btnNavigateHome;
    public VisualElement _btnRefresh;

    public new class UxmlFactory : UxmlFactory<SampleControl_Header, UxmlTraits>
    { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription _HeaderTitle = new() { name = "header-title", defaultValue = "untitled" };
        UxmlBoolAttributeDescription _ShowNavigateBack = new() { name = "show-navigate-back", defaultValue = true };
        UxmlBoolAttributeDescription _ShowNavigateHome = new() { name = "show-navigate-home", defaultValue = false };
        UxmlBoolAttributeDescription _ShowRefresh = new() { name = "show-refresh", defaultValue = false };

        //public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        //{
        //    get { yield break; }
        //}

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            var element = ve as SampleControl_Header;

            element.HeaderTitle = _HeaderTitle.GetValueFromBag(bag, cc);
            element.ShowNavigateBack = _ShowNavigateBack.GetValueFromBag(bag, cc);
            element.ShowNavigateHome = _ShowNavigateHome.GetValueFromBag(bag, cc);
            element.ShowRefresh = _ShowRefresh.GetValueFromBag(bag, cc);

            element.RefreshControl();
        }
    }

    public SampleControl_Header()
    {
        var tree = Resources.Load<VisualTreeAsset>("SampleControl_Header");
        tree.CloneTree(this);

        _lblHeaderTitle = this.Q<Label>("lbl-header-title");

        _btnNavigateBack = this.Q<VisualElement>("btn-back");
        _btnNavigateBack.AddManipulator(new Clickable(evt => OnNavigateBack?.Invoke()));

        _btnNavigateHome = this.Q<VisualElement>("btn-home");
        _btnNavigateHome.AddManipulator(new Clickable(evt => OnNavigateHome?.Invoke()));

        _btnRefresh = this.Q<VisualElement>("btn-refresh");
        _btnRefresh.AddManipulator(new Clickable(evt => OnRefresh?.Invoke()));
    }

    public void RefreshControl()
    {
        _lblHeaderTitle.text = HeaderTitle;
        _btnNavigateBack.ToggleDisplay(ShowNavigateBack);
        _btnNavigateHome.ToggleDisplay(ShowNavigateHome);
        _btnRefresh.ToggleDisplay(ShowRefresh);
    }
}
