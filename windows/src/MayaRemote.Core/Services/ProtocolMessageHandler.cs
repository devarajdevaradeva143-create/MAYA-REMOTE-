using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;
using MayaRemote.Core.Protocol;

namespace MayaRemote.Core.Services;

public sealed class ProtocolMessageHandler
{
    private readonly Dictionary<ushort, Action<ProtocolMessage>> _handlers = new();

    public void Register(ushort messageType, Action<ProtocolMessage> handler)
    {
        _handlers[messageType] = handler;
    }

    public bool TryHandle(ProtocolMessage message)
    {
        if (_handlers.TryGetValue(message.MessageType, out var handler))
        {
            handler(message);
            return true;
        }
        return false;
    }
}
