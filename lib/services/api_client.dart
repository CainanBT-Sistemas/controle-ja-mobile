import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Configuração central do cliente HTTP Dio.
///
/// BaseUrl extraída de AppConstants.cs do projeto MAUI original.
/// Interceptor injeta automaticamente o token Bearer lido do flutter_secure_storage.
class ApiClient {
  static const String baseUrl =
      'http://192.168.100.103:8080/controle_ja_api/v1/';

  static const String authTokenKey = 'auth_token';

  final Dio dio;
  final FlutterSecureStorage _secureStorage;

  ApiClient({
    Dio? dio,
    FlutterSecureStorage? secureStorage,
  })  : _secureStorage = secureStorage ?? const FlutterSecureStorage(),
        dio = dio ?? Dio() {
    this.dio.options = BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 10),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    );

    this.dio.interceptors.add(_AuthInterceptor(_secureStorage));
  }
}

/// Interceptor que lê o token do flutter_secure_storage e injeta no
/// cabeçalho `Authorization: Bearer <token>`.
///
/// Rotas públicas (contendo "/auth" ou "/users/register") são ignoradas,
/// replicando o comportamento de AddAuthenticationHeaderAsync do C#.
class _AuthInterceptor extends Interceptor {
  final FlutterSecureStorage _secureStorage;

  /// Rotas que não precisam de autenticação.
  static const List<String> _publicPaths = ['/auth', '/users/register'];

  _AuthInterceptor(this._secureStorage);

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final isPublic =
        _publicPaths.any((path) => options.path.contains(path));

    if (!isPublic) {
      final token =
          await _secureStorage.read(key: ApiClient.authTokenKey);
      if (token != null && token.isNotEmpty) {
        options.headers['Authorization'] = 'Bearer $token';
      }
    }

    handler.next(options);
  }
}
