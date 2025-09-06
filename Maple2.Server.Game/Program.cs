﻿using System.Globalization;
using System.Net;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using Grpc.Core;
using Grpc.Net.Client;
using Maple2.Database.Storage;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Modules;
using Maple2.Server.Core.Network;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Config;
using Maple2.Server.Game;
using Maple2.Server.Game.Commands;
using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Service;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using Maple2.Server.Game.Util.Sync;
using Maple2.Server.World.Service;
using Maple2.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

// Force Globalization to en-US because we use periods instead of commas for decimals
CultureInfo.CurrentCulture = new("en-US");
Console.OutputEncoding = System.Text.Encoding.UTF8;

if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase)) {
    DotEnv.Load();
}

// Load YAML server configuration once
ConfigProvider.Initialize();

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configRoot)
    .CreateLogger();

// Check for the --instanced parameter
bool overrideInstanced = args.Contains("--instanced");

AddChannelResponse? response = null;
int attempt = 0;
GrpcChannel channel = GrpcChannel.ForAddress(Target.GrpcWorldUri);
var worldClient = new WorldClient(channel);

// Retry until World responds, with exponential backoff (cap 30s) and jitter
while (true) {
    try {
        response = worldClient.AddChannel(new AddChannelRequest {
            GameIp = Target.GameIp.ToString(),
            GrpcGameIp = Target.GrpcGameIp,
            InstancedContent = overrideInstanced || Target.InstancedContent,
        });
        if (response != null && response.GamePort != 0 && response.GrpcPort != 0) {
            break; // Success with valid ports
        }
        // Received an invalid allocation (likely due to a race). Retry.
        attempt++;
        int baseDelayMs = (int)Math.Min(30000, 1000 * Math.Pow(2, Math.Min(attempt, 6)));
        int jitterMs = Random.Shared.Next(250, 1000);
        int delayMs = baseDelayMs + jitterMs;
        Log.Warning("World returned invalid ports. Retry {Attempt} in {DelayMs}ms", attempt, delayMs);
        await Task.Delay(delayMs);
    } catch (RpcException) {
        attempt++;
        int baseDelayMs = (int)Math.Min(30000, 1000 * Math.Pow(2, Math.Min(attempt, 6)));
        int jitterMs = Random.Shared.Next(250, 1000);
        int delayMs = baseDelayMs + jitterMs;
        Log.Warning("World not ready yet. Retry {Attempt} in {DelayMs}ms", attempt, delayMs);
        await Task.Delay(delayMs);
    }
}

if (response == null || response.GamePort == 0) {
    Log.Error("Failed to add channel to World Server. Is the World Server running?");
    return;
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options => {
    options.Listen(new IPEndPoint(IPAddress.Any, response.GrpcPort), listen => {
        listen.Protocols = HttpProtocols.Http2;
    });
});
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(15));

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(dispose: true);

IGraphicsContext debugGraphicsContext = new HeadlessGraphicsContext();

builder.Services.AddGrpc();
builder.Services.RegisterModule<WorldClientModule>();
builder.Services.AddSingleton<GameServer>(provider => new GameServer(
    provider.GetRequiredService<FieldManager.Factory>(),
    provider.GetRequiredService<PacketRouter<GameSession>>(),
    provider.GetRequiredService<IComponentContext>(),
    provider.GetRequiredService<GameStorage>(),
    provider.GetRequiredService<ItemMetadataStorage>(),
    provider.GetRequiredService<ServerTableMetadataStorage>(),
    debugGraphicsContext,
    response.GamePort,
    response.GameChannel
));
builder.Services.AddHostedService<GameServer>(provider => provider.GetService<GameServer>()!);

builder.Services.AddGrpcHealthChecks();
builder.Services.Configure<HealthCheckPublisherOptions>(options => {
    options.Delay = TimeSpan.Zero;
    options.Period = TimeSpan.FromSeconds(10);
});
builder.Services.AddHealthChecks()
    .AddCheck<GameServer>("game_channel_health_check");

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(autofac => {
    autofac.RegisterType<PacketRouter<GameSession>>()
        .As<PacketRouter<GameSession>>()
        .SingleInstance();
    autofac.RegisterType<GameSession>()
        .PropertiesAutowired()
        .AsSelf();
    autofac.RegisterType<ItemStatsCalculator>()
        .PropertiesAutowired()
        .SingleInstance();
    autofac.RegisterType<TriggerCache>()
        .PropertiesAutowired()
        .SingleInstance();
    autofac.RegisterType<PlayerInfoStorage>()
        .SingleInstance();
    autofac.RegisterType<WorldMapGraphStorage>()
        .SingleInstance();
    autofac.RegisterInstance(debugGraphicsContext);

    // Database
    autofac.RegisterModule<GameDbModule>();
    autofac.RegisterModule<DataDbModule>();
    autofac.RegisterModule<WebDbModule>();

    // Make all packet handlers available to PacketRouter
    autofac.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(type => typeof(PacketHandler<GameSession>).IsAssignableFrom(type))
        .As<PacketHandler<GameSession>>()
        .PropertiesAutowired()
        .SingleInstance();

    // ChatCommand Handlers
    autofac.RegisterType<CommandRouter>();

    autofac.RegisterAssemblyTypes(typeof(CommandRouter).Assembly)
        .PublicOnly()
        .WithAttributeFiltering()
        .Where(type => typeof(GameCommand)
            .IsAssignableFrom(type) && !type.IsAbstract)
        .As<GameCommand>()
        .PropertiesAutowired();

    // Managers
    autofac.RegisterType<FieldManager.Factory>()
        .PropertiesAutowired()
        .SingleInstance();
});

WebApplication app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<ChannelService>();
app.MapGrpcHealthChecksService();

await app.RunAsync();
