using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SampleViewBase<T> : MonoBehaviour where T : Enum
{
    private Dictionary<string, object> _blackboard = new ();

    public T ViewId;

    [HideInInspector] public SampleViewManager<T> ViewManager;
    [HideInInspector] public SampleViewBase<T> PreviousView { get; set; }
    protected UIDocument View;
    protected VisualElement ViewRoot => View?.rootVisualElement;

    protected bool ShowNavigateBack = false;
    protected bool ShowNavigateHome = false;
    protected bool ShowRefresh = false;
    protected string ViewTitle = "untitled";

    private SampleControl_Header _viewHeader;

    protected SampleViewBase(T viewId)
    {
        ViewId = viewId;
    }

    protected void ToggleElement(string elementName, bool visible)
    {
        var element = GetElement<VisualElement>(elementName);
        element?.ToggleDisplay(visible);
    }

    protected void GetElement<T0>(out T0 element, string elementName) where T0 : VisualElement
    {
        element = GetElement<T0>(elementName);
    }

    protected T0 GetElement<T0>(string elementName) where T0 : VisualElement
    {
        var element = ViewRoot.Q<T0>(elementName);
        if (element == null)
        {
            throw new Exception($"[SampleViewBase::GetElement] Element not found. ({typeof(T0).Name} > {elementName}) [View={ViewId}]");
        }

        return element;
    }

    public bool HasBlackboardData(string key)
    {
        return _blackboard.ContainsKey(key);
    }

    public void SetBlackboardData(string key, object value)
    {
        _blackboard[key] = value;
    }

    public object GetBlackboardDataRaw(string key)
    {
        return _blackboard[key];
    }

    public T0 GetBlackBoardData<T0>(string key) where T0 : class
    {
        return GetBlackboardDataRaw(key) as T0;
    }

    protected void BindButton(string elementName, Action targetFunction)
    {
        var button = GetElement<Button>(elementName);
        button.clickable.clicked += targetFunction;
    }
    protected void SetLabel(string elementName, string txt)
    {
        var label = GetElement<Label>(elementName);
        label.text = txt;
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        //Root Initialize
        View = GetComponent<UIDocument>();
        if (View == null)
        {
            Debug.LogWarning($"No UI Document found for View Controller ({ViewTitle})");
            return;
        }

        _viewHeader = GetElement<SampleControl_Header>(null);
        _viewHeader.OnNavigateBack += OnClick_NavigateBack;
        _viewHeader.OnNavigateHome += OnClick_NavigateHome;
        _viewHeader.OnRefresh += OnClick_Refresh;

        ViewTitle = ViewManager.GetTitle(ViewId);

        OnBindElements(View.rootVisualElement);
        OnActivate();

        //Update Header
        _viewHeader.HeaderTitle = ViewTitle;
        _viewHeader.ShowNavigateBack = ShowNavigateBack;
        _viewHeader.ShowNavigateHome = ShowNavigateHome;
        _viewHeader.ShowRefresh = ShowRefresh;
        _viewHeader.RefreshControl();
    }
    public void Deactivate()
    {
        OnDeactivate();
        gameObject.SetActive(false);
    }

    protected virtual void OnClick_NavigateBack()
    {
        ViewManager.NavigateBack();
    }

    protected virtual void OnClick_NavigateHome()
    {
        ViewManager.NavigateHome();
    }

    protected virtual void OnClick_Refresh()
    {

    }

    protected virtual void OnBindElements(VisualElement root)
    {


        //Dynamic
        var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        foreach (var info in fields)
        {
            var att = info.GetCustomAttribute<UIBindAttribute>();
            if (att == null) continue;

            var element = GetElement<VisualElement>(att.ElementName);
            info.SetValue(this, element);
        }
    }

    #region Interface
    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
    #endregion
}
