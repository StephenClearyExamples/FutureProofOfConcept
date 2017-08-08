using System.Threading.Tasks;

namespace FuturePlayground
{
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
}