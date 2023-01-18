using System;
using System.Threading.Tasks;

namespace VenlySDK.Core
{
    public abstract class VyTaskBase
    {
        protected static TaskScheduler ForegroundScheduler = TaskScheduler.Default;

        public static void Initialize()
        {
#if UNITY_2017_1_OR_NEWER
            ForegroundScheduler = TaskScheduler.FromCurrentSynchronizationContext();
#else
            ForegroundScheduler = TaskScheduler.Current;
#endif
        }

        public enum eVyTaskState
        {
            Pending,
            Success,
            Failed,
            Cancelled
        }

        public eVyTaskState State => _state;
        protected eVyTaskState _state = eVyTaskState.Pending;

        internal bool _asyncMethodBuilderCreated = false;

        internal bool IsCompleted => _state != eVyTaskState.Pending;
        internal bool IsSuccess => _state == eVyTaskState.Success;
        internal bool IsFail => _state == eVyTaskState.Failed;
        internal bool IsCancelled => _state == eVyTaskState.Cancelled;

        internal bool IsExceptionConsumed { get; private set; } = false;
        internal bool IsResultConsumed { get; private set; } = false;

        protected string _identifier = String.Empty;
        public string Identifier => _identifier;

        internal void SetIdentifier(string identifier = null)
        {
            _identifier = identifier;
        }

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

        //protected abstract void HandleCompletion();
    }
}