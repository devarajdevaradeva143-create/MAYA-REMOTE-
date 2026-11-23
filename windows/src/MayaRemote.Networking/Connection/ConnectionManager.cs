using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;
using MayaRemote.Core.Protocol;
using MayaRemote.Networking.Transport;

namespace MayaRemote.Networking.Connection;

public sealed class ConnectionManager : IConnectionManager
{
    private readonly Dictionary<Guid, (TcpTransport Transport, ConnectionInfo Info)> _connections = new();
    private readonly object _lock = new();

    public event Action<ConnectionInfo>? ConnectionEstablished;
    public event Action<Guid>? ConnectionLost;

    public async Task<ConnectionInfo> ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var transport = new TcpTransport();
        transport.Connected += () => { };
        transport.Disconnected += () => { };
        transport.ErrorOccurred += ex => { };

        await transport.ConnectAsync(endpoint, cancellationToken);
        var connectionId = Guid.NewGuid();
        var info = new ConnectionInfo
        {
            ConnectionId = connectionId,
            Endpoint = endpoint,
            ConnectedAt = DateTime.UtcNow,
            IsAuthenticated = false
        };

        lock (_lock) _connections[connectionId] = (transport, info);

        ConnectionEstablished?.Invoke(info);
        return info;
    }

    public Task DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(connectionId, out var entry))
            {
                _connections.Remove(connectionId);
                _ = entry.Transport.DisconnectAsync(cancellationToken);
            }
        }
        ConnectionLost?.Invoke(connectionId);
        return Task.CompletedTask;
    }

    public Task<bool> SendAsync(Guid connectionId, ProtocolMessage message, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(connectionId, out var entry))
            {
                return entry.Transport.SendAsync(message, cancellationToken)
                    .ContinueWith(t => t.IsCompletedSuccessfully, cancellationToken);
            }
        }
        return Task.FromResult(false);
    }

    public IReadOnlyList<ConnectionInfo> GetConnections()
    {
        lock (_lock)
        {
            var list = new List<ConnectionInfo>(_connections.Count);
            foreach (var kvp in _connections) list.Add(kvp.Value.Info);
            return list;
        }
    }
}
