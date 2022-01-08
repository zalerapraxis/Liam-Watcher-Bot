using System.Threading.Tasks;

namespace LiamWatcher
{
    class Program
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}
