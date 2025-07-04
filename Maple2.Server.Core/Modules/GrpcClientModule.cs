using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace Maple2.Server.Core.Modules;

public abstract class GrpcClientModule {
    public abstract void Configure(IServiceCollection services);

    protected abstract void Options(GrpcClientFactoryOptions options);
}

public static class ServiceModuleExtensions {
    public static void RegisterModule<TModule>(this IServiceCollection services) where TModule : GrpcClientModule, new() {
        if (services == null) {
            throw new ArgumentNullException(nameof(services));
        }

        var module = new TModule();
        module.Configure(services);
    }
}
