package com.cainanbt.controleja.util

import retrofit2.HttpException
import java.net.ConnectException
import java.net.SocketTimeoutException
import java.net.UnknownHostException

data class UserFriendlyError(
    val title: String,
    val message: String
)

object ErrorHandler {
    fun parse(ex: Throwable): UserFriendlyError {
        return when (ex) {
            is UnknownHostException, is ConnectException -> UserFriendlyError(
                title = "Sem Conexão",
                message = "Não foi possível conectar ao servidor. Verifique sua conexão."
            )
            is SocketTimeoutException -> UserFriendlyError(
                title = "Tempo Esgotado",
                message = "O servidor demorou para responder. Tente novamente."
            )
            is HttpException -> {
                when (ex.code()) {
                    401 -> UserFriendlyError("Não Autorizado", "Sessão expirada. Faça login novamente.")
                    403 -> UserFriendlyError("Acesso Negado", "Você não tem permissão para esta ação.")
                    404 -> UserFriendlyError("Não Encontrado", "O recurso solicitado não foi encontrado.")
                    500 -> UserFriendlyError("Erro no Servidor", "Ocorreu um erro interno. Tente novamente.")
                    else -> UserFriendlyError("Erro", "Ocorreu um erro (${ex.code()}). Tente novamente.")
                }
            }
            else -> UserFriendlyError(
                title = "Ops! Algo deu errado",
                message = ex.message ?: "Erro desconhecido. Tente novamente."
            )
        }
    }
}
