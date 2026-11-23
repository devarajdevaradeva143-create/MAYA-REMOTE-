package com.maya.remote.core.networking

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test
import java.nio.ByteBuffer
import java.nio.ByteOrder

class ProtocolDecodeTest {
    @Test
    fun decodeValid() {
        val msg = ProtocolMessage(1, 1, 42L, byteArrayOf(1,2,3))
        val buf = ByteBuffer.wrap(ProtocolMessage.encode(msg)).order(ByteOrder.BIG_ENDIAN)
        val dec = ProtocolMessage.decode(buf)
        assertEquals(msg, dec)
    }
    @Test
    fun decodeInvalidVersion() {
        val buf = ByteBuffer.allocate(15).order(ByteOrder.BIG_ENDIAN)
        buf.put(9.toByte()); buf.putShort(1); buf.putLong(1L); buf.putInt(0)
        buf.flip()
        assertNull(ProtocolMessage.decode(buf))
    }
}
EOF