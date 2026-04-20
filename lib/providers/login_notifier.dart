import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/error_handler.dart';
import '../services/auth_service.dart';
import 'auth_providers.dart';

/// Estado do LoginNotifier.
///
/// Migrado de LoginViewModel.cs — campos Email, Password, IsLoading.
class LoginState {
  final String email;
  final String password;
  final bool isLoading;
  final UserFriendlyError? error;
  final bool loginSuccess;

  const LoginState({
    this.email = '',
    this.password = '',
    this.isLoading = false,
    this.error,
    this.loginSuccess = false,
  });

  LoginState copyWith({
    String? email,
    String? password,
    bool? isLoading,
    UserFriendlyError? error,
    bool? loginSuccess,
    bool clearError = false,
  }) {
    return LoginState(
      email: email ?? this.email,
      password: password ?? this.password,
      isLoading: isLoading ?? this.isLoading,
      error: clearError ? null : (error ?? this.error),
      loginSuccess: loginSuccess ?? this.loginSuccess,
    );
  }
}

/// Notifier para a tela de login.
///
/// Migrado de LoginViewModel.cs — executa login via AuthService.
class LoginNotifier extends StateNotifier<LoginState> {
  final AuthService _authService;

  LoginNotifier(this._authService) : super(const LoginState());

  void setEmail(String value) =>
      state = state.copyWith(email: value, clearError: true);

  void setPassword(String value) =>
      state = state.copyWith(password: value, clearError: true);

  /// Executa o login. Retorna true em caso de sucesso.
  Future<bool> login() async {
    if (state.email.trim().isEmpty || state.password.trim().isEmpty) {
      state = state.copyWith(
        error: const UserFriendlyError(
          title: 'Ops',
          message: 'Preencha e-mail e senha',
        ),
      );
      return false;
    }

    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _authService.login(
        state.email.trim(),
        state.password,
      );
      state = state.copyWith(isLoading: false, loginSuccess: success);
      if (!success) {
        state = state.copyWith(
          error: const UserFriendlyError(
            title: 'Falha no Login',
            message: 'E-mail ou senha incorretos.',
          ),
        );
      }
      return success;
    } catch (e) {
      final friendlyError = ErrorHandler.parse(e);
      state = state.copyWith(isLoading: false, error: friendlyError);
      return false;
    }
  }

  void clearError() => state = state.copyWith(clearError: true);
}

/// Provider para o LoginNotifier.
final loginNotifierProvider =
    StateNotifierProvider.autoDispose<LoginNotifier, LoginState>((ref) {
  final authService = ref.watch(authServiceProvider);
  return LoginNotifier(authService);
});
