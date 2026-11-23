using System;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Security;
using MayaRemote.Core.Services;
using InfrastructureLogLevel = MayaRemote.Infrastructure.Logging.LogLevel;
using MayaRemote.Networking.Authentication;
using MayaRemote.Networking.Connection;
using MayaRemote.Networking.Discovery;
using MayaRemote.Networking.Pairing;
using MayaRemote.Networking.Transport;
using MayaRemote.WindowsServices.ApplicationControl;
using MayaRemote.WindowsServices.Audio;
using MayaRemote.WindowsServices.Input;
using MayaRemote.WindowsServices.Permissions;
using MayaRemote.WindowsServices.ScreenCapture;

namespace MayaRemote.Agent;

public sealed class AgentHost : IDisposable
{
    private readonly AgentConfiguration _config;
    private readonly CancellationTokenSource _cts = new();
    private ConnectionManager? _connectionManager;
    private PairingManager? _pairingManager;
    private Authenticator? _authenticator;
    private UdpDiscovery? _discovery;
    private bool _disposed;

    public AgentHost(AgentConfiguration config) => _config = config;

    public bool IsRunning { get; private set; }

    public async Task StartAsync()
    {
        if (IsRunning) return;
        LogManager.Logger = new FileLogger(_config.LogFilePath);
        LogManager.Logger.Log(InfrastructureLogLevel.Information, "MayaRemote Agent starting...");

        var deviceId = Guid.NewGuid();
        var storage = new DeviceIdentityStore();
        var identity = storage.Load();
        if (identity != null) deviceId = identity.DeviceId;

        _authenticator = new Authenticator();
        _connectionManager = new ConnectionManager();
        _pairingManager = new PairingManager(null!, storage, _authenticator);
        _discovery = new UdpDiscovery(_config.DeviceName, deviceId, identity != null && identity.IsPaired);

        _discovery.StartListeningAsync(_cts.Token);

        var keepAlive = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try { await Task.Delay(TimeSpan.FromSeconds(_config.KeepAliveIntervalSeconds), _cts.Token); }
                catch { break; }
            }
        }, _cts.Token);

        IsRunning = true;
        LogManager.Logger.Log(InfrastructureLogLevel.Information, $"MayaRemote Agent running on port {_config.TcpPort}. DeviceId={deviceId}");
        Console.Title = $"MayaRemote Agent - {_config.DeviceName}";
        Console.WriteLine($"MayaRemote Agent running on port {_config.TcpPort}");
        Console.WriteLine($"DeviceId: {deviceId}");
        Console.WriteLine("Press Ctrl+C or type 'stop' to quit.");
    }

    public async Task StopAsync()
    {
        if (!IsRunning) return;
        LogManager.Logger.Log(InfrastructureLogLevel.Information, "MayaRemote Agent stopping...");
        _cts.Cancel();
        IsRunning = false;
        LogManager.Logger.Log(InfrastructureLogLevel.Information, "MayaRemote Agent stopped.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopAsync().Wait();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
