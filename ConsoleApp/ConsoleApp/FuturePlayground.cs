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
        bool IsCompleted { get; }
    }

    public static class FutureEx
    {
        public static FutureAwaiter<T> GetAwaiter<T>(this IFuture<T> @this) => new FutureAwaiter<T>(((Future)@this).Task.GetAwaiter());
        public static FutureAwaiter<Unit> GetAwaiter(this IFuture @this) => new FutureAwaiter<Unit>(((Future)@this).Task.GetAwaiter());

        public static ConfiguredFutureAwaitable<T> ConfigureAwait<T>(this IFuture<T> @this, bool continueOnCapturedContext) => new ConfiguredFutureAwaitable<T>(@this, continueOnCapturedContext);
        public static ConfiguredFutureAwaitable ConfigureAwait(this IFuture @this, bool continueOnCapturedContext) => new ConfiguredFutureAwaitable(@this, continueOnCapturedContext);

        public static async Task<T> AsTask<T>(this IFuture<T> @this)
        {
            var future = (Future) @this;
            return (T)await future.Task.ConfigureAwait(false);
        }

        public static Task AsTask(this IFuture @this)
        {
            var future = (Future) @this;
            return future.Task;
        }
    }

    public class FutureHelpers
    {
        public static async Task<object> Box<T>(Task<T> source) => await source.ConfigureAwait(false);
    }

    public abstract class Future : IFuture
    {
        protected Future(Task<object> task)
        {
            Task = task;
        }

        public Task<object> Task { get; }
        public bool IsCompleted => Task.IsCompleted;
    }

    public sealed class Future<T> : Future, IFuture<T>
    {
        public Future(Task<object> task)
            : base(task)
        {
        }
    }

    public sealed class FutureAwaiter<T> : ICriticalNotifyCompletion
    {
        private TaskAwaiter<object> _awaiter;

        public FutureAwaiter(TaskAwaiter<object> awaiter)
        {
            _awaiter = awaiter;
        }

        public bool IsCompleted => _awaiter.IsCompleted;
        public T GetResult() => (T)_awaiter.GetResult();
        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }

    public sealed class ConfiguredFutureAwaiter<T> : ICriticalNotifyCompletion
    {
        private ConfiguredTaskAwaitable<object>.ConfiguredTaskAwaiter _awaiter;

        public ConfiguredFutureAwaiter(ConfiguredTaskAwaitable<object>.ConfiguredTaskAwaiter awaiter)
        {
            _awaiter = awaiter;
        }

        public bool IsCompleted => _awaiter.IsCompleted;
        public T GetResult() => (T)_awaiter.GetResult();
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

        public ConfiguredFutureAwaiter<T> GetAwaiter() => new ConfiguredFutureAwaiter<T>(((Future) _future).Task.ConfigureAwait(_continueOnCapturedContext).GetAwaiter());
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

        public ConfiguredFutureAwaiter<Unit> GetAwaiter() => new ConfiguredFutureAwaiter<Unit>(((Future) _future).Task.ConfigureAwait(_continueOnCapturedContext).GetAwaiter());
    }

    public struct FutureAsyncMethodBuilder
    {
        private AsyncTaskMethodBuilder<Unit> _taskMethodBuilder;

        public FutureAsyncMethodBuilder(AsyncTaskMethodBuilder<Unit> taskMethodBuilder)
        {
            _taskMethodBuilder = taskMethodBuilder;
            Task = new Future<Unit>(FutureHelpers.Box(_taskMethodBuilder.Task));
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
