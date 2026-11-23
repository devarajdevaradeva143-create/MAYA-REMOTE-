using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Protocol;

namespace MayaRemote.Networking.Transport;

public sealed class TcpTransport : ITransport, IDisposable
{
    private readonly int _receiveBufferSize = 65536;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveTask;
    private readonly object _lock = new();

    public string Endpoint { get; private set; } = string.Empty;
    public bool IsConnected => _client?.Connected == true && _stream != null;

    public event Action<ProtocolMessage>? MessageReceived;
    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<Exception>? ErrorOccurred;

    public async Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var parts = endpoint.Split(':', 2);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
            throw new ArgumentException("Endpoint must be in host:port format.", nameof(endpoint));

        lock (_lock)
        {
            DisposeSocket();
        }

        _client = new TcpClient();
        await _client.ConnectAsync(parts[0], port).WaitAsync(cancellationToken);
        _client.NoDelay = true;
        _stream = _client.GetStream();
        Endpoint = endpoint;

        _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _receiveTask = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), _receiveCts.Token);

        Connected?.Invoke();
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var localTask = _receiveTask;
        if (_receiveCts != null) _receiveCts.Cancel();
        if (localTask != null)
        {
            try { await localTask; } catch { }
        }

        lock (_lock)
        {
            DisposeSocket();
        }

        Endpoint = string.Empty;
        Disconnected?.Invoke();
    }

    public async Task SendAsync(ProtocolMessage message, CancellationToken cancellationToken = default)
    {
        var buffer = message.Encode();
        NetworkStream? stream;
        lock (_lock) stream = _stream;
        if (stream == null) throw new InvalidOperationException("Not connected.");
        await stream.WriteAsync(buffer, cancellationToken);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        NetworkStream? stream;
        lock (_lock) stream = _stream;
        if (stream == null) return;

        var headerBuffer = new byte[15];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int headerRead = 0;
                while (headerRead < headerBuffer.Length && !cancellationToken.IsCancellationRequested)
                {
                    var read = await stream.ReadAsync(headerBuffer.AsMemory(headerRead), cancellationToken);
                    if (read == 0) break;
                    headerRead += read;
                }

                if (headerRead < headerBuffer.Length) break;

                var message = ProtocolMessage.Decode(headerBuffer);
                if (message == null) continue;

                int length = BinaryPrimitives.ReadInt32BigEndian(headerBuffer.AsSpan(11, 4));
                if (length > 0)
                {
                    var payload = new byte[length];
                    int payloadRead = 0;
                    while (payloadRead < length && !cancellationToken.IsCancellationRequested)
                    {
                        var read = await stream.ReadAsync(payload.AsMemory(payloadRead), cancellationToken);
                        if (read == 0) break;
                        payloadRead += read;
                    }
                    if (payloadRead < length) break;
                    message = new ProtocolMessage(message.ProtocolVersion, message.MessageType, message.MessageId, payload);
                }

                MessageReceived?.Invoke(message);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }
        finally
        {
            Disconnected?.Invoke();
        }
    }

    private void DisposeSocket()
    {
        try { _stream?.Dispose(); } catch { }
        try { _client?.Dispose(); } catch { }
        _stream = null;
        _client = null;
    }

    public void Dispose()
    {
        DisconnectAsync(CancellationToken.None).GetAwaiter().GetResult();
        _receiveCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class TcpListenerTransport : IDisposable
{
    private readonly TcpListener _listener;
    private CancellationTokenSource? _acceptCts;
    private Task? _acceptTask;

    public event Action<TcpClient>? ClientAccepted;
    public event Action<Exception>? ErrorOccurred;

    public TcpListenerTransport(int port, IPAddress? address = null)
    {
        _listener = new TcpListener(address ?? IPAddress.Any, port);
    }

    public void Start()
    {
        _listener.Start();
        _acceptCts = new CancellationTokenSource();
        _acceptTask = Task.Run(() => AcceptLoopAsync(_acceptCts.Token), _acceptCts.Token);
    }

    public async Task StopAsync()
    {
        if (_acceptCts != null) _acceptCts.Cancel();
        if (_acceptTask != null)
        {
            try { await _acceptTask; } catch { }
        }
        try { _listener.Stop(); } catch { }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync();
            }
            catch (ObjectDisposedException) { break; }
            catch (SocketException) { break; }
            catch (OperationCanceledException) { break; }
            ClientAccepted?.Invoke(client);
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}
