using MayaRemote.Core.Protocol;
using Xunit;

namespace MayaRemote.Networking.Tests;

public class ProtocolRoundtripTest
{
    [Fact]
    public void EncodeDecode_Roundtrips_WithPayload()
    {
        var original = new ProtocolMessage(1, MessageTypes.PING, 42, new byte[] { 1, 2, 3 });
        var encoded = original.Encode();
        var decoded = ProtocolMessage.Decode(encoded);

        Assert.NotNull(decoded);
        Assert.Equal(original.ProtocolVersion, decoded!.ProtocolVersion);
        Assert.Equal(original.MessageType, decoded.MessageType);
        Assert.Equal(original.MessageId, decoded.MessageId);
        Assert.Equal(original.Payload, decoded.Payload);
    }

    [Fact]
    public void EncodeDecode_Roundtrips_EmptyPayload()
    {
        var original = new ProtocolMessage(1, MessageTypes.PONG, 999, Array.Empty<byte>());
        var decoded = ProtocolMessage.Decode(original.Encode());
        Assert.NotNull(decoded);
        Assert.Equal(original.MessageId, decoded!.MessageId);
        Assert.Empty(decoded.Payload);
    }

    [Fact]
    public void Decode_Rejects_TooShort()
    {
        Assert.Null(ProtocolMessage.Decode(new byte[10]));
    }

    [Fact]
    public void Decode_Rejects_BadVersion()
    {
        var buf = new byte[15];
        buf[0] = 9;
        Assert.Null(ProtocolMessage.Decode(buf));
    }

    [Fact]
    public void Decode_Rejects_TruncatedPayload()
    {
        var original = new ProtocolMessage(1, MessageTypes.PING, 1, new byte[] { 1, 2, 3 });
        var encoded = original.Encode();
        Assert.Null(ProtocolMessage.Decode(encoded.AsSpan(0, encoded.Length - 1)));
    }
}
