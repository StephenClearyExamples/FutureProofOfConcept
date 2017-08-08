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

    public static class FutureExtensions
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
}