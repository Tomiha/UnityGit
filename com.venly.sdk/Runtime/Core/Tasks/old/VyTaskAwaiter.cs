//using System;
//using System.Runtime.CompilerServices;
//using Proto.Promises.Async.CompilerServices;

//namespace VenlySDK.Core
//{
//    public class VyTaskAwaiter : ICriticalNotifyCompletion
//    {
//        public bool IsCompleted => _source.IsCompleted;

//        private TaskBase _source;

//        public VyTaskAwaiter(TaskBase source)
//        {
//            _source = source;
//        }

//        public VyTaskResult GetResult()
//        {
//            return _source.Awaiter_GetVoidResult();
//        }

//        public void OnCompleted(Action continuation)
//        {
//            _source.Awaiter_OnCompleted(continuation);
//        }

//        public void UnsafeOnCompleted(Action continuation)
//        {
//            _source.Awaiter_OnUnsafeCompleted(continuation);
//        }
//    }

//    public class VyTaskAwaiter<T> : ICriticalNotifyCompletion
//    {
//        public bool IsCompleted => _source.IsCompleted;
//        protected readonly VyTask<T> _source;

//        public VyTaskAwaiter(VyTask<T> source)
//        {
//            _source = source;
//        }

//        public VyTaskResult<T> GetResult()
//        {
//            return _source.Awaiter_GetResult();
//        }

//        public void OnCompleted(Action continuation)
//        {
//            _source.Awaiter_OnCompleted(continuation);
//        }

//        public void UnsafeOnCompleted(Action continuation)
//        {
//            _source.Awaiter_OnUnsafeCompleted(continuation);
//        }
//    }
//}
