using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;
using MayaRemote.Core.Protocol;

namespace MayaRemote.Networking.Pairing;

public sealed class PairingManager : IPairingManager
{
    private readonly ITransport _transport;
    private readonly IDeviceStorage _storage;
    private readonly IAuthenticationService _auth;
    private readonly Dictionary<Guid, PairingInfo> _pending = new();

    public event Action<DeviceIdentity>? DevicePaired;
    public event Action<Guid>? DeviceRevoked;

    public PairingManager(ITransport transport, IDeviceStorage storage, IAuthenticationService auth)
    {
        _transport = transport;
        _storage = storage;
        _auth = auth;
    }

    public async Task<PairingInfo> InitiatePairingAsync(string deviceName, CancellationToken cancellationToken = default)
    {
        var challenge = _auth.CreateChallenge();
        var deviceId = Guid.NewGuid();
        var info = new PairingInfo
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Challenge = challenge,
            CreatedAt = DateTime.UtcNow
        };
        _pending[deviceId] = info;

        var payload = new
        {
            deviceId = deviceId,
            deviceName = deviceName,
            challenge = Convert.ToHexString(challenge)
        };
        await _transport.SendAsync(new ProtocolMessage(1, MessageTypes.PAIR_REQUEST, 0, System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(payload)), cancellationToken);
        return info;
    }

    public Task<bool> RespondToPairingAsync(Guid deviceId, byte[] challengeResponse, CancellationToken cancellationToken = default)
    {
        if (!_pending.TryGetValue(deviceId, out var info)) return Task.FromResult(false);
        var valid = _auth.VerifyResponse(info.Challenge!, challengeResponse, Array.Empty<byte>());
        if (!valid) return Task.FromResult(false);

        var identity = new DeviceIdentity
        {
            DeviceId = deviceId,
            DeviceName = info.DeviceName,
            PairedAt = DateTime.UtcNow,
            IsPaired = true
        };
        _storage.Save(identity);
        _pending.Remove(deviceId);
        DevicePaired?.Invoke(identity);
        return Task.FromResult(true);
    }

    public Task<bool> IsPairedAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var identity = _storage.Load();
        return Task.FromResult(identity != null && identity.DeviceId == deviceId && identity.IsPaired);
    }

    public Task RevokeDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var identity = _storage.Load();
        if (identity != null && identity.DeviceId == deviceId)
        {
            identity.IsPaired = false;
            _storage.Save(identity);
        }
        DeviceRevoked?.Invoke(deviceId);
        return Task.CompletedTask;
    }
}
