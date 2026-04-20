import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../models/user_models.dart';
import 'api_client.dart';

/// Serviço de autenticação.
///
/// Migrado de AuthService.cs — mantém todas as rotas da API originais.
class AuthService {
  final ApiClient _apiClient;
  final FlutterSecureStorage _secureStorage;

  AuthService({
    required ApiClient apiClient,
    FlutterSecureStorage? secureStorage,
  })  : _apiClient = apiClient,
        _secureStorage = secureStorage ?? const FlutterSecureStorage();

  Dio get _dio => _apiClient.dio;

  // --- LOGIN & AUTO-LOGIN ---

  Future<bool> login(String email, String password) async {
    try {
      final response = await _dio.post(
        'auth',
        data: {'email': email, 'password': password},
      );
      if (response.data != null) {
        final userResponse = UserResponse.fromJson(
          response.data is String
              ? jsonDecode(response.data as String) as Map<String, dynamic>
              : response.data as Map<String, dynamic>,
        );
        if (userResponse.id.isNotEmpty) {
          await saveAuthToken(
            token: userResponse.tokens?.accessToken,
            refreshToken: userResponse.tokens?.refreshToken,
            username: userResponse.username,
            email: userResponse.email,
            userId: userResponse.id,
          );
          return true;
        }
      }
      return false;
    } on DioException catch (_) {
      return false;
    } catch (e) {
      return false;
    }
  }

  Future<bool> loginWithToken(String token) async {
    try {
      final response = await _dio.post(
        'auth/auto-login',
        data: {'token': token},
      );
      if (response.data != null) {
        final userResponse = UserResponse.fromJson(
          response.data is String
              ? jsonDecode(response.data as String) as Map<String, dynamic>
              : response.data as Map<String, dynamic>,
        );
        if (userResponse.tokens != null &&
            userResponse.tokens!.accessToken.isNotEmpty) {
          await saveAuthToken(
            token: userResponse.tokens!.accessToken,
            refreshToken: userResponse.tokens!.refreshToken,
            username: userResponse.username,
            email: userResponse.email,
            userId: userResponse.id,
          );
          return true;
        }
      }
      return false;
    } on DioException catch (e) {
      if (e.response?.data?.toString().contains('Token inválido') ?? false) {
        await saveAuthToken();
      }
      return false;
    } catch (_) {
      return false;
    }
  }

  // --- REGISTRO ---

  Future<bool> register(
      String username, String email, String password) async {
    try {
      final response = await _dio.post(
        'users/register',
        data: {
          'username': username,
          'email': email,
          'password': password,
        },
      );
      if (response.data != null) {
        final raw = response.data is String
            ? response.data as String
            : jsonEncode(response.data);
        // Verifica erros de validação retornados como texto
        if (raw.contains('A senha deve ter no mínimo') ||
            raw.contains('O nome de usuário é obrigatório') ||
            raw.contains('Formato de email inválido') ||
            raw.contains('já está em uso')) {
          return false;
        }
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        final userResponse = UserResponse.fromJson(parsed);
        return userResponse.id.isNotEmpty;
      }
      return false;
    } catch (_) {
      return false;
    }
  }

  // --- GESTÃO DE PERFIL, SENHA E EXCLUSÃO ---

  Future<bool> changePassword(
      String currentPassword, String newPassword) async {
    try {
      final response = await _dio.put(
        'users/change-password',
        data: {
          'currentPassword': currentPassword,
          'newPassword': newPassword,
        },
      );
      final body = response.data?.toString() ?? '';
      return body.toLowerCase().contains('sucesso');
    } catch (_) {
      return false;
    }
  }

  Future<bool> updateProfile(String newUsername) async {
    try {
      final response = await _dio.put(
        'users/profile',
        data: {'username': newUsername},
      );
      if (response.data != null) {
        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('UserName', newUsername);
        return true;
      }
      return false;
    } catch (_) {
      return false;
    }
  }

  Future<bool> deleteAccount() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final userId = prefs.getString('UserId') ?? '';
      if (userId.isEmpty) return false;
      await _dio.delete('users/$userId');
      return true;
    } catch (_) {
      return false;
    }
  }

  Future<bool> resetDataUser() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final userId = prefs.getString('UserId') ?? '';
      if (userId.isEmpty) return false;
      final response = await _dio.get('users/reset/$userId');
      return response.data != null;
    } catch (_) {
      return false;
    }
  }

  // --- PERSISTÊNCIA DE TOKENS ---

  Future<void> saveAuthToken({
    String? token,
    String? refreshToken,
    String? username,
    String? email,
    String? userId,
  }) async {
    if (token == null) {
      await _secureStorage.delete(key: 'auth_token');
    } else {
      await _secureStorage.write(key: 'auth_token', value: token);
    }

    if (refreshToken == null) {
      await _secureStorage.delete(key: 'refresh_token');
    } else {
      await _secureStorage.write(key: 'refresh_token', value: refreshToken);
    }

    final prefs = await SharedPreferences.getInstance();

    if (username == null) {
      await prefs.remove('UserName');
    } else {
      await prefs.setString('UserName', username);
    }

    if (email == null) {
      await prefs.remove('UserEmail');
    } else {
      await prefs.setString('UserEmail', email);
    }

    if (userId == null) {
      await prefs.remove('UserId');
    } else {
      await prefs.setString('UserId', userId);
    }
  }

  /// Lê o token armazenado para uso em auto-login.
  Future<String?> getStoredToken() async {
    return await _secureStorage.read(key: 'auth_token');
  }

  /// Lê o refresh token armazenado.
  Future<String?> getStoredRefreshToken() async {
    return await _secureStorage.read(key: 'refresh_token');
  }
}
