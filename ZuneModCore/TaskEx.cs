using System.Threading.Tasks;

namespace ZuneModCore
{
    public static class TaskEx
    {
        public static Task<T> FromResult<T>(T result)
        {
            return Task.Factory.StartNew(() => result);
        }

        public static Task CompletedTask => Task.Factory.StartNew(() => { });
    }
}
