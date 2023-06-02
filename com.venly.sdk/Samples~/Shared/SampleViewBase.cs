using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Core;

[RequireComponent(typeof(UIDocument))]
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
    protected bool IsSelectionMode { get; private set; }
    protected string ViewTitle = "untitled";

    private SampleControl_Header _viewHeader;
    private VyTaskNotifier<object> _selectionNotifier;

    protected SampleViewBase(T viewId)
    {
        ViewId = viewId;
    }

    protected void ToggleElement(string elementName, bool visible)
    {
        var element = GetElement<VisualElement>(elementName);
        element?.ToggleDisplay(visible);
    }

    protected void GetElement<T0>(out T0 element, string elementName = null) where T0 : VisualElement
    {
        element = GetElement<T0>(elementName);
    }

    protected T0 GetElement<T0>(string elementName = null) where T0 : VisualElement
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

    public void ClearBlackboardData(string key)
    {
        if (_blackboard.ContainsKey(key))
            _blackboard.Remove(key);
    }

    public object GetBlackboardDataRaw(string key)
    {
        return _blackboard[key];
    }

    public T0 GetBlackBoardData<T0>(string key) where T0 : class
    {
        return GetBlackboardDataRaw(key) as T0;
    }

    protected void BindButton_SwitchView(string elementName, T viewId)
    {
        BindButton(elementName, () =>
        {
            ViewManager.SwitchView(viewId);
        });
    }

    protected void BindButton(string elementName, Action targetFunction)
    {
        var button = GetElement<Button>(elementName);
        button.clickable.clicked += targetFunction;
    }

    protected void SetLabel(string elementName, string txt)
    {
        var el = GetElement<VisualElement>(elementName);
        if(el is Label label) label.text = txt;
        else if (el is LabelField labelField) labelField.UpdateText(txt);
        else if (el is Button btn) btn.text = txt;
        else throw new Exception($"Element \'{elementName}\' is not a Label or LabelField...");
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

        View.sortingOrder = IsSelectionMode ? 1000 : 0;

        _viewHeader = GetElement<SampleControl_Header>(null);
        _viewHeader.OnNavigateBack += OnClick_NavigateBackInternal;
        _viewHeader.OnNavigateHome += OnClick_NavigateHome;
        _viewHeader.OnRefresh += OnClick_Refresh;

        ViewTitle = ViewManager.GetTitle(ViewId);

        OnBindElements(View.rootVisualElement);
        OnActivate();

        //Update Header
        UpdateHeader(ViewTitle);
    }

    private void UpdateHeader(string viewTitle)
    {
        _viewHeader.HeaderTitle = viewTitle;
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

    public VyTask<object> SelectionMode(string newTitle = null)
    {
        _selectionNotifier = VyTask<object>.Create();
        IsSelectionMode = true;

        Activate();

        ShowNavigateBack = true;
        ShowNavigateHome = false;

        UpdateHeader(newTitle ?? ViewTitle);

        return _selectionNotifier.Task;
    }

    protected void FinishSelection(object result)
    {
        _selectionNotifier.NotifySuccess(result);
        OnClick_NavigateBackInternal();
    }

    protected void CancelSelection()
    {
        _selectionNotifier.NotifyCancel();
        OnClick_NavigateBackInternal();
    }

    private void OnClick_NavigateBackInternal()
    {
        if (IsSelectionMode)
        {
            if (_selectionNotifier.NotificationPending)
            {
                _selectionNotifier.NotifyCancel();
            }

            IsSelectionMode = false;
            Deactivate();
        }
        else OnClick_NavigateBack();
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
    protected virtual void OnDeactivate()
    {
    }
    #endregion
}
