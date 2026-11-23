using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;

namespace MayaRemote.Networking.Discovery;

public sealed class UdpDiscovery : IDiscoveryService
{
    private const int DiscoveryPort = 47111;
    private readonly string _deviceName;
    private readonly Guid _deviceId;
    private readonly bool _isPaired;

    public event Action<DiscoveredDevice>? DeviceDiscovered;

    public UdpDiscovery(string deviceName, Guid deviceId, bool isPaired = false)
    {
        _deviceName = deviceName;
        _deviceId = deviceId;
        _isPaired = isPaired;
    }

    public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(int timeoutMs = 3000, CancellationToken cancellationToken = default)
    {
        var results = new List<DiscoveredDevice>();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);

        using var udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        udpClient.Client.ReceiveTimeout = 200;

        var message = System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "mayaremote.discover",
            deviceName = _deviceName,
            deviceId = _deviceId,
            isPaired = _isPaired
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(message);

        try
        {
            await udpClient.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, DiscoveryPort));
        }
        catch
        {
            // Ignore broadcast failures on interfaces without broadcast support
        }

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(cts.Token);
                var json = System.Text.Encoding.UTF8.GetString(result.Buffer);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.GetProperty("type").GetString() != "mayaremote.discover.response") continue;

                var device = new DiscoveredDevice
                {
                    DeviceName = root.GetProperty("deviceName").GetString() ?? string.Empty,
                    Endpoint = result.RemoteEndPoint.Address.ToString() + ":" + DiscoveryPort,
                    DeviceId = Guid.Parse(root.GetProperty("deviceId").GetString() ?? Guid.Empty.ToString()),
                    Fingerprint = root.TryGetProperty("fingerprint", out var fp) ? fp.GetString() : null,
                    IsPaired = root.GetProperty("isPaired").GetBoolean()
                };
                results.Add(device);
                DeviceDiscovered?.Invoke(device);
            }
            catch (OperationCanceledException) { break; }
            catch (SocketException) { break; }
            catch { }
        }

        return results;
    }

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        _ = Task.Run(() => ListenLoopAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        using var udpClient = new UdpClient(DiscoveryPort) { EnableBroadcast = true };
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(cancellationToken);
                var json = System.Text.Encoding.UTF8.GetString(result.Buffer);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.GetProperty("type").GetString() != "mayaremote.discover") continue;

                var response = new
                {
                    type = "mayaremote.discover.response",
                    deviceName = _deviceName,
                    deviceId = _deviceId,
                    isPaired = _isPaired
                };
                var responseBytes = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(response));
                await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
            }
            catch (OperationCanceledException) { break; }
            catch { }
        }
    }
}
