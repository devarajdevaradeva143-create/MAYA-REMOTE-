package com.maya.remote.di

import com.maya.remote.core.dispatchers.DefaultDispatcherProvider
import com.maya.remote.core.dispatchers.DispatcherProvider
import com.maya.remote.core.security.DeviceIdentity
import com.maya.remote.core.security.KeyStoreWrapper
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object AppModule {
    @Provides @Singleton fun provideDispatcherProvider(): DispatcherProvider = DefaultDispatcherProvider()
    @Provides @Singleton fun provideKeyStore(): KeyStoreWrapper = KeyStoreWrapper()
    @Provides @Singleton fun provideDeviceIdentity(ks: KeyStoreWrapper): DeviceIdentity = DeviceIdentity(ks)
}
EOF