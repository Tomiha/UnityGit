using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Venly;
using Venly.Core;
using Venly.Models.Shared;

public class BackTargetArgs<T> where T : Enum
{
    //public BackTargetArgs(){}
    public BackTargetArgs(SampleViewBase<T> target)
    {
        Target = target;
    }

    public SampleViewBase<T> Target { get; set; }
    public Dictionary<string, object> Blackboard { get; private set; }
    public bool RequiresRefresh { get; set; } = true;
    public bool HasBlackboard => Blackboard != null;

    public void SetValue(string key, object value)
    {
        Blackboard ??= new Dictionary<string, object>();
        Blackboard[key] = value;
    }

    public TVal GetValue<TVal>(string key) where TVal : class
    {
        if (HasValue(key))
        {
            return (TVal) Blackboard[key];
        }

        return null;
    }

    public bool HasValue(string key)
    {
        return Blackboard?.ContainsKey(key) ?? false;
    }
}

public class SwitchArgs<T> where T : Enum
{
    public static SwitchArgs<T> Default = new ();

    public SwitchArgs(){}

    public SwitchArgs(SampleViewBase<T> backTarget, bool requiresRefresh = true)
    {
        BackTargetArgs = new BackTargetArgs<T>(backTarget) {RequiresRefresh = requiresRefresh};
    }

    public bool SetBackTarget { get; set; } = true;
    public BackTargetArgs<T> BackTargetArgs { get; set; }
    public bool SkipRefreshData { get; set; } = false;
}

public abstract class SampleViewManager<T> : MonoBehaviour where T : Enum
{
    public List<SampleViewBase<T>> Views = new ();

    public T LandingDevMode = default;
    public T LandingAuth = default;

    public ApiExplorer_LoaderVC Loader;
    public ApiExplorer_ExceptionVC Exception;
    public ApiExplorer_InfoVC Info;

    private bool _firstFrame = true;

    private SampleViewBase<T> _currView = null;
    private T _homeViewId;
    private Dictionary<string, object> _globalBlackboard = new();

    public static SampleViewManager<T> Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        if(!VenlyAPI.IsInitialized) 
            VenlyUnity.Initialize();

        InitializeViews();
        Loader.Hide();
        Exception.Hide();
    }

    void Update()
    {
        if (_firstFrame)
        {
            _firstFrame = false;
            OnFirstFrame();
        }
    }

    public abstract string GetTitle(T viewId);

    public VyTask<object> SelectionMode(SampleViewBase<T> targetView, string viewTitle = null)
    {
        return targetView.SelectionMode(viewTitle);
    }

    public VyTask<object> SelectionMode(T targetViewId, string viewTitle = null)
    {
        var targetView = GetView(targetViewId);
        return SelectionMode(targetView, viewTitle);
    }

    public void SwitchView(SampleViewBase<T> targetView, bool setBackTarget = true, bool forceRefresh = false)
    {
        SwitchView(targetView, new SwitchArgs<T>
        {
            SetBackTarget = setBackTarget,
            //SkipRefreshData = !forceRefresh
        });
    }

    public void SwitchView(SampleViewBase<T> targetView, SwitchArgs<T> args)
    {
        if (targetView == null)
        {
            throw new Exception("SwitchView Failed, TargetView is null.");
        }

        if (args.SetBackTarget)
        {
            targetView.BackTargetArgs = args.BackTargetArgs ?? new BackTargetArgs<T>(_currView);
        }

        _currView?.Deactivate();
        _currView = targetView;
        _currView?.Activate(args);
    }

    public void SwitchView(T targetViewId, SwitchArgs<T> args)
    {
        SwitchView(GetView(targetViewId), args);
    }

    public void SwitchView(T targetViewId, T backTargetId, bool backTargetRefresh = true)
    {
        SwitchView(GetView(targetViewId), new SwitchArgs<T>(GetView(backTargetId), backTargetRefresh));
    }

    public void SwitchView(T targetViewId, SampleViewBase<T> backTarget, bool backTargetRefresh = true)
    {
        SwitchView(GetView(targetViewId), new SwitchArgs<T>(backTarget, backTargetRefresh));
    }

    public void SwitchView(T targetViewId, bool setBackTarget = true, bool forceRefresh = false)
    {
        SwitchView(GetView(targetViewId), setBackTarget, forceRefresh);
    }

    public void NavigateHome()
    {
        SwitchView(_homeViewId, false);
    }

    public void NavigateBack()
    {
        if (_currView?.BackTargetArgs != null)
        {
            //Override Blackboard if required
            if (_currView.BackTargetArgs.HasBlackboard)
            {
                _currView.BackTargetArgs.Target.OverrideBlackboard(_currView.BackTargetArgs.Blackboard);
            }

            //Switch to Back Target
            SwitchView(_currView.BackTargetArgs.Target, new SwitchArgs<T>
            {
                SetBackTarget = false,
                SkipRefreshData = !_currView.BackTargetArgs.RequiresRefresh
            });
        }
    }

    public void SetGlobalBlackboardData(string key, object data)
    {
        _globalBlackboard[key] = data;
    }

    public object GetGlobalBlackboardDataRaw(string key)
    {
        return _globalBlackboard[key];
    }

    public T0 GetGlobalBlackBoardData<T0>(string key) where T0 : class
    {
        return GetGlobalBlackboardDataRaw(key) as T0;
    }

    public void ClearGlobalBlackboardData(string key)
    {
        if (HasGlobalBlackboardData(key))
            _globalBlackboard.Remove(key);
    }

    public bool HasGlobalBlackboardData(string key)
    {
        return _globalBlackboard.ContainsKey(key);
    }

    public void ClearViewBlackboardData(T viewId, string key)
    {
        var view = GetView(viewId);
        view.ClearBlackboardData(key);
    }

    public void ClearViewBlackboardData(T viewId)
    {
        var view = GetView(viewId);
        view.ClearBlackboardData();
    }

    public void SetViewBlackboardData(T viewId, string key, object data)
    {
        var view = GetView(viewId);
        view.SetBlackboardData(key, data);
    }

    public SampleViewBase<T> GetView(T viewId)
    {
        return Views.FirstOrDefault(v => v.ViewId.Equals(viewId));
    }

    public void HandleException(Exception ex)
    {
        Exception.Show(ex);
    }

    void InitializeViews()
    {
        Views.Clear();

        var sampleViewBases = gameObject.transform.GetComponentsInChildren<SampleViewBase<T>>(includeInactive: true);
        Views.AddRange(sampleViewBases);

        Views.ForEach(v =>
        {
            v.gameObject.SetActive(true);
            v.ViewManager = this;
            v.gameObject.SetActive(false);
        });
    }

    void OnFirstFrame()
    {
        switch (VenlySettings.BackendProvider)
        {
            case eVyBackendProvider.DevMode:
            case eVyBackendProvider.Custom:
                _homeViewId = LandingDevMode;
                break;
            default:
                _homeViewId = LandingAuth;
                break;
        }

        SwitchView(_homeViewId);
    }
}
