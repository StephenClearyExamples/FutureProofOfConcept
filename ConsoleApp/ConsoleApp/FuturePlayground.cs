using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FuturePlayground
{
    public sealed class Unit
    {
        private Unit()
        {
        }

        public static Unit Instance { get; } = new Unit();
    }

    [AsyncMethodBuilder(typeof(FutureAsyncMethodBuilder))]
    public interface IFuture
    {
        bool IsCompleted { get; }
    }

    [AsyncMethodBuilder(typeof(FutureAsyncMethodBuilder<>))]
    public interface IFuture<out T>
    {
    }

    public static class FutureEx
    {
        // Dynamic doesn't work here! :(
        //   [RuntimeBinderException]: 'object' does not contain a definition for 'Task'
        //public static FutureAwaiter<T> GetAwaiter<T>(this IFuture<T> @this) => new FutureAwaiter<T>(((dynamic) @this).Task.GetAwaiter());
        public static FutureAwaiter<T> GetAwaiter<T>(this IFuture<T> @this)
        {
            var task = @this.GetType().GetProperty("Task").GetValue(@this, null);
            var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, new object[0]);
            return new FutureAwaiter<T>((ICriticalNotifyCompletion)awaiter);
        }

        // Dynamic does work here! :D
        public static FutureAwaiter<Unit> GetAwaiter(this IFuture @this) => new FutureAwaiter<Unit>(((dynamic)@this).Task.GetAwaiter());
        //public static FutureAwaiter<Unit> GetAwaiter(this IFuture @this)
        //{
        //    var task = @this.GetType().GetProperty("Task").GetValue(@this, null);
        //    var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, new object[0]);
        //    return new FutureAwaiter<Unit>((ICriticalNotifyCompletion)awaiter);
        //}

        public static ConfiguredFutureAwaitable<T> ConfigureAwait<T>(this IFuture<T> @this, bool continueOnCapturedContext) => new ConfiguredFutureAwaitable<T>(@this, continueOnCapturedContext);
        public static ConfiguredFutureAwaitable ConfigureAwait(this IFuture @this, bool continueOnCapturedContext) => new ConfiguredFutureAwaitable(@this, continueOnCapturedContext);

        public static async Task<T> AsTask<T>(this IFuture<T> @this) => await @this.ConfigureAwait(false);
        public static async Task AsTask(this IFuture @this) => await @this.ConfigureAwait(false);
    }

    public sealed class Future<T> : IFuture<T>, IFuture
    {
        public Future(Task<T> task)
        {
            Task = task;
        }

        public Task<T> Task { get; }
        public bool IsCompleted => Task.IsCompleted;
    }

    public sealed class FutureAwaiter<T> : ICriticalNotifyCompletion
    {
        private readonly ICriticalNotifyCompletion _awaiter;

        public FutureAwaiter(ICriticalNotifyCompletion awaiter)
        {
            _awaiter = awaiter;
        }

        // Dynamic doesn't work here! :(
        //   [RuntimeBinderException]: 'System.ValueType' does not contain a definition for 'IsCompleted'
        //public bool IsCompleted => ((dynamic) _awaiter).IsCompleted;
        public bool IsCompleted => (bool)_awaiter.GetType().GetProperty("IsCompleted").GetValue(_awaiter);
        // Dynamic doesn't work here! :(
        //   [RuntimeBinderException]: 'System.ValueType' does not contain a definition for 'GetResult'
        //public T GetResult() => ((dynamic) _awaiter).GetResult();
        public T GetResult() => (T)_awaiter.GetType().GetMethod("GetResult").Invoke(_awaiter, new object[0]);
        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }

    public struct ConfiguredFutureAwaitable<T>
    {
        private readonly IFuture<T> _future;
        private readonly bool _continueOnCapturedContext;

        public ConfiguredFutureAwaitable(IFuture<T> future, bool continueOnCapturedContext)
        {
            _future = future;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        // Dynamic doesn't work here! :(
        //   [RuntimeBinderException]: 'object' does not contain a definition for 'Task'
        //public FutureAwaiter<T> GetAwaiter() => new FutureAwaiter<T>(((dynamic) _future).Task.ConfigureAwait(_continueOnCapturedContext).GetAwaiter());
        public FutureAwaiter<T> GetAwaiter()
        {
            var task = _future.GetType().GetProperty("Task").GetValue(_future, null);
            var configuredTaskAwaitable = task.GetType().GetMethod("ConfigureAwait").Invoke(task, new object[] { _continueOnCapturedContext });
            var awaiter = configuredTaskAwaitable.GetType().GetMethod("GetAwaiter").Invoke(configuredTaskAwaitable, new object[0]);
            return new FutureAwaiter<T>((ICriticalNotifyCompletion)awaiter);
        }
    }

    public struct ConfiguredFutureAwaitable
    {
        private readonly IFuture _future;
        private readonly bool _continueOnCapturedContext;

        public ConfiguredFutureAwaitable(IFuture future, bool continueOnCapturedContext)
        {
            _future = future;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        // Dynamic does work here! :D
        public FutureAwaiter<Unit> GetAwaiter() => new FutureAwaiter<Unit>(((dynamic)_future).Task.ConfigureAwait(_continueOnCapturedContext).GetAwaiter());
        //public FutureAwaiter<Unit> GetAwaiter()
        //{
        //    var task = _future.GetType().GetProperty("Task").GetValue(_future, null);
        //    var configuredTaskAwaitable = task.GetType().GetMethod("ConfigureAwait").Invoke(task, new object[] { _continueOnCapturedContext });
        //    var awaiter = configuredTaskAwaitable.GetType().GetMethod("GetAwaiter").Invoke(configuredTaskAwaitable, new object[0]);
        //    return new FutureAwaiter<Unit>((ICriticalNotifyCompletion)awaiter);
        //}
    }

    public struct FutureAsyncMethodBuilder
    {
        private AsyncTaskMethodBuilder<Unit> _taskMethodBuilder;

        public FutureAsyncMethodBuilder(AsyncTaskMethodBuilder<Unit> taskMethodBuilder)
        {
            _taskMethodBuilder = taskMethodBuilder;
            Task = new Future<Unit>(_taskMethodBuilder.Task);
        }

        public static FutureAsyncMethodBuilder Create() => new FutureAsyncMethodBuilder(AsyncTaskMethodBuilder<Unit>.Create());

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _taskMethodBuilder.Start(ref stateMachine);
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _taskMethodBuilder.SetStateMachine(stateMachine);
        public void SetResult() => _taskMethodBuilder.SetResult(Unit.Instance);
        public void SetException(Exception exception) => _taskMethodBuilder.SetException(exception);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        public IFuture<Unit> Task { get; }
    }

    public struct FutureAsyncMethodBuilder<T>
    {
        private AsyncTaskMethodBuilder<T> _taskMethodBuilder;

        public FutureAsyncMethodBuilder(AsyncTaskMethodBuilder<T> taskMethodBuilder)
        {
            _taskMethodBuilder = taskMethodBuilder;
            Task = new Future<T>(_taskMethodBuilder.Task);
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
