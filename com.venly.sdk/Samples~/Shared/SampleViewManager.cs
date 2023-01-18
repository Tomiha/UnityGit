using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VenlySDK;
using VenlySDK.Models;

public abstract class SampleViewManager<T> : MonoBehaviour where T : Enum
{
    public List<SampleViewBase<T>> Views = new ();

    public T LandingDevMode = default;
    public T LandingAuth = default;

    public ApiExplorer_LoaderVC Loader;

    private bool _firstFrame = true;

    private SampleViewBase<T> _currView = null;
    private T _homeViewId;
    private Dictionary<string, object> _globalBlackboard = new();

    // Start is called before the first frame update
    void Start()
    {
        if(!Venly.IsInitialized) 
            Venly.Initialize();

        InitializeViews();
        Loader.Hide();
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

    public void SwitchView(SampleViewBase<T> targetView, bool setBackNavigation = true)
    {
        if (targetView == null)
        {
            throw new Exception("SwitchView Failed, TargetView is null.");
        }

        if (setBackNavigation)
            targetView.PreviousView = _currView;

        _currView?.Deactivate();

        _currView = targetView;
        _currView?.Activate();
    }

    public void SwitchView(T targetViewId, bool setBackNavigation = true)
    {
        SwitchView(GetView(targetViewId), setBackNavigation);
    }

    public void NavigateHome()
    {
        SwitchView(_homeViewId, false);
    }

    public void NavigateBack()
    {
        if (_currView?.PreviousView != null)
        {
            SwitchView(_currView.PreviousView.ViewId, false);
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

    public bool HasGlobalBlackboardData(string key)
    {
        return _globalBlackboard.ContainsKey(key);
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
        Debug.LogException(ex);
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
        _homeViewId = VenlySettings.BackendProvider == eVyBackendProvider.DevMode ? LandingDevMode : LandingAuth;
        SwitchView(_homeViewId);
    }
}
