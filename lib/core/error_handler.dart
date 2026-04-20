import 'package:dio/dio.dart';

/// Representação de um erro amigável para o usuário.
///
/// Baseado em UserFriendlyError + ErrorHandler.cs do projeto MAUI.
class UserFriendlyError {
  final String title;
  final String message;

  const UserFriendlyError({required this.title, required this.message});

  @override
  String toString() => 'UserFriendlyError(title: $title, message: $message)';
}

/// Modelo para respostas de erro padronizadas da API.
///
/// Baseado em ApiErrorResponse.cs do projeto MAUI.
class ApiErrorResponse {
  final int code;
  final String message;
  final String title;

  const ApiErrorResponse({
    this.code = 0,
    this.message = '',
    this.title = '',
  });

  factory ApiErrorResponse.fromJson(Map<String, dynamic> json) {
    return ApiErrorResponse(
      code: json['code'] as int? ?? 0,
      message: json['message'] as String? ?? '',
      title: json['title'] as String? ?? '',
    );
  }
}

/// Utilitário estático para tratamento padronizado de erros HTTP.
///
/// Converte exceções do Dio em mensagens amigáveis para o usuário,
/// replicando o comportamento de ErrorHandler.cs do projeto MAUI.
class ErrorHandler {
  ErrorHandler._();

  /// Analisa uma exceção e retorna um [UserFriendlyError] amigável.
  static UserFriendlyError parse(dynamic error) {
    // 1. Erros do Dio (requisição HTTP)
    if (error is DioException) {
      return _parseDioException(error);
    }

    // 2. Erro genérico
    final message = error is Exception ? error.toString() : '$error';
    return UserFriendlyError(
      title: 'Ops! Algo deu errado',
      message:
          'Ocorreu um erro inesperado.\nDetalhe técnico: $message\n\nTente reiniciar o aplicativo.',
    );
  }

  static UserFriendlyError _parseDioException(DioException error) {
    switch (error.type) {
      // Timeout
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return const UserFriendlyError(
          title: 'Tempo Esgotado',
          message:
              'O servidor demorou muito para responder.\n\nSua conexão pode estar lenta. Tente novamente.',
        );

      // Sem conexão / DNS
      case DioExceptionType.connectionError:
        return const UserFriendlyError(
          title: 'Sem Conexão',
          message:
              'Parece que você está offline.\n\nVerifique seu Wi-Fi ou dados móveis e tente novamente.',
        );

      // Resposta HTTP com erro
      case DioExceptionType.badResponse:
        return _parseStatusCode(error.response?.statusCode, error.response);

      // Cancelamento
      case DioExceptionType.cancel:
        return const UserFriendlyError(
          title: 'Requisição Cancelada',
          message: 'A operação foi cancelada.',
        );

      // Outros
      default:
        return const UserFriendlyError(
          title: 'Servidor Indisponível',
          message:
              'Não conseguimos conectar ao servidor do Controle Já.\n\nVerifique se não há bloqueios na sua rede ou tente mais tarde.',
        );
    }
  }

  static UserFriendlyError _parseStatusCode(
      int? statusCode, Response<dynamic>? response) {
    switch (statusCode) {
      case 400:
        final body = _tryExtractMessage(response);
        return UserFriendlyError(
          title: 'Dados Inválidos',
          message: body ?? 'Verifique os dados informados e tente novamente.',
        );

      case 401:
        return const UserFriendlyError(
          title: 'Acesso Negado',
          message:
              'Sua sessão expirou ou suas credenciais são inválidas.\n\nPor favor, faça login novamente.',
        );

      case 403:
        return const UserFriendlyError(
          title: 'Acesso Negado',
          message: 'Você não tem permissão para realizar esta ação.',
        );

      case 404:
        return const UserFriendlyError(
          title: 'Não Encontrado',
          message:
              'O recurso que você tentou acessar não existe ou foi movido.',
        );

      case 409:
        final body = _tryExtractMessage(response);
        return UserFriendlyError(
          title: 'Conflito',
          message: body ?? 'Este registro já existe ou está em conflito.',
        );

      case 500:
      case 503:
        return const UserFriendlyError(
          title: 'Problema no Servidor',
          message:
              'Nossos servidores estão passando por instabilidades no momento.\n\nPor favor, aguarde alguns minutos e tente novamente.',
        );

      default:
        return UserFriendlyError(
          title: 'Erro',
          message: 'Ocorreu um erro inesperado (código $statusCode).',
        );
    }
  }

  /// Tenta extrair a mensagem de erro do corpo da resposta.
  static String? _tryExtractMessage(Response<dynamic>? response) {
    try {
      final data = response?.data;
      if (data is Map<String, dynamic>) {
        // Tenta campo "message" ou "title"
        final msg = data['message'] as String?;
        if (msg != null && msg.isNotEmpty) {
          return msg.split(', ').where((e) => e.isNotEmpty).join('\n');
        }
        final title = data['title'] as String?;
        if (title != null && title.isNotEmpty) return title;
      }
      if (data is String && data.isNotEmpty) return data;
    } catch (_) {}
    return null;
  }
}
