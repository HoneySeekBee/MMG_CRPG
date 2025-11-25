using WebServer.GrpcServices;

namespace WebServer.Extensions
{
    public static class GrpcExtensions
    {
        public static IServiceCollection AddGrpcServices(this IServiceCollection services)
        {
            services.AddGrpc(options =>
            { 
            });

            return services;
        }

        public static IEndpointRouteBuilder MapGrpcServices(this IEndpointRouteBuilder app)
        {
            // gRPC 서비스 등록
            app.MapGrpcService<UserServiceGrpc>();
            app.MapGrpcService<InventoryServiceGrpc>();
            app.MapGrpcService<WalletServiceGrpc>();
             
            return app;
        }
    }
}
