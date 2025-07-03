using System;
using System.Globalization;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Maple2.Server.Login;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Reflection;
using Maple2.Database.Storage;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Modules;
using Maple2.Server.Core.Network;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Login.Service;
using Maple2.Server.Login.Session;
using Microsoft.Extensions.DependencyInjection;
using Maple2.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// Force Globalization to en-US because we use periods instead of commas for decimals
CultureInfo.CurrentCulture = new("en-US");

DotEnv.Load();

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configRoot)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options => {
    options.ListenAnyIP(Target.GrpcLoginPort, listen => {
        listen.Protocols = HttpProtocols.Http2;
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(dispose: true);

builder.Services.AddGrpc();
builder.Services.RegisterModule<WorldClientModule>();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<LoginServer>(provider => new LoginServer(
    provider.GetRequiredService<PacketRouter<LoginSession>>(),
    provider.GetRequiredService<IComponentContext>(),
    provider.GetRequiredService<GameStorage>(),
    provider.GetRequiredService<ServerTableMetadataStorage>()
));
builder.Services.AddHostedService<LoginServer>(provider => provider.GetService<LoginServer>()!);

builder.Services.AddGrpcHealthChecks();
builder.Services.Configure<HealthCheckPublisherOptions>(options => {
    options.Delay = TimeSpan.Zero;
    options.Period = TimeSpan.FromSeconds(10);
});
builder.Services.AddHealthChecks()
    .AddCheck<LoginServer>("login_health_check");

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(autofac => {
    // Database
    autofac.RegisterModule<GameDbModule>();
    autofac.RegisterModule<DataDbModule>();

    autofac.RegisterType<PacketRouter<LoginSession>>()
        .As<PacketRouter<LoginSession>>()
        .SingleInstance();

    autofac.RegisterType<LoginSession>()
        .PropertiesAutowired()
        .AsSelf();

    // Make all packet handlers available to PacketRouter
    autofac.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(type => typeof(PacketHandler<LoginSession>).IsAssignableFrom(type))
        .As<PacketHandler<LoginSession>>()
        .PropertiesAutowired()
        .SingleInstance();
});


WebApplication app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<LoginService>();

await app.RunAsync();
