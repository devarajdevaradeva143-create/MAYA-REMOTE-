package com.maya.remote

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.maya.remote.navigation.Routes
import com.maya.remote.ui.screens.*

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                Surface {
                    val nav = rememberNavController()
                    NavHost(navController = nav, startDestination = Routes.SPLASH) {
                        composable(Routes.SPLASH) { SplashScreen() }
                        composable(Routes.WELCOME) { WelcomeScreen() }
                        composable(Routes.PROFILE_SETUP) { ProfileSetupScreen() }
                        composable(Routes.HOME) { HomeScreen() }
                        composable(Routes.PAIR) { PairScreen() }
                        composable(Routes.PAIR_QR) { PairQrScreen() }
                        composable(Routes.ENTER_CODE) { EnterCodeScreen() }
                        composable(Routes.DEVICE_APPROVAL) { DeviceApprovalScreen() }
                        composable(Routes.CONNECTED_DEVICES) { ConnectedDevicesScreen() }
                        composable(Routes.CONNECTION_STATUS) { ConnectionStatusScreen() }
                        composable(Routes.REMOTE_MOUSE) { RemoteMouseScreen() }
                        composable(Routes.REMOTE_KEYBOARD) { RemoteKeyboardScreen() }
                        composable(Routes.SCREEN_VIEWER) { ScreenViewerScreen() }
                        composable(Routes.SECURITY_SETTINGS) { SecuritySettingsScreen() }
                        composable(Routes.APP_SETTINGS) { AppSettingsScreen() }
                    }
                }
            }
        }
    }
}
EOF