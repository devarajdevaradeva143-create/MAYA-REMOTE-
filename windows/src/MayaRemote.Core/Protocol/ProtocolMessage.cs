using System;

namespace MayaRemote.Core.Protocol;

public static class MessageTypes
{
    public const ushort PING = 1;
    public const ushort PONG = 2;
    public const ushort PAIR_REQUEST = 10;
    public const ushort PAIR_RESPONSE = 11;
    public const ushort AUTH_REQUEST = 20;
    public const ushort AUTH_RESPONSE = 21;
    public const ushort DEVICE_INFO = 30;
    public const ushort CONNECTION_STATUS = 40;
    public const ushort DISCONNECT = 50;
    public const ushort REVOKE_DEVICE = 60;
    public const ushort ERROR = 99;
}

public sealed class ProtocolMessage
{
    public byte ProtocolVersion { get; }
    public ushort MessageType { get; }
    public ulong MessageId { get; }
    public byte[] Payload { get; }

    public ProtocolMessage(byte protocolVersion, ushort messageType, ulong messageId, byte[] payload)
    {
        ProtocolVersion = protocolVersion;
        MessageType = messageType;
        MessageId = messageId;
        Payload = payload ?? Array.Empty<byte>();
    }

    public byte[] Encode()
    {
        var buf = new byte[15 + Payload.Length];
        buf[0] = ProtocolVersion;
        buf[1] = (byte)(MessageType >> 8);
        buf[2] = (byte)(MessageType & 0xFF);
        // big-endian ulong
        for (int i = 0; i < 8; i++) buf[3 + i] = (byte)(MessageId >> (56 - i * 8));
        // big-endian length
        int len = Payload.Length;
        buf[11] = (byte)(len >> 24);
        buf[12] = (byte)(len >> 16);
        buf[13] = (byte)(len >> 8);
        buf[14] = (byte)(len & 0xFF);
        Buffer.BlockCopy(Payload, 0, buf, 15, Payload.Length);
        return buf;
    }

    public static ProtocolMessage? Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < 15) return null;
        byte version = data[0];
        if (version != 1) return null;
        ushort type = (ushort)((data[1] << 8) | data[2]);
        ulong id = 0;
        for (int i = 0; i < 8; i++) id = (id << 8) | data[3 + i];
        int len = (data[11] << 24) | (data[12] << 16) | (data[13] << 8) | data[14];
        if (len < 0 || len > 1 * 1024 * 1024) return null;
        if (data.Length < 15 + len) return null;
        var payload = new byte[len];
        data.Slice(15, len).CopyTo(payload);
        return new ProtocolMessage(version, type, id, payload);
    }
}
