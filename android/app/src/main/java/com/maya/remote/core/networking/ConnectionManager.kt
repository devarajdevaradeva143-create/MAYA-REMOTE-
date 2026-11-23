package com.maya.remote.core.networking

import com.maya.remote.core.common.ConnectionState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow

class ConnectionManager(private val transport: TcpTransport) {
    private val _state = MutableStateFlow(ConnectionState.DISCONNECTED)
    val state: StateFlow<ConnectionState> = _state
    fun setState(s: ConnectionState) { _state.value = s }
}
EOF