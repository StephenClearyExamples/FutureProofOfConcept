using System.Threading.Tasks;

namespace FuturePlayground.CompilerSupport
{
    public class Future : IFuture
    {
        public Future(Task<object> task)
        {
            Task = task;
        }

        public Task<object> Task { get; }
    }

    public sealed class Future<T> : Future, IFuture<T>
    {
        public Future(Task<object> task)
            : base(task)
        {
        }
    }
}