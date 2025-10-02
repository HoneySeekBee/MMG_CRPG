using WebServer.Formatters;
using Microsoft.Extensions.DependencyInjection;

public static class ProtoExtensions
{
    public static IServiceCollection AddProtoFormatters(this IServiceCollection services)
    {
        services.AddControllers(opts =>
        {
            opts.InputFormatters.Insert(0, new ProtobufInputFormatter());
            opts.OutputFormatters.Insert(0, new ProtobufOutputFormatter());
        });
        return services;
    }
}
