package com.maya.remote.core.security

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import java.security.KeyStore
import javax.crypto.KeyGenerator

class KeyStoreWrapper {
    private val keyStore = KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
    fun generateKey(alias: String): Boolean {
        if (keyStore.containsAlias(alias)) return true
        val kg = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore")
        kg.init(KeyGenParameterSpec.Builder(alias, KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
            .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
            .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).build())
        kg.generateKey()
        return true
    }
    fun hasKey(alias: String): Boolean = keyStore.containsAlias(alias)
}

class DeviceIdentity(private val keyStore: KeyStoreWrapper) {
    val alias = "maya_device_key"
    fun ensureIdentity() = keyStore.generateKey(alias)
    fun hasIdentity() = keyStore.hasKey(alias)
}
EOF