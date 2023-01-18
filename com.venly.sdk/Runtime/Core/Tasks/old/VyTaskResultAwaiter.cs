//using System;
//using System.Runtime.CompilerServices;

//namespace VenlySDK.Core
//{
//    public class VyAwaitResultTask
//    {
//        private readonly TaskBase _baseTask;
//        private readonly bool _throwOnException;

//        public VyAwaitResultTask(TaskBase baseTask, bool throwOnException = true)
//        {
//            _baseTask = baseTask;
//            _throwOnException = throwOnException;
//        }

//        public VyTaskResultAwaiter GetAwaiter()
//        {
//            return new VyTaskResultAwaiter(_baseTask, _throwOnException);
//        }
//    }

//    public class VyTaskResultAwaiter : ICriticalNotifyCompletion
//    {
//        public bool IsCompleted => _source.IsCompleted;
//        private readonly TaskBase _source;
//        private readonly bool _throwOnException;

//        public VyTaskResultAwaiter(TaskBase source, bool throwOnException)
//        {
//            _source = source;
//            _throwOnException = throwOnException;
//        }

//        public bool GetResult()
//        {
//            var result = _source.Awaiter_GetVoidResult();

//            if (_throwOnException && !result.Success)
//                throw result.Exception;

//            return result.Success;
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

//    public class VyAwaitResultTask<T>
//    {
//        private readonly VyTask<T> _baseTask;
//        private readonly bool _throwOnException;

//        public VyAwaitResultTask(VyTask<T> baseTask, bool throwOnException)
//        {
//            _baseTask = baseTask;
//            _throwOnException = throwOnException;
//        }

//        public VyTaskResultAwaiter<T> GetAwaiter()
//        {
//            return new VyTaskResultAwaiter<T>(_baseTask, _throwOnException);
//        }
//    }

//    public class VyTaskResultAwaiter<T> : ICriticalNotifyCompletion
//    {
//        public bool IsCompleted => _source.IsCompleted;
//        private readonly VyTask<T> _source;
//        private readonly bool _throwOnException;

//        public VyTaskResultAwaiter(VyTask<T> source, bool throwOnException)
//        {
//            _source = source;
//            _throwOnException = throwOnException;
//        }

//        public T GetResult()
//        {
//            var result = _source.Awaiter_GetResult();

//            if (_throwOnException && !result.Success)
//                throw result.Exception;

//            return result.Data;
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
