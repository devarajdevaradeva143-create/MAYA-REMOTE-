package com.maya.remote.core.networking

interface TcpTransport {
    suspend fun connect(host: String, port: Int)
    suspend fun send(msg: ProtocolMessage)
    suspend fun receive(): ProtocolMessage?
    fun disconnect()
}
EOF