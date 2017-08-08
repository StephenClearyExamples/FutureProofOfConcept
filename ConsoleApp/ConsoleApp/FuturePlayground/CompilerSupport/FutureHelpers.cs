using System.Threading.Tasks;

namespace FuturePlayground.CompilerSupport
{
    public class FutureHelpers
    {
        public static async Task<object> Box<T>(Task<T> source) => await source.ConfigureAwait(false);
        public static async Task<object> Box(Task source)
        {
            await source.ConfigureAwait(false);
            return null;
        }
    }
}