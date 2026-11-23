package com.maya.remote.core.common

enum class ConnectionState {
    DISCONNECTED, DISCOVERING, PAIRING, AUTHENTICATING,
    CONNECTING, CONNECTED, RECONNECTING, FAILED, REVOKED;

    fun canTransitionTo(next: ConnectionState): Boolean = when (this) {
        DISCONNECTED -> next in setOf(DISCOVERING, PAIRING, CONNECTING)
        DISCOVERING -> next in setOf(DISCONNECTED, PAIRING)
        PAIRING -> next in setOf(DISCONNECTED, AUTHENTICATING, FAILED)
        AUTHENTICATING -> next in setOf(CONNECTING, FAILED, DISCONNECTED)
        CONNECTING -> next in setOf(CONNECTED, FAILED, DISCONNECTED)
        CONNECTED -> next in setOf(RECONNECTING, DISCONNECTED, REVOKED, FAILED)
        RECONNECTING -> next in setOf(CONNECTED, FAILED, DISCONNECTED)
        FAILED -> next in setOf(DISCONNECTED, DISCOVERING, PAIRING)
        REVOKED -> next in setOf(DISCONNECTED)
    }
}
