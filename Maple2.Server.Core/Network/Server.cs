using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Maple2.Server.Core.Network;

public abstract class Server<T>(ushort port, PacketRouter<T> router, IComponentContext context) : BackgroundService, IHealthCheck
    where T : Session {
    private enum ServerState {
        Unstarted,
        Running,
        Stopped,
    }

    private readonly IComponentContext context = context ?? throw new ArgumentException("null context provided");

    private ServerState state = ServerState.Unstarted;

    protected readonly ILogger Logger = Log.Logger.ForContext<T>();

    public ushort Port { get; private set; } = port;

    public abstract void OnConnected(T session);
    public abstract void OnDisconnected(T session);
    protected abstract void AddSession(T session);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        state = ServerState.Running;

        Logger.Information("{Type} started on Port:{Port}", GetType().Name, Port);
        await using CancellationTokenRegistration registry = cancellationToken.Register(() => listener.Stop());
        while (!cancellationToken.IsCancellationRequested) {
            TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);

            var session = context.Resolve<T>(new NamedParameter("tcpClient", client));
            session.OnPacket += router.OnPacket;

            AddSession(session);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) {
        switch (state) {
            case ServerState.Unstarted:
                Logger.Information("{Type} has not been started", GetType().Name);
                break;
            case ServerState.Running:
                state = ServerState.Stopped;
                Logger.Information("{Type} was stopped", GetType().Name);
                break;
            case ServerState.Stopped:
                Logger.Information("{Type} has already been stopped", GetType().Name);
                break;
        }

        return Task.CompletedTask;
    }

    public virtual Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthContext, CancellationToken cancellationToken = new()) {
        switch (state) {
            case ServerState.Unstarted:
                return Task.FromResult(HealthCheckResult.Unhealthy("Server has not started."));
            case ServerState.Running:
                return Task.FromResult(HealthCheckResult.Healthy("Server is running."));
            case ServerState.Stopped:
                return Task.FromResult(HealthCheckResult.Unhealthy("Server has been stopped."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy($"Invalid server state: {state}"));
    }
}
