//#define VYTASK_DEBUG

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VenlySDK.Utils;

namespace VenlySDK.Core
{
    #region TASK_NOTIFIER<T>
    public class VyTaskNotifier
    {
        public VyTask Task => _baseTask;

        private VyTask _baseTask;
        private TaskCompletionSource<VyTaskResult> _nativeTaskCompletionSource;

        private VyTaskNotifier()
        {
            _nativeTaskCompletionSource = new TaskCompletionSource<VyTaskResult>();
        }

        internal static VyTaskNotifier Create(string identifier = null)
        {
            var notifier = new VyTaskNotifier();
            notifier._baseTask = new VyTask(notifier._nativeTaskCompletionSource.Task);
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

        public void NotifySuccess()
        {
#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Task Notify Success (id={_baseTask.NativeTask?.Id} | identifier={_baseTask.Identifier})");
#endif
            _nativeTaskCompletionSource.SetResult(new VyTaskResult());
        }

        public void NotifyFail(Exception ex)
        {
#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Task Notify Fail (id={_baseTask.NativeTask?.Id} | identifier={_baseTask.Identifier})");
#endif
            _nativeTaskCompletionSource.SetResult(new VyTaskResult(ex));
        }

        public void NotifyCancel()
        {
#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Task Notify Cancel (id={_baseTask.NativeTask?.Id} | identifier={_baseTask.Identifier})");
#endif
            _nativeTaskCompletionSource.SetCanceled();
        }

        public void Notify(VyTaskResult result)
        {
#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Task Notify (id={_baseTask.NativeTask?.Id} | identifier={_baseTask.Identifier})");
#endif
            _nativeTaskCompletionSource.SetResult(result);
        }
    }

    #endregion

    #region TASK<T>

    [AsyncMethodBuilder(typeof(VyTaskAsyncMethodBuilder))]
    public class VyTask : VyTaskBase
    {
        internal Task<VyTaskResult> NativeTask => _nativeTask;
        private readonly Task<VyTaskResult> _nativeTask;

        internal VyTaskResult TaskResult => _taskResult;
        private VyTaskResult _taskResult;

        private Action _onSuccessCallback;
        private Action<Exception> _onFailCallback;
        private Action<VyTaskResult> _onCompleteCallback;
        private Action _onFinallyCallback;

        private VyTaskAwaiter _awaiter;
        private VyTaskResultAwaiter _resultAwaiter;

        #region Constructors

        //WITH Native Task
        internal VyTask(Task<VyTaskResult> nativeTask)
        {
            _nativeTask = nativeTask;

#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Task Created (id={_nativeTask.Id} | identifier={_identifier})");
#endif

            _nativeTask.ContinueWith(task =>
                {
#if VYTASK_DEBUG
                    Debug.Log($"[VYTASK_DEBUG] Task Completed (id={_nativeTask.Id} | identifier={_identifier})");
#endif

                    if (task.IsCompletedSuccessfully)
                    {
                        _taskResult = task.Result;
                        _state = _taskResult.Success ? eVyTaskState.Success : eVyTaskState.Failed;
                    }
                    else if (task.IsFaulted)
                    {
                        _taskResult = new VyTaskResult(task.Exception);
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
#if VYTASK_DEBUG
                    Debug.Log($"[VYTASK_DEBUG] Task Failed (id={_nativeTask.Id} | identifier={_identifier})");
#endif

                    //todo: format stacktrace output
                    VenlyLog.Exception(task.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(task => { _onFinallyCallback?.Invoke(); });
        }

        //DIRECT Success
        private VyTask()
        {
            _nativeTask = null;
            _taskResult = new VyTaskResult();
            _state = eVyTaskState.Success;

            HandleCompletion();
        }

        //DIRECT Fail
        private VyTask(Exception ex)
        {
            _nativeTask = null;
            _taskResult = new VyTaskResult(ex);
            _state = eVyTaskState.Failed;

            HandleCompletion();
        }

        #endregion

        #region Creation

        public static VyTaskNotifier Create(string identifier = null)
        {
            return VyTaskNotifier.Create(identifier);
        }

        internal static VyTaskNotifier MethodBuilderCreate()
        {
            var notifier = VyTaskNotifier.Create();
            notifier.Task._asyncMethodBuilderCreated = true;
            return notifier;
        }

        #endregion

        #region State Handlers

        private bool HandleAwaiters()
        {
            if (_awaiter != null || _resultAwaiter != null)
            {
#if VYTASK_DEBUG
                Debug.Log($"[VYTASK_DEBUG] Handle Awaiters (id={_nativeTask?.Id} | identifier={_identifier})");
#endif

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

#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Handle Completion (id={_nativeTask?.Id} | identifier={_identifier})");
#endif

            //todo: verify flow (awaiter - default - methodbuilder)
            //Check for Awaiter
            if (HandleAwaiters())
                return;

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
            _onSuccessCallback.Invoke();
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

        public static VyTask Succeeded(string identifier = null)
        {
            var t =  new VyTask();
            t.SetIdentifier(identifier);
            return t;
        }

        public static VyTask Failed(Exception ex)
        {
            return new VyTask(ex);
        }

        public static VyTask Failed(string msg)
        {
            return new VyTask(new Exception(msg));
        }

        #endregion

        #region Callback Handlers

        public VyTask OnComplete(Action<VyTaskResult> callback)
        {
            _onCompleteCallback = callback;
            if (IsCompleted) CallComplete();

            return this;
        }

        public VyTask OnSuccess(Action callback)
        {
            _onSuccessCallback = callback;
            if (IsSuccess) CallSuccess();

            return this;
        }

        public VyTask OnFail(Action<Exception> callback)
        {
            _onFailCallback = callback;
            if (IsFail) CallFail();

            return this;
        }

        public VyTask Finally(Action callback)
        {
            _onFinallyCallback = callback;
            if (IsCompleted) CallFinally();

            return this;
        }

        #endregion

        #region Awaiter

        public VyTaskAwaiter GetAwaiter()
        {
            _awaiter ??= new VyTaskAwaiter(this);
            return _awaiter;
        }

        //public TaskAwaiter<VyTaskResult> GetAwaiter()
        //{
        //    return _nativeTask.GetAwaiter();
        //}

        public VyTaskResultAwaiter AwaitResult(bool throwOnException = true)
        {
            _resultAwaiter ??= new VyTaskResultAwaiter(this, throwOnException);
            return _resultAwaiter;
        }

        #endregion

        ~VyTask()
        {
            if (IsFail && !IsExceptionConsumed)
            {
                VenlyLog.Exception(_taskResult.Exception);
            }
        }
    }

    #endregion

    #region TASK_AWAITER<T>
    public class VyTaskResultAwaiter : ICriticalNotifyCompletion
    {
        private readonly VyTask _sourceTask;
        private Action _continuation;
        private bool _throwOnException;

        public bool IsCompleted => _sourceTask.IsCompleted;

        public VyTaskResultAwaiter(VyTask sourceTask, bool throwOnException)
        {
            _sourceTask = sourceTask;
            _throwOnException = throwOnException;
            _continuation = null;
        }

        public VyTaskResultAwaiter GetAwaiter()
        {
            return this;
        }

        public bool GetResult()
        {
#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] ResultAwaiter - GetResult (id={_sourceTask.NativeTask?.Id} | identifier={_sourceTask.Identifier})");
#endif

            if (!_sourceTask.TaskResult.Success && _throwOnException)
                throw _sourceTask.TaskResult.Exception;

            return _sourceTask.TaskResult.Success;
        }

        internal void SignalComplete()
        {
#if VYTASK_DEBUG
            Debug.Log($"[VYTASK_DEBUG] Signal Awaiter (Result) (id={_sourceTask.NativeTask?.Id} | identifier={_sourceTask.Identifier})");
#endif
            _continuation?.Invoke();
        }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _continuation = continuation;
        }
    }


    public class VyTaskAwaiter : ICriticalNotifyCompletion
    {
        private readonly VyTask _sourceTask;
        private Action _continuation;

        public bool IsCompleted => _sourceTask.IsCompleted;

        public VyTaskAwaiter(VyTask sourceTask)
        {
            _sourceTask = sourceTask;
            _continuation = null;
        }

        public VyTaskResult GetResult()
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

    public struct VyTaskAsyncMethodBuilder
    {
        private readonly VyTaskNotifier _taskNotifier;
        public VyTask Task => _taskNotifier.Task;

        private VyTaskAsyncMethodBuilder(VyTaskNotifier taskNotifier)
        {
            _taskNotifier = taskNotifier;
        }

        public static VyTaskAsyncMethodBuilder Create()
        {
            return new VyTaskAsyncMethodBuilder(VyTask.MethodBuilderCreate());
        }

        public void SetException(Exception e)
        {
            _taskNotifier.NotifyFail(e);
        }

        public void SetResult()
        {
            _taskNotifier.NotifySuccess();
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