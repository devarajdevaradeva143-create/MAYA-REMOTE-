package com.maya.remote.core.result

sealed class Result<out T> {
    data class Success<T>(val data: T) : Result<T>()
    data class Failure(val error: MayaError) : Result<Nothing>()
}

data class MayaError(
    val code: String,
    val message: String,
    val technicalDetail: String? = null
)
