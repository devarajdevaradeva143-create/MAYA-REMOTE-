package com.maya.remote.core.networking

import java.nio.ByteBuffer
import java.nio.ByteOrder

data class ProtocolMessage(
    val protocolVersion: Int,
    val messageType: Int,
    val messageId: Long,
    val payload: ByteArray
) {
    companion object {
        const val PROTOCOL_VERSION = 1
        const val HEADER_SIZE = 15
        const val MAX_PAYLOAD = 1 * 1024 * 1024

        fun decode(buffer: ByteBuffer): ProtocolMessage? {
            if (buffer.remaining() < HEADER_SIZE) return null
            val dup = buffer.duplicate().order(ByteOrder.BIG_ENDIAN)
            val version = dup.get().toInt() and 0xFF
            val type = dup.short.toInt() and 0xFFFF
            val id = dup.long
            val len = dup.int
            if (version != PROTOCOL_VERSION) return null
            if (len < 0 || len > MAX_PAYLOAD) return null
            if (dup.remaining() < len) return null
            val payload = ByteArray(len)
            dup.get(payload)
            return ProtocolMessage(version, type, id, payload)
        }

        fun encode(msg: ProtocolMessage): ByteArray {
            val buf = ByteBuffer.allocate(HEADER_SIZE + msg.payload.size).order(ByteOrder.BIG_ENDIAN)
            buf.put(msg.protocolVersion.toByte())
            buf.putShort(msg.messageType.toShort())
            buf.putLong(msg.messageId)
            buf.putInt(msg.payload.size)
            buf.put(msg.payload)
            return buf.array()
        }
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ProtocolMessage) return false
        return protocolVersion == other.protocolVersion &&
            messageType == other.messageType &&
            messageId == other.messageId &&
            payload.contentEquals(other.payload)
    }

    override fun hashCode(): Int {
        var result = protocolVersion
        result = 31 * result + messageType
        result = 31 * result + messageId.hashCode()
        result = 31 * result + payload.contentHashCode()
        return result
    }
}

object MessageTypes {
    const val PING = 1
    const val PONG = 2
    const val PAIR_REQUEST = 10
    const val PAIR_RESPONSE = 11
    const val AUTH_REQUEST = 20
    const val AUTH_RESPONSE = 21
    const val DEVICE_INFO = 30
    const val CONNECTION_STATUS = 40
    const val DISCONNECT = 50
    const val REVOKE_DEVICE = 60
    const val ERROR = 99
}
