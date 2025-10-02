using WebServer.HostedServices;

namespace WebServer.Extensions
{
    public static class HostedExtensions
    {
        public static IServiceCollection AddHostedWorkers(this IServiceCollection s)
        {
            s.AddHostedService<CacheWarmupHostedService>();
            return s;
        }
    }
}
