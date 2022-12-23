using System;
using System.Threading;
using Proto.Promises;
using Proto.Promises.Async.CompilerServices;

namespace VenlySDK.Core
{
    public struct VyTaskVoid
    {
        internal static VyTaskVoid Empty = new VyTaskVoid();
    }

    public abstract class TaskBase
    {
        public enum eVyTaskState
        {
            Pending,
            Success,
            Failed
        }

        public eVyTaskState State => _state;
        protected eVyTaskState _state = eVyTaskState.Pending;

        internal bool IsCompleted => _state != eVyTaskState.Pending;

        internal bool IsExceptionConsumed { get; private set; } = false;
        internal bool IsResultConsumed { get; private set; } = false;

        internal void ResultConsumed(bool consumed)
        {
            if (consumed && !IsResultConsumed)
                IsResultConsumed = true;
        }

        internal void ExceptionConsumed(bool consumed)
        {
            if (consumed && !IsExceptionConsumed)
                IsExceptionConsumed = true;
        }

        internal abstract void Awaiter_OnCompleted(Action continuation);
        internal abstract void Awaiter_OnUnsafeCompleted(Action continuation);
        internal abstract VyTaskResult Awaiter_GetVoidResult();
    }

    public class VyTask
    {
        //Source
        private TaskBase _baseTask;
        private VyTaskResult _result;

        //Events
        private Action<VyTaskResult> _onCompleteCallback;
        private Action _onSuccesCallback;
        private Action<VyException> _onFailCallback;
        private Action _onFinallyCallback;

        public VyTask(TaskBase baseTask)
        {
            _baseTask = baseTask;
        }

        public static void Configure()
        {
            Promise.Config.DebugCausalityTracer = Promise.TraceLevel.Rejections;
            Promise.Config.ForegroundContext = SynchronizationContext.Current;
        }

        public static VyTaskNotifier Create(string identifier = "")
        {
            var task = new VyTask<VyTaskVoid>($"[VoidPromise] {identifier}");
            return new VyTaskNotifier(task._notifier);
        }

        public VyTaskAwaiter GetAwaiter()
        {
            return new VyTaskAwaiter(_baseTask);
        }

        public VyAwaitResultTask AwaitResult(bool throwOnException = true)
        {
            return new VyAwaitResultTask(_baseTask, throwOnException);
        }

        #region State Handlers
        internal void CallSuccess()
        {
            _onSuccesCallback?.Invoke();
            _baseTask.ResultConsumed(_onSuccesCallback != null);
        }

        internal void CallFail(VyException ex)
        {
            _onFailCallback?.Invoke(ex);
            _baseTask.ExceptionConsumed(_onFailCallback != null);
        }

        internal void CallComplete(VyTaskResult result)
        {
            _onCompleteCallback?.Invoke(result);
            _baseTask.ResultConsumed(_onCompleteCallback != null);
            _baseTask.ExceptionConsumed(_onCompleteCallback != null);
        }

        internal void CallFinally()
        {
            _onFinallyCallback?.Invoke();
        }

        internal void SetResult(VyTaskResult result)
        {
            _result = result;
        }
        #endregion

        #region Direct Notifiers
        public static VyTask Succeeded(string identifier = "")
        {
            return new VyTask<VyTaskVoid>(VyTaskVoid.Empty, $"[Direct Success] {identifier}").ToVoidTask();
        }

        public static VyTask Failed(VyException ex, string identifier = "")
        {
            return new VyTask<VyTaskVoid>(ex, $"[Direct Fail] {identifier}").ToVoidTask();
        }

        public static VyTask Failed(Exception ex, string identifier = "")
        {
            return new VyTask<VyTaskVoid>(new VyException(ex), $"[Direct Fail] {identifier}").ToVoidTask();
        }

        #endregion

        #region Event Subscribers
        public VyTask OnSucces(Action callback)
        {
            _onSuccesCallback = callback;
            if (_baseTask.State == TaskBase.eVyTaskState.Success)
            {
                CallSuccess();
            }

            return this;
        }

        public VyTask OnFail(Action<VyException> callback)
        {
            _onFailCallback = callback;
            if (_baseTask.State == TaskBase.eVyTaskState.Failed)
            {
                CallFail(_result.Exception);
            }

            return this;
        }

        public VyTask OnComplete(Action<VyTaskResult> callback)
        {
            _onCompleteCallback = callback;
            if (_baseTask.IsCompleted)
            {
                CallComplete(_result);
            }

            return this;
        }

        public VyTask Finally(Action callback)
        {
            _onFinallyCallback = callback;
            if (_baseTask.IsCompleted)
            {
                CallFinally();
            }

            return this;
        }
        #endregion
    }

    public class VyTask<T> : TaskBase
    {
        #region Datamembers

        //Result
        internal bool HasResult => _result != null;
        internal VyTaskResult<T> Result => _result;
        private VyTaskResult<T> _result = null;

        //Promises
        internal bool IsPromiseValid => PromiseCreated && _preservedPromise.Value.IsValid;
        internal bool PromiseCreated => _deferredPromise.HasValue;

        private Promise<VyTaskResult<T>>.Deferred? _deferredPromise;
        private Promise<VyTaskResult<T>>? _preservedPromise;
        internal Promise<VyTaskResult<T>>? PreservedPromise => _preservedPromise;
        private PromiseAwaiter<VyTaskResult<T>>? _awaiter;

        //Events
        private Action<VyTaskResult<T>> _onCompleteCallback;
        private Action<T> _onSuccesCallback;
        private Action<VyException> _onFailCallback;
        private Action _onFinallyCallback;

        //Misc
        internal string Identifier;
        internal VyTaskNotifier<T> _notifier;
        private VyTask _voidTask;

        #endregion

        ~VyTask()
        {
            if (PromiseCreated && IsPromiseValid)
                _preservedPromise?.Forget();

            if (_state == eVyTaskState.Failed && !IsExceptionConsumed)
            {
                //Raise as UnhandledException
                VyException.RaiseUnhandled(_result.Exception);
            }
        }

        internal VyTask(string identifier = "")
        {
            Identifier = identifier;

            _deferredPromise = Promise<VyTaskResult<T>>.Deferred.New();
            _preservedPromise = _deferredPromise.Value.Promise.Preserve();

            _preservedPromise.Value
#if !ENABLE_VENLY_AZURE
                .WaitAsync(SynchronizationOption.Foreground)
#endif
                .Then(HandleCompletion)
                .Catch((VyException ex) => { HandleCompletion(new VyTaskResult<T>(ex)); })
                .Catch((Exception ex) =>
                {
                    var venlyEx = new VyException(ex);
                    HandleCompletion(new VyTaskResult<T>(venlyEx));
                })
                .Finally(() =>
                {
                    CallFinally();
                    _preservedPromise?.Forget();
                }).Forget();

            //Create Notifier
            _notifier = new VyTaskNotifier<T>(this);
        }

        internal VyTask(T data, string identifier = "")
        {
            Identifier = identifier;
            HandleCompletion(new VyTaskResult<T>(data));
        }

        internal VyTask(VyException ex, string identifier = "")
        {
            Identifier = identifier;
            HandleCompletion(new VyTaskResult<T>(ex));
        }

        public static VyTaskNotifier<T> Create(string identifier = "")
        {
            var task = new VyTask<T>($"[Promise] {identifier}");
            return task._notifier;
        }

        public VyTaskAwaiter<T> GetAwaiter()
        {
            return new VyTaskAwaiter<T>(this);
        }

        public VyAwaitResultTask<T> AwaitResult(bool throwOnException = true)
        {
            return new VyAwaitResultTask<T>(this, throwOnException);
        }

        public VyTask ToVoidTask()
        {
            _voidTask = new VyTask(this);
            return _voidTask;
        }

#region Internal Notifiers

        internal void _NotifySuccess(T data)
        {
            _deferredPromise?.Resolve(new VyTaskResult<T>(data));
        }

        internal void _Notify(VyTaskResult<T> result)
        {
            _deferredPromise?.Resolve(result);
        }

        internal void _NotifyFail(VyException ex)
        {
            _deferredPromise?.Resolve(new VyTaskResult<T>(ex));
        }

        internal void _NotifyFail(Exception ex)
        {
            _deferredPromise?.Resolve(new VyTaskResult<T>(new VyException(ex)));
        }

#endregion

#region State Handlers
        protected void HandleCompletion(VyTaskResult<T> result)
        {
            _result = result;
            _voidTask?.SetResult(result.ToVoidResult());

            _state = result.Success ? eVyTaskState.Success : eVyTaskState.Failed;

            InvokeCallbacks();
        }

        protected bool InvokeCallbacks()
        {
            if (IsResultConsumed)
                return true;

            if (_state == eVyTaskState.Pending)
                return false;

            //Call OnSuccess/OnFail
            if (_state == eVyTaskState.Success)
            {
                CallSuccess(_result.Data);
            }
            else
            {
                CallFail(_result.Exception);
            }

            //Call OnComplete
            CallComplete(_result);

            ////Fail-Safe, todo: check timing...
            //if (_state == eVyTaskState.Failed && !IsExceptionConsumed)
            //{
            //    ExceptionConsumed(true);
            //    VyException.RaiseUnhandled(_result.Exception);
            //}

            return IsResultConsumed;
        }

        private void CallSuccess(T data)
        {
            if (_voidTask != null)
            {
                _voidTask.CallSuccess();
            }
            else
            {
                _onSuccesCallback?.Invoke(data);
                ResultConsumed(_onSuccesCallback != null);
            }
        }

        private void CallFail(VyException ex)
        {
            if (_voidTask != null)
            {
                _voidTask.CallFail(ex);
            }
            else
            {
                _onFailCallback?.Invoke(ex);
                ResultConsumed(_onFailCallback != null);
                ExceptionConsumed(_onFailCallback != null);
            }
        }

        private void CallComplete(VyTaskResult<T> result)
        {
            if (_voidTask != null)
            {
                _voidTask.CallComplete(result.ToVoidResult());
            }
            else
            {
                _onCompleteCallback?.Invoke(result);
                ResultConsumed(_onCompleteCallback != null);
                ExceptionConsumed(_onCompleteCallback != null);
            }
        }

        private void CallFinally()
        {
            if (_voidTask != null)
            {
                _voidTask.CallFinally();
            }
            else
            {
                _onFinallyCallback?.Invoke();
            }
        }

#endregion

#region Direct Notifiers

        public static VyTask<T> Succeeded(T data, string identifier = "")
        {
            return new VyTask<T>(data, $"[Direct Success] {identifier}");
        }

        public static VyTask<T> Failed(VyException ex, string identifier = "")
        {
            return new VyTask<T>(ex, $"[Direct Fail] {identifier}");
        }

        public static VyTask<T> Failed(Exception ex, string identifier = "")
        {
            return new VyTask<T>(new VyException(ex), $"[Direct Fail] {identifier}");
        }

#endregion

#region Event Subscribers

        public VyTask<T> OnSucces(Action<T> callback)
        {
            _onSuccesCallback = callback;
            if (_state == eVyTaskState.Success)
            {
                CallSuccess(_result.Data);
            }

            return this;
        }

        public VyTask<T> OnFail(Action<VyException> callback)
        {
            _onFailCallback = callback;
            if (_state == eVyTaskState.Failed)
            {
                CallFail(_result.Exception);
            }

            return this;
        }

        public VyTask<T> OnComplete(Action<VyTaskResult<T>> callback)
        {
            _onCompleteCallback = callback;
            if (IsCompleted)
            {
                CallComplete(_result);
            }

            return this;
        }

        public VyTask<T> Finally(Action callback)
        {
            _onFinallyCallback = callback;
            if (IsCompleted)
            {
                CallFinally();
            }

            return this;
        }

#endregion

#region Awaiter Handlers

        internal override void Awaiter_OnCompleted(Action continuation)
        {
            if (IsPromiseValid)
            {
                _awaiter ??= PreservedPromise?.GetAwaiter();
                _awaiter?.UnsafeOnCompleted(continuation);
            }
            else
            {
                continuation();
            }
        }

        internal override void Awaiter_OnUnsafeCompleted(Action continuation)
        {
            if (IsPromiseValid)
            {
                _awaiter ??= PreservedPromise?.GetAwaiter();
                _awaiter?.UnsafeOnCompleted(continuation);
            }
            else
            {
                continuation();
            }
        }

        internal VyTaskResult<T> Awaiter_GetResult()
        {
            ExceptionConsumed(true);
            ResultConsumed(true);

            if (_awaiter.HasValue)
            {
                var result = _awaiter.Value.GetResult();
                return result;
            }

            if (HasResult)
            {
                return _result;
            }

            throw new VyException("Async result requested but no result available.");
        }

        internal override VyTaskResult Awaiter_GetVoidResult()
        {
            ExceptionConsumed(true);
            ResultConsumed(true);

            if (_awaiter.HasValue)
            {
                var result = _awaiter.Value.GetResult();
                return result.ToVoidResult();
            }

            if (HasResult)
            {
                return _result.ToVoidResult();
            }

            throw new VyException("Async result requested but no result available.");
        }

#endregion
    }
}