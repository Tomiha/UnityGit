using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Core;
using Venly.Utils;

[RequireComponent(typeof(UIDocument))]
public abstract class SampleViewBase<T> : MonoBehaviour where T : Enum
{
    private Dictionary<string, object> _blackboard = new ();

    public T ViewId;

    [HideInInspector] public SampleViewManager<T> ViewManager;
    [HideInInspector] public BackTargetArgs<T> BackTargetArgs { get; set; }
    [HideInInspector] public SampleViewBase<T> CurrentBackTarget => BackTargetArgs?.Target ?? null;

    protected UIDocument View;
    protected VisualElement ViewRoot => View?.rootVisualElement;

    protected bool ShowNavigateBack = false;
    protected bool ShowNavigateHome = false;
    protected bool ShowRefresh = false;
    protected bool NoDataRefresh = false;
    protected bool IsSelectionMode { get; private set; }
    protected string ViewTitle = "untitled";

    private SampleControl_Header _viewHeader;
    private VyTaskNotifier<object> _selectionNotifier;

    protected SampleViewBase(T viewId)
    {
        ViewId = viewId;
    }

    protected void ToggleElement(string elementName, bool visible, bool keepSpace = false)
    {
        var element = GetElement<VisualElement>(elementName);
        if(keepSpace) element?.ToggleVisibility(visible);
        else element?.ToggleDisplay(visible);
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

    public bool TryGetBlackboardDataRaw(out object result, string localKey = null, string globalKey = null)
    {
        result = null;

        if (!string.IsNullOrEmpty(globalKey))
        {
            if (ViewManager.HasGlobalBlackboardData(globalKey))
            {
                result = ViewManager.GetGlobalBlackboardDataRaw(globalKey);
                return true;
            }
        }

        if (!string.IsNullOrEmpty(localKey))
        {
            if (HasBlackboardData(localKey))
            {
                result = GetBlackboardDataRaw(localKey);
                return true;
            }
        }

        return false;
    }

    public bool TryGetBlackboardData<T0>(out T0 result, string localKey = null, string globalKey = null) where T0 : class
    {
        if (TryGetBlackboardDataRaw(out object obj, localKey, globalKey))
        {
            result = (T0) obj;
            return true;
        }

        result = null;
        return false;
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

    public void ClearBlackboardData()
    {
        _blackboard.Clear();
    }

    public void OverrideBlackboard(Dictionary<string, object> newBlackboard)
    {
        _blackboard.Clear();
        _blackboard = newBlackboard;
    }

    public object GetBlackboardDataRaw(string key)
    {
        return _blackboard[key];
    }

    public T0 GetBlackBoardData<T0>(string key) where T0 : class
    {
        var obj = GetBlackboardDataRaw(key);
        if (obj is T0 val)
        {
            return val;
        }
        
        VenlyLog.Exception($"[GetBlackBoardData] Stored object \'{key}\' is not of type \'{typeof(T0).Name}\' (type=\'{obj.GetType().Name}\')");
        return null;
    }

    protected void BindButton_SwitchView(string elementName, T viewId, bool clearData = false)
    {
        BindButton(elementName, () =>
        {
            if(clearData)
                ViewManager.ClearViewBlackboardData(viewId);
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
        else if (el is TextField txtbx) txtbx.value = txt;
        else throw new Exception($"Element \'{elementName}\' is not a Label or LabelField...");
    }

    protected void SetLabel(string elementName, object txt)
    {
       SetLabel(elementName, txt.ToString());
    }

    protected void SetToggleValue(string elementName, bool value)
    {
        var el = GetElement<VisualElement>(elementName);
        if (el is Toggle tgl)
        {
            tgl.value = value;
            return;
        }

        throw new Exception($"Element \'{elementName}\' is not a Toggle...");
    }

    protected bool GetToggleValue(string elementName)
    {
        var el = GetElement<VisualElement>(elementName);
        if (el is Toggle tgl) return tgl.value;

        throw new Exception($"Element \'{elementName}\' is not a Toggle...");
    }

    protected string GetValue(string elementName)
    {
        var el = GetElement<VisualElement>(elementName);
        if (el is Label lbl) return lbl.text;
        if (el is TextField txt) return txt.value;

        throw new Exception($"Element \'{elementName}\' is not a Label or TextField...");
    }

    protected bool TryGetValue<TVal>(string elementName, out TVal value, bool throwOnFail = false)
    {
        var strVal = GetValue(elementName);

        try
        {
            value = (TVal)Convert.ChangeType(strVal, typeof(TVal));
            return true;
        }
        catch(Exception ex)
        {
            if (throwOnFail) throw ex;
        }

        value = default;
        return false;
    }

    protected TVal GetValue<TVal>(string elementName, bool throwException = true)
    {
        var strVal = GetValue(elementName);

        try
        {
            return (TVal) Convert.ChangeType(strVal, typeof(TVal));
        }
        catch (Exception ex)
        {
            ViewManager.Exception.Show(new Exception($"Failed to cast \'{strVal}\' (element=\'{elementName}\' into \'{typeof(TVal).Name}\'", ex));
        }

        return default;
    }

    public async void Refresh(bool refreshData = true)
    {
        if(refreshData) await OnRefreshData();
        OnRefreshUI();
    }

    public void Activate(SwitchArgs<T> args)
    {
        gameObject.SetActive(true);

        //Root Initialize
        View = GetComponent<UIDocument>();
        if (View == null)
        {
            Debug.LogWarning($"No UI Document found for View Controller ({ViewTitle})");
            return;
        }

        //Reset Values
        ShowRefresh = true;
        ShowNavigateHome = true;
        ShowNavigateBack = true;
        NoDataRefresh = false;

        View.sortingOrder = IsSelectionMode ? 1000 : 0;

        _viewHeader = GetElement<SampleControl_Header>();
        _viewHeader.OnNavigateBack += OnClick_NavigateBackInternal;
        _viewHeader.OnNavigateHome += OnClick_NavigateHome;
        _viewHeader.OnRefresh += OnClick_RefreshInternal;

        ViewTitle = ViewManager.GetTitle(ViewId);

        OnBindElements(View.rootVisualElement);
        OnActivate();

        Refresh(!(args.SkipRefreshData || NoDataRefresh));

        if (BackTargetArgs == null || BackTargetArgs.Target == null)
            ShowNavigateBack = false;

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

        Activate(SwitchArgs<T>.Default);

        ShowNavigateBack = true;
        ShowNavigateHome = false;

        UpdateHeader(newTitle ?? ViewTitle);

        return _selectionNotifier.Task;
    }

    protected void FailSelection(Exception ex)
    {
        _selectionNotifier.NotifyFail(ex);
        OnClick_NavigateBackInternal();
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

        //Clear View Data
        ClearBlackboardData();
    }

    private void OnClick_RefreshInternal()
    {
        OnClick_Refresh();
        Refresh();
    }

    protected virtual void OnClick_NavigateBack()
    {
        ViewManager.NavigateBack();
    }

    protected virtual void OnClick_NavigateHome()
    {
        ViewManager.NavigateHome();
    }

    protected virtual void OnClick_Refresh() {}

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

    protected bool ValidateInput<TVal>(string elementName, [CallerMemberName] string callerMemberName = "")
    {
        if (!ValidateInput(elementName, callerMemberName)) return false;
        try
        {
            TryGetValue<TVal>(elementName, out _, true);
            return true;
        }
        catch (Exception ex)
        {
            var txt = GetElement<TextField>(elementName); //Should be Textfield (would throw during ValidateInput)
            ViewManager.HandleException(new Exception($"({callerMemberName}) Input for \'{txt.label}\' invalid... (expected type = {typeof(TVal).Name})", ex));
        }

        return false;
    }

    protected bool ValidateInput(string elementName, [CallerMemberName] string callerMemberName = "")
    {
        var el = GetElement<VisualElement>(elementName);
        if (el is TextField txt)
        {
            if (string.IsNullOrEmpty(txt.value))
            {
                ViewManager.Exception.Show($"({callerMemberName}) Input for \'{txt.label}\' can not be empty...");
                return false;
            }
        }
        else
        {
            ViewManager.Exception.Show($"({callerMemberName}) Failed to ValidateInput for \'{el.name}\' (unknown type)");
            return false;
        }

        return true;
    }

    protected bool ValidateData(object data, string dataName, [CallerMemberName] string callerMemberName = "")
    {
        if (data == null)
        {
            ViewManager.Exception.Show($"({callerMemberName}) No Value set for \'{dataName}\'.");
            return false;
        }

        return true;
    }

    #region Interface
    protected abstract void OnActivate();
    protected virtual void OnActivate(Dictionary<string,object> data){}
    protected virtual Task OnRefreshData(){return Task.CompletedTask;}
    protected virtual void OnRefreshUI(){}
    protected virtual void OnDeactivate(){}
    #endregion
}
