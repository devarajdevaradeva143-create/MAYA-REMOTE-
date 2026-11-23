package com.maya.remote.core.logging

object AppLogger {
    fun d(tag: String, msg: String) = android.util.Log.d(tag, msg)
    fun e(tag: String, msg: String, t: Throwable? = null) = android.util.Log.e(tag, msg, t)
}
EOF