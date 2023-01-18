using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VenlySDK.Utils;

namespace VenlySDK.Core
{
    #region TASK_NOTIFIER<T>

    public class VyTaskNotifier<T>
    {
        public VyTask<T> Task => _baseTask;

        private VyTask<T> _baseTask;
        private TaskCompletionSource<VyTaskResult<T>> _nativeTaskCompletionSource;

        private VyTaskNotifier()
        {
            _nativeTaskCompletionSource = new TaskCompletionSource<VyTaskResult<T>>();
        }

        internal static VyTaskNotifier<T> Create(string identifier = null)
        {
            var notifier = new VyTaskNotifier<T>();
            notifier._baseTask = new VyTask<T>(notifier._nativeTaskCompletionSource.Task);
            notifier._baseTask.SetIdentifier(identifier);

            return notifier;
        }

        public async void Scope(Func<Task> scope)
        {
            try
            {
                await scope();
            }
            catch (Exception ex)
            {
                NotifyFail(ex);
            }
        }

        public async void Scope(Task t)
        {
            try
            {
                await t;
            }
            catch (Exception ex)
            {
                NotifyFail(ex);
            }
        }

        public void NotifySuccess(T data)
        {
            _nativeTaskCompletionSource.SetResult(new VyTaskResult<T>(data));
        }

        public void NotifyFail(Exception ex)
        {
            _nativeTaskCompletionSource.SetResult(new VyTaskResult<T>(ex));
        }

        public void NotifyFail(string msg)
        {
            NotifyFail(new Exception(msg));
        }

        public void NotifyCancel()
        {
            _nativeTaskCompletionSource.SetCanceled();
        }

        public void Notify(VyTaskResult<T> result)
        {
            _nativeTaskCompletionSource.SetResult(result);
        }
    }

    #endregion

    #region TASK<T>

    [AsyncMethodBuilder(typeof(VyTaskAsyncMethodBuilder<>))]
    public class VyTask<T> : VyTaskBase
    {
        internal Task<VyTaskResult<T>> NativeTask => _nativeTask;
        private readonly Task<VyTaskResult<T>> _nativeTask;

        internal VyTaskResult<T> TaskResult => _taskResult;
        private VyTaskResult<T> _taskResult;

        private Action<T> _onSuccessCallback;
        private Action<Exception> _onFailCallback;
        private Action<VyTaskResult<T>> _onCompleteCallback;
        private Action _onFinallyCallback;

        private VyTaskAwaiter<T> _awaiter;
        private VyTaskResultAwaiter<T> _resultAwaiter;

        #region Constructors

        //WITH Native Task
        internal VyTask(Task<VyTaskResult<T>> nativeTask)
        {
            _nativeTask = nativeTask;

            _nativeTask.ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        _taskResult = task.Result;
                        _state = _taskResult.Success ? eVyTaskState.Success : eVyTaskState.Failed;
                    }
                    else if (task.IsFaulted)
                    {
                        _taskResult = new VyTaskResult<T>(task.Exception);
                        _state = eVyTaskState.Failed;
                    }
                    else if (task.IsCanceled)
                    {
                        _taskResult = null;
                        _state = eVyTaskState.Cancelled;
                    }

                    HandleCompletion();

                }, ForegroundScheduler)
                .ContinueWith(task =>
                {
                    //todo: format stacktrace output
                    VenlyLog.Exception(task.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(task =>
                {
                    _onFinallyCallback?.Invoke();
                }, ForegroundScheduler)
                .ContinueWith(task =>
                {
                    //todo: format stacktrace output
                    VenlyLog.Exception(task.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        //DIRECT Success
        private VyTask(T data)
        {
            _nativeTask = null;
            _taskResult = new VyTaskResult<T>(data);
            _state = eVyTaskState.Success;

            HandleCompletion();
        }

        //DIRECT Fail
        private VyTask(Exception ex)
        {
            _nativeTask = null;
            _taskResult = new VyTaskResult<T>(ex);
            _state = eVyTaskState.Failed;

            HandleCompletion();
        }

        #endregion

        #region Creation

        public static VyTaskNotifier<T> Create(string identifier = null)
        {
            return VyTaskNotifier<T>.Create(identifier);
        }

        #endregion

        #region State Handlers

        private bool HandleAwaiters()
        {
            if (_awaiter != null || _resultAwaiter != null)
            {
                ResultConsumed(true);
                ExceptionConsumed(true);

                _awaiter?.SignalComplete();
                _resultAwaiter?.SignalComplete();

                return true;
            }

            return false;
        }

        protected void HandleCompletion()
            //protected override void HandleCompletion()
        {
            //Check for Awaiter
            if (HandleAwaiters())
                return;

            //Check if Result is already consumed
            if (IsResultConsumed) 
                return;

            //Default Flow
            switch (_state)
            {
                case eVyTaskState.Pending:
                    throw new Exception("VyTask \'{HandleCompletion}\' called but Task is still pending");
                case eVyTaskState.Success:
                    CallSuccess();
                    break;
                case eVyTaskState.Failed:
                    CallFail();
                    break;
                case eVyTaskState.Cancelled:
                    throw new Exception("VyTask \'{HandleCompletion}\' >> Task Canceled (todo)");
            }

            CallComplete();
        }

        private void CallSuccess()
        {
            if (_onSuccessCallback == null) return;
            _onSuccessCallback.Invoke(_taskResult.Data);
            ResultConsumed(true);
        }

        private void CallFail()
        {
            if (_onFailCallback == null) return;
            _onFailCallback.Invoke(_taskResult.Exception);
            ExceptionConsumed(true);
        }

        private void CallComplete()
        {
            if (_onCompleteCallback == null) return;
            _onCompleteCallback.Invoke(_taskResult);
            ResultConsumed(true);
            ExceptionConsumed(true);
        }

        private void CallFinally()
        {
            _onFinallyCallback?.Invoke();
        }

        #endregion

        #region Direct Notifiers

        public static VyTask<T> Succeeded(T data, string identifier = null)
        {
            var t = new VyTask<T>(data);
            t.SetIdentifier(identifier);
            return t;
        }

        public static VyTask<T> Failed(Exception ex)
        {
            return new VyTask<T>(ex);
        }

        public static VyTask<T> Failed(string msg)
        {
            return new VyTask<T>(new Exception(msg));
        }

        #endregion

        #region Callback Handlers

        public VyTask<T> OnComplete(Action<VyTaskResult<T>> callback)
        {
            _onCompleteCallback = callback;
            if (IsCompleted) CallComplete();

            return this;
        }

        public VyTask<T> OnSuccess(Action<T> callback)
        {
            _onSuccessCallback = callback;
            if (IsSuccess) CallSuccess();

            return this;
        }

        public VyTask<T> OnFail(Action<Exception> callback)
        {
            _onFailCallback = callback;
            if (IsFail) CallFail();

            return this;
        }

        public VyTask<T> Finally(Action callback)
        {
            _onFinallyCallback = callback;
            if (IsCompleted) CallFinally();

            return this;
        }

        #endregion

        public VyTaskAwaiter<T> GetAwaiter()
        {
            _awaiter ??= new VyTaskAwaiter<T>(this);
            return _awaiter;
        }

        //public TaskAwaiter<VyTaskResult<T>> GetAwaiter()
        //{
        //    return _nativeTask.GetAwaiter();
        //}

        public VyTaskResultAwaiter<T> AwaitResult()
        {
            _resultAwaiter ??= new VyTaskResultAwaiter<T>(this);
            return _resultAwaiter;
        }
    }

    #endregion

    #region TASK_AWAITER<T>

    public class VyTaskResultAwaiter<T> : ICriticalNotifyCompletion
    {
        private readonly VyTask<T> _sourceTask;
        private Action _continuation;

        public bool IsCompleted => _sourceTask.IsCompleted;

        public VyTaskResultAwaiter(VyTask<T> sourceTask)
        {
            _sourceTask = sourceTask;
            _continuation = null;
        }

        public VyTaskResultAwaiter<T> GetAwaiter()
        {
            return this;
        }

        public T GetResult()
        {
            if (_sourceTask.TaskResult.Success) 
                return _sourceTask.TaskResult.Data;

            throw _sourceTask.TaskResult.Exception;
        }

        internal void SignalComplete()
        {
            _continuation?.Invoke();
        }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;

            //Check if the Task is not yet completed already
            if (_sourceTask.IsCompleted)
                continuation?.Invoke();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _continuation = continuation;

            //Check if the Task is not yet completed already
            if (_sourceTask.IsCompleted)
                continuation?.Invoke();
        }
    }

    public class VyTaskAwaiter<T> : ICriticalNotifyCompletion
    {
        private readonly VyTask<T> _sourceTask;
        private Action _continuation;

        public bool IsCompleted => _sourceTask.IsCompleted;

        public VyTaskAwaiter(VyTask<T> sourceTask)
        {
            _sourceTask = sourceTask;
            _continuation = null;
        }

        public VyTaskResult<T> GetResult()
        {
            return _sourceTask.TaskResult;
        }

        internal void SignalComplete()
        {
            _continuation?.Invoke();
        }

        public void OnCompleted(Action continuation)
        {
            //Debug.Log("VyTask Await - OnComplete Called");
            _continuation = continuation;
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            //Debug.Log("VyTask Await - UnsafeOnComplete Called");
            _continuation = continuation;
        }
    }

    #endregion

    #region TASK_ASYNC_METHOD_BUILDER<T>

    public struct VyTaskAsyncMethodBuilder<T>
    {
        private readonly VyTaskNotifier<T> _taskNotifier;
        public VyTask<T> Task => _taskNotifier.Task;

        private VyTaskAsyncMethodBuilder(VyTaskNotifier<T> taskNotifier)
        {
            this._taskNotifier = taskNotifier;
        }

        public static VyTaskAsyncMethodBuilder<T> Create()
        {
            return new VyTaskAsyncMethodBuilder<T>(VyTask<T>.Create());
        }

        public void SetException(Exception e)
        {
            _taskNotifier.NotifyFail(e);
        }

        public void SetResult(T result)
        {
            _taskNotifier.NotifySuccess(result);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
            //Action move = stateMachine.MoveNext;
            //ThreadPool.QueueUserWorkItem(_ =>
            //{
            //    move();
            //});
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // nothing to do
        }
    }

    #endregion
}