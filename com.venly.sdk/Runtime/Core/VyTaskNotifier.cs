using System;

namespace VenlySDK.Core
{
    public class VyTaskNotifier
    {
        public bool IsCompleted => _baseNotifier.IsCompleted;
        private readonly VyTaskNotifier<VyTaskVoid> _baseNotifier;

        public VyTask Task => _baseNotifier.Task.ToVoidTask();

        public VyTaskNotifier(VyTaskNotifier<VyTaskVoid> baseNotifier)
        {
            _baseNotifier = baseNotifier;
        }

        public void NotifySuccess()
        {
            _baseNotifier.NotifySuccess(VyTaskVoid.Empty);
        }

        public void NotifyFail(string errorMsg)
        {
            _baseNotifier.NotifyFail(new VyException(errorMsg));
        }

        public void NotifyFail(VyException ex)
        {
            _baseNotifier.NotifyFail(ex);
        }

        public void NotifyFail(Exception ex)
        {
            _baseNotifier.NotifyFail(ex);
        }

        public void Notify(VyTaskResult result)
        {
            _baseNotifier.Notify(result.ToVoidResult());
        }
    }

    public class VyTaskNotifier<T>
    {
        public bool IsCompleted => _task?.IsCompleted ?? false;
        public VyTask<T> Task => _task;

        internal VyTask<T> _task;

        internal VyTaskNotifier(VyTask<T> task)
        {
            _task = task;
        }

        public void NotifySuccess(T data)
        {
            _task._NotifySuccess(data);
        }

        public void NotifyFail(string errorMsg)
        {
            _task._NotifyFail(new VyException(errorMsg));
        }

        public void NotifyFail(VyException ex)
        {
            _task._NotifyFail(ex);
        }

        public void NotifyFail(Exception ex)
        {
            _task._NotifyFail(ex);
        }

        public void Notify(VyTaskResult<T> result)
        {
            _task._Notify(result);
        }
    }
}
