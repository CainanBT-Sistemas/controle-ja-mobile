import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/error_handler.dart';
import '../services/auth_service.dart';
import 'auth_providers.dart';

/// Estado do RegisterNotifier.
///
/// Migrado de RegisterViewModel.cs — campos Name, Email, Password, ConfirmPassword.
class RegisterState {
  final String name;
  final String email;
  final String password;
  final String confirmPassword;
  final bool isLoading;
  final UserFriendlyError? error;
  final bool registerSuccess;

  const RegisterState({
    this.name = '',
    this.email = '',
    this.password = '',
    this.confirmPassword = '',
    this.isLoading = false,
    this.error,
    this.registerSuccess = false,
  });

  RegisterState copyWith({
    String? name,
    String? email,
    String? password,
    String? confirmPassword,
    bool? isLoading,
    UserFriendlyError? error,
    bool? registerSuccess,
    bool clearError = false,
  }) {
    return RegisterState(
      name: name ?? this.name,
      email: email ?? this.email,
      password: password ?? this.password,
      confirmPassword: confirmPassword ?? this.confirmPassword,
      isLoading: isLoading ?? this.isLoading,
      error: clearError ? null : (error ?? this.error),
      registerSuccess: registerSuccess ?? this.registerSuccess,
    );
  }
}

/// Notifier para a tela de registro.
///
/// Migrado de RegisterViewModel.cs — executa registro via AuthService.
class RegisterNotifier extends StateNotifier<RegisterState> {
  final AuthService _authService;

  RegisterNotifier(this._authService) : super(const RegisterState());

  void setName(String value) =>
      state = state.copyWith(name: value, clearError: true);

  void setEmail(String value) =>
      state = state.copyWith(email: value, clearError: true);

  void setPassword(String value) =>
      state = state.copyWith(password: value, clearError: true);

  void setConfirmPassword(String value) =>
      state = state.copyWith(confirmPassword: value, clearError: true);

  /// Executa o registro. Retorna true em caso de sucesso.
  Future<bool> register() async {
    if (state.name.trim().isEmpty ||
        state.email.trim().isEmpty ||
        state.password.trim().isEmpty) {
      state = state.copyWith(
        error: const UserFriendlyError(
          title: 'Atenção',
          message: 'Preencha todos os campos.',
        ),
      );
      return false;
    }

    if (state.password != state.confirmPassword) {
      state = state.copyWith(
        error: const UserFriendlyError(
          title: 'Erro',
          message: 'As senhas não coincidem.',
        ),
      );
      return false;
    }

    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _authService.register(
        state.name.trim(),
        state.email.trim(),
        state.password,
      );
      state = state.copyWith(
        isLoading: false,
        registerSuccess: success,
      );
      if (!success) {
        state = state.copyWith(
          error: const UserFriendlyError(
            title: 'Falha no Registro',
            message: 'Não foi possível criar a conta. Verifique os dados informados.',
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

/// Provider para o RegisterNotifier.
final registerNotifierProvider =
    StateNotifierProvider.autoDispose<RegisterNotifier, RegisterState>((ref) {
  final authService = ref.watch(authServiceProvider);
  return RegisterNotifier(authService);
});
