using System;
using System.Runtime.CompilerServices;

namespace FuturePlayground.CompilerSupport
{
    public struct FutureAsyncMethodBuilder
    {
        private AsyncTaskMethodBuilder _taskMethodBuilder;

        public FutureAsyncMethodBuilder(AsyncTaskMethodBuilder taskMethodBuilder)
        {
            _taskMethodBuilder = taskMethodBuilder;
            Task = new Future(FutureHelpers.Box(_taskMethodBuilder.Task));
        }

        public static FutureAsyncMethodBuilder Create() => new FutureAsyncMethodBuilder(AsyncTaskMethodBuilder.Create());

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _taskMethodBuilder.Start(ref stateMachine);
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _taskMethodBuilder.SetStateMachine(stateMachine);
        public void SetResult() => _taskMethodBuilder.SetResult();
        public void SetException(Exception exception) => _taskMethodBuilder.SetException(exception);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        public IFuture Task { get; }
    }

    public struct FutureAsyncMethodBuilder<T>
    {
        private AsyncTaskMethodBuilder<T> _taskMethodBuilder;

        public FutureAsyncMethodBuilder(AsyncTaskMethodBuilder<T> taskMethodBuilder)
        {
            _taskMethodBuilder = taskMethodBuilder;
            Task = new Future<T>(FutureHelpers.Box(_taskMethodBuilder.Task));
        }

        public static FutureAsyncMethodBuilder<T> Create() => new FutureAsyncMethodBuilder<T>(AsyncTaskMethodBuilder<T>.Create());

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _taskMethodBuilder.Start(ref stateMachine);
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _taskMethodBuilder.SetStateMachine(stateMachine);
        public void SetResult(T result) => _taskMethodBuilder.SetResult(result);
        public void SetException(Exception exception) => _taskMethodBuilder.SetException(exception);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        public IFuture<T> Task { get; }
    }
}