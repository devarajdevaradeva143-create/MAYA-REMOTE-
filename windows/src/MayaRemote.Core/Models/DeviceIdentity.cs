using System;

namespace MayaRemote.Core.Models;

public sealed class DeviceIdentity
{
    public Guid DeviceId { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public byte[]? PublicKey { get; init; }
    public DateTime PairedAt { get; init; }
    public bool IsPaired { get; init; }

    public string? Fingerprint
    {
        get
        {
            if (PublicKey == null || PublicKey.Length == 0) return null;
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(PublicKey);
            return Convert.ToHexString(hash).AsSpan(0, 16).ToString();
        }
    }
}

public sealed class ConnectionInfo
{
    public Guid ConnectionId { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public DateTime ConnectedAt { get; init; }
    public bool IsAuthenticated { get; init; }
    public Guid? PairedDeviceId { get; init; }
}

public sealed class PairingInfo
{
    public Guid DeviceId { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public byte[]? Challenge { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool Approved { get; init; }
}

public sealed class ProtocolEnvelope
{
    public ushort MessageType { get; set; }
    public ulong MessageId { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
}
