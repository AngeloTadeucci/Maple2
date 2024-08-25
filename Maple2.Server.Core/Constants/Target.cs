using System;
using System.Net;

namespace Maple2.Server.Core.Constants;

public static class Target {
    public const string SERVER_NAME = "Paperwood";
    public const string LOCALE = "NA";

    public static readonly IPAddress LoginIp = IPAddress.Loopback;
    public static readonly ushort LoginPort = 20001;

    public static readonly IPAddress GameIp = IPAddress.Loopback;
    public static readonly ushort BaseGamePort = 20002;

    public static readonly IPAddress GrpcGameIp = IPAddress.Loopback;

    public static readonly Uri WebUri = new("http://localhost");

    public static readonly IPAddress GrpcWorldIp = IPAddress.Loopback;
    public static readonly ushort GrpcWorldPort = 21001;
    public static readonly Uri GrpcWorldUri = new($"http://{GrpcWorldIp}:{GrpcWorldPort}");

    public static readonly ushort BaseGrpcChannelPort = 21002;

    static Target() {
        if (IPAddress.TryParse(Environment.GetEnvironmentVariable("LOGIN_IP"), out IPAddress? loginIpAddress)) {
            LoginIp = loginIpAddress;
        }
        if (ushort.TryParse(Environment.GetEnvironmentVariable("LOGIN_PORT"), out ushort loginPortOverride)) {
            LoginPort = loginPortOverride;
        }

        if (IPAddress.TryParse(Environment.GetEnvironmentVariable("GAME_IP"), out IPAddress? gameIpAddress)) {
            GameIp = gameIpAddress;
        }

        if (IPAddress.TryParse(Environment.GetEnvironmentVariable("GRPC_GAME_IP"), out IPAddress? grpcGameIpAddress)) {
            GrpcGameIp = grpcGameIpAddress;
        }

        if (IPAddress.TryParse(Environment.GetEnvironmentVariable("GRPC_WORLD_IP"), out IPAddress? grpcWorldIpOverride)) {
            GrpcWorldIp = grpcWorldIpOverride;
        }

        if (ushort.TryParse(Environment.GetEnvironmentVariable("GRPC_WORLD_PORT"), out ushort grpcWorldPortOverride)) {
            GrpcWorldPort = grpcWorldPortOverride;
        }

        GrpcWorldUri = new Uri($"http://{GrpcWorldIp}:{GrpcWorldPort}");

        string webIP = Environment.GetEnvironmentVariable("WEB_IP") ?? "localhost";
        string webPort = Environment.GetEnvironmentVariable("WEB_PORT") ?? "4000";

        if (Uri.TryCreate($"http://{webIP}:{webPort}", UriKind.Absolute, out Uri? webUriOverride)) {
            WebUri = webUriOverride;
        }
    }
}
