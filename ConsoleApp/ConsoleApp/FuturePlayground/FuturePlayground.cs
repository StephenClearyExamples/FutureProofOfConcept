using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FuturePlayground
{
    [AsyncMethodBuilder(typeof(FutureAsyncMethodBuilder))]
    public interface IFuture
    {
        bool IsCompleted { get; }
    }

    [AsyncMethodBuilder(typeof(FutureAsyncMethodBuilder<>))]
    public interface IFuture<out T> : IFuture
    {
    }

    public static class FutureEx
    {
        public static async Task<T> AsTask<T>(this IFuture<T> @this)
        {
            var future = (Future)@this;
            return (T)await future.Task.ConfigureAwait(false);
        }

        public static Task AsTask(this IFuture @this)
        {
            var future = (Future)@this;
            return future.Task;
        }

        public static TaskAwaiter<T> GetAwaiter<T>(this IFuture<T> @this) => @this.AsTask().GetAwaiter();
        public static TaskAwaiter GetAwaiter(this IFuture @this) => @this.AsTask().GetAwaiter();

        public static ConfiguredTaskAwaitable<T> ConfigureAwait<T>(this IFuture<T> @this, bool continueOnCapturedContext) => @this.AsTask().ConfigureAwait(continueOnCapturedContext);
        public static ConfiguredTaskAwaitable ConfigureAwait(this IFuture @this, bool continueOnCapturedContext) => @this.AsTask().ConfigureAwait(continueOnCapturedContext);
    }

    public class FutureHelpers
    {
        public static async Task<object> Box<T>(Task<T> source) => await source.ConfigureAwait(false);
        public static async Task<object> Box(Task source)
        {
            await source;
            return null;
        }
    }

    public class Future : IFuture
    {
        public Future(Task<object> task)
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
