import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/error_handler.dart';
import '../models/account.dart';
import '../services/account_service.dart';
import 'service_providers.dart';

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/// Estado do AccountsListNotifier.
///
/// Migrado de AccountsViewModel.cs — lista de contas bancárias.
class AccountsListState {
  final bool isLoading;
  final List<Account> accounts;
  final UserFriendlyError? error;

  const AccountsListState({
    this.isLoading = false,
    this.accounts = const [],
    this.error,
  });

  AccountsListState copyWith({
    bool? isLoading,
    List<Account>? accounts,
    UserFriendlyError? error,
    bool clearError = false,
  }) {
    return AccountsListState(
      isLoading: isLoading ?? this.isLoading,
      accounts: accounts ?? this.accounts,
      error: clearError ? null : (error ?? this.error),
    );
  }
}

// ---------------------------------------------------------------------------
// Notifier
// ---------------------------------------------------------------------------

/// Notifier para a lista de contas.
///
/// Migrado de AccountsViewModel.cs — carrega e exclui contas via
/// AccountService.
class AccountsListNotifier extends StateNotifier<AccountsListState> {
  final AccountService _accountService;

  AccountsListNotifier(this._accountService)
      : super(const AccountsListState());

  /// Carrega todas as contas do usuário.
  Future<void> loadAccounts() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final accounts = await _accountService.getAccounts();
      state = state.copyWith(accounts: accounts);
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  /// Exclui uma conta e recarrega a lista.
  Future<bool> deleteAccount(String id) async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _accountService.deleteAccount(id);
      if (success) {
        await loadAccounts();
      }
      return success;
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
      return false;
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  void clearError() => state = state.copyWith(clearError: true);
}

// ---------------------------------------------------------------------------
// Providers
// ---------------------------------------------------------------------------

/// Provider do AccountsListNotifier.
final accountsListNotifierProvider =
    StateNotifierProvider<AccountsListNotifier, AccountsListState>((ref) {
  final service = ref.watch(accountServiceProvider);
  return AccountsListNotifier(service);
});
