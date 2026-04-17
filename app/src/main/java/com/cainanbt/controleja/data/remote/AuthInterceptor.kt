package com.cainanbt.controleja.data.remote

import com.cainanbt.controleja.data.local.TokenManager
import okhttp3.Interceptor
import okhttp3.Response
import javax.inject.Inject

class AuthInterceptor @Inject constructor(
    private val tokenManager: TokenManager
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val path = request.url.encodedPath

        // Skip auth for public endpoints
        if (path.contains("/auth") || path.contains("/users/register")) {
            return chain.proceed(request)
        }

        val token = tokenManager.getAccessToken()
        return if (token != null) {
            val authenticatedRequest = request.newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
            chain.proceed(authenticatedRequest)
        } else {
            chain.proceed(request)
        }
    }
}
