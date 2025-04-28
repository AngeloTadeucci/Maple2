using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Maple2.Model.Enum;
using Maple2.PacketLib.Crypto;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Core.Network;

public enum PatchType : byte {
    Delete = 0,
    Patch = 1,
    Ignore = 2,
}

public abstract class Session : IDisposable {
    public const uint VERSION = 12;
    private const uint BLOCK_IV = 12; // TODO: should this be variable

    private const int HANDSHAKE_SIZE = 19;
    private const int STOP_TIMEOUT = 2000;

    public SessionState State { get; set; }

    public EventHandler<string>? OnError;
    public EventHandler<IByteReader>? OnPacket;
    public Action? OnLoop;

    private bool disposed;
    private readonly uint siv;
    private readonly uint riv;

    private readonly string name;
    private readonly TcpClient client;
    private readonly NetworkStream networkStream;
    private readonly MapleCipher.Encryptor sendCipher;
    private readonly MapleCipher.Decryptor recvCipher;

    private readonly Thread thread;
    private readonly QueuedPipeScheduler pipeScheduler;
    private readonly Pipe recvPipe;

    private readonly ConcurrentDictionary<SendOp, byte[]> lastSentPackets = [];

    protected abstract PatchType Type { get; }
    protected readonly ILogger Logger = Log.Logger.ForContext<Session>();

    protected Session(TcpClient tcpClient) {
        thread = new Thread(StartInternal);
        pipeScheduler = new QueuedPipeScheduler();
        var options = new PipeOptions(
            readerScheduler: pipeScheduler,
            writerScheduler: pipeScheduler,
            useSynchronizationContext: false
        );
        recvPipe = new Pipe(options);

        // Allow client to close immediately
        tcpClient.LingerState = new LingerOption(true, 0);
        name = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        byte[] sivBytes = RandomNumberGenerator.GetBytes(4);
        byte[] rivBytes = RandomNumberGenerator.GetBytes(4);
        siv = BitConverter.ToUInt32(sivBytes);
        riv = BitConverter.ToUInt32(rivBytes);

        client = tcpClient;
        networkStream = tcpClient.GetStream();
        sendCipher = new MapleCipher.Encryptor(VERSION, siv, BLOCK_IV);
        recvCipher = new MapleCipher.Decryptor(VERSION, riv, BLOCK_IV);
    }

    ~Session() => Dispose(false);

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposed) return;

        disposed = true;
        State = SessionState.Disconnected;
        Complete();
        thread.Join(STOP_TIMEOUT);

        CloseClient();
    }

    protected void Complete() {
        recvPipe.Writer.Complete();
        recvPipe.Reader.Complete();
        pipeScheduler.Complete();
    }

    public void Disconnect() {
        if (disposed) return;

        Logger.Information("Disconnected {Session}", this);
        Dispose();
    }

    public bool Connected() {
        if (disposed) {
            return false;
        }

        Socket socket = client.Client;
        return !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
    }

    public void Start() {
        if (disposed) {
            throw new ObjectDisposedException("Session has been disposed.");
        }
        if (client == null) {
            throw new InvalidOperationException("Cannot start a session without a client.");
        }

        thread.Start();
    }

    public void Send(params byte[] packet) => SendInternal(packet, packet.Length);

    public void Send(ByteWriter packet) => SendInternal(packet.Buffer, packet.Length);

    public override string ToString() => $"{GetType().Name} from {name}";


    private void StartInternal() {
        try {
            PerformHandshake(); // Perform handshake to initialize connection

            // Pipeline tasks can be run asynchronously
            Task writeTask = WriteRecvPipe(client.Client, recvPipe.Writer);
            Task readTask = ReadRecvPipe(recvPipe.Reader);
            Task.WhenAll(writeTask, readTask).ContinueWith(_ => CloseClient());

            while (!disposed && pipeScheduler.OutputAvailableAsync().Result) {
                pipeScheduler.ProcessQueue();
                OnLoop?.Invoke();
            }
        } catch (Exception ex) {
            if (!disposed) {
                Logger.Error(ex, "Exception on session thread");
            }
        } finally {
            Disconnect();
        }
    }

    private void PerformHandshake() {
        var handshake = new ByteWriter(HANDSHAKE_SIZE);
        handshake.Write<SendOp>(SendOp.RequestVersion);
        handshake.Write<uint>(VERSION);
        handshake.Write<uint>(riv);
        handshake.Write<uint>(siv);
        handshake.Write<uint>(BLOCK_IV);
        handshake.WriteByte((byte) Type);

        // No encryption for handshake
        using PoolByteWriter packet = sendCipher.WriteHeader(handshake.Buffer, 0, handshake.Length);
        SendRaw(packet);
    }

    private async Task WriteRecvPipe(Socket socket, PipeWriter writer) {
        try {
            FlushResult result;
            do {
                Memory<byte> memory = writer.GetMemory();
                int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                if (bytesRead <= 0) {
                    break;
                }

                writer.Advance(bytesRead);

                result = await writer.FlushAsync();
            } while (!disposed && !result.IsCompleted);
        } catch (Exception) {
            Disconnect();
        }
    }

    private async Task ReadRecvPipe(PipeReader reader) {
        try {
            ReadResult result;
            do {
                result = await reader.ReadAsync();

                int bytesRead;
                ReadOnlySequence<byte> buffer = result.Buffer;
                while ((bytesRead = recvCipher.TryDecrypt(buffer, out PoolByteReader packet)) > 0) {
                    try {
#if DEBUG
                        LogRecv(packet.Buffer);
#endif
                        OnPacket?.Invoke(this, packet); // handle packet
                    } finally {
                        packet.Dispose();
                    }
                    buffer = buffer.Slice(bytesRead);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
            } while (!disposed && !result.IsCompleted);
        } catch (Exception ex) {
            if (!disposed) {
                Logger.Error(ex, "Exception reading recv packet");
            }
        } finally {
            Disconnect();
        }
    }


    /*
     * Send packet to client
     * packet: packet to send
     * length: length of packet that only includes data
     */
    private void SendInternal(byte[] packet, int length) {
        if (disposed) return;
#if DEBUG
        LogSend(packet, length);
#endif
        // Track last sent packet by opcode
        if (length >= 2) {
            var op = (SendOp) (packet[1] << 8 | packet[0]);
            // Store a copy to avoid mutation issues
            lastSentPackets[op] = packet.Take(length).ToArray();
        }

        lock (sendCipher) {
            using PoolByteWriter encryptedPacket = sendCipher.Encrypt(packet, 0, length);
            SendRaw(encryptedPacket);
        }
    }

    private void SendRaw(ByteWriter packet) {
        if (disposed) return;

        try {
            networkStream.Write(packet.Buffer, 0, packet.Length);
        } catch (Exception) {
            Disconnect();
        }
    }

    public byte[]? GetLastSentPacket(SendOp op) {
        lastSentPackets.TryGetValue(op, out byte[]? packet);
        return packet;
    }

    private void CloseClient() {
        // Must close socket before network stream to prevent lingering
        client.Client?.Close();
        client.Close();
    }

    private void LogSend(byte[] packet, int length) {
        short op = (short) (packet[1] << 8 | packet[0]);
        SendOp opcode = (SendOp) op;
        switch (opcode) {
            case SendOp.UserSync:
            case SendOp.NpcControl:
            case SendOp.ProxyGameObj:
            case SendOp.ResponseTimeSync:
            case SendOp.Stat:
                break;
            default:
                Logger.Verbose("{Mode} ({Name} - {OpCode}): {Packet}", "SEND".ColorRed(), opcode, $"0x{op:X4}", packet.ToHexString(length, ' '));
                break;
        }
    }

    private void LogRecv(byte[] packet) {
        short op = (short) (packet[1] << 8 | packet[0]);
        RecvOp opcode = (RecvOp) op;
        switch (opcode) {
            case RecvOp.UserSync:
            case RecvOp.RequestTimeSync:
            case RecvOp.GuideObjectSync:
                break;
            default:
                Logger.Verbose("{Mode} ({Name} - {OpCode}): {Packet}", "RECV".ColorGreen(), opcode, $"0x{op:X4}", packet.ToHexString(packet.Length, ' '));
                break;
        }
    }
}
