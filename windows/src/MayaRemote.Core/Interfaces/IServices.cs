using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Models;
using MayaRemote.Core.Protocol;

namespace MayaRemote.Core.Interfaces;

public interface ITransport
{
    string Endpoint { get; }
    bool IsConnected { get; }
    Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task SendAsync(ProtocolMessage message, CancellationToken cancellationToken = default);
    event Action<ProtocolMessage>? MessageReceived;
    event Action? Connected;
    event Action? Disconnected;
    event Action<Exception>? ErrorOccurred;
}

public interface IDiscoveryService
{
    Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(int timeoutMs = 3000, CancellationToken cancellationToken = default);
    event Action<DiscoveredDevice>? DeviceDiscovered;
}

public interface IPairingManager
{
    Task<PairingInfo> InitiatePairingAsync(string deviceName, CancellationToken cancellationToken = default);
    Task<bool> RespondToPairingAsync(Guid deviceId, byte[] challengeResponse, CancellationToken cancellationToken = default);
    Task<bool> IsPairedAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task RevokeDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);
    event Action<DeviceIdentity>? DevicePaired;
    event Action<Guid>? DeviceRevoked;
}

public interface IConnectionManager
{
    Task<ConnectionInfo> ConnectAsync(string endpoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<bool> SendAsync(Guid connectionId, ProtocolMessage message, CancellationToken cancellationToken = default);
    IReadOnlyList<ConnectionInfo> GetConnections();
    event Action<ConnectionInfo>? ConnectionEstablished;
    event Action<Guid>? ConnectionLost;
}

public interface IAuthenticationService
{
    byte[] CreateChallenge();
    bool VerifyResponse(byte[] challenge, byte[] response, byte[] expectedKey);
    Task AuthenticateAsync(Guid connectionId, CancellationToken cancellationToken = default);
}

public interface IDeviceStorage
{
    DeviceIdentity? Load();
    void Save(DeviceIdentity identity);
    void Clear();
}

public interface ILogger
{
    void Log(LogLevel level, string message, Exception? exception = null);
    IDisposable BeginScope(string state);
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

public interface IConfiguration
{
    T Get<T>(string key, T defaultValue = default);
    void Set<T>(string key, T value);
    void Save();
    void Reload();
}

public interface IScreenCaptureService
{
    Task<byte[]> CaptureScreenAsync(CancellationToken cancellationToken = default);
    Task<byte[]> CaptureRegionAsync(int left, int top, int right, int bottom, CancellationToken cancellationToken = default);
}

public interface IInputService
{
    Task SendKeyAsync(ushort virtualKey, bool keyDown);
    Task SendMouseAsync(int x, int y, uint flags);
}

public interface IApplicationControlService
{
    Task StartProcessAsync(string fileName, string? arguments = null);
    Task KillProcessAsync(int processId);
    IReadOnlyList<ProcessInfo> GetRunningProcesses();
}

public sealed class ProcessInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? MainWindowTitle { get; set; }
}

public interface IAudioService
{
    Task SetVolumeAsync(float volume);
    Task<float> GetVolumeAsync();
    Task SetMuteAsync(bool muted);
    Task<bool> GetMuteAsync();
}

public interface IPermissionService
{
    bool IsElevated { get; }
    Task<bool> RequestElevationAsync();
}

public sealed class DiscoveredDevice
{
    public string DeviceName { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public Guid DeviceId { get; init; }
    public string? Fingerprint { get; init; }
    public bool IsPaired { get; init; }
    public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;
}
