import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/error_handler.dart';
import '../models/credit_card.dart';
import '../services/credit_card_service.dart';
import 'service_providers.dart';

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/// Estado do CreditCardsListNotifier.
///
/// Migrado de CreditCardsViewModel.cs — lista de cartões de crédito.
class CreditCardsListState {
  final bool isLoading;
  final List<CreditCard> cards;
  final UserFriendlyError? error;

  const CreditCardsListState({
    this.isLoading = false,
    this.cards = const [],
    this.error,
  });

  CreditCardsListState copyWith({
    bool? isLoading,
    List<CreditCard>? cards,
    UserFriendlyError? error,
    bool clearError = false,
  }) {
    return CreditCardsListState(
      isLoading: isLoading ?? this.isLoading,
      cards: cards ?? this.cards,
      error: clearError ? null : (error ?? this.error),
    );
  }
}

// ---------------------------------------------------------------------------
// Notifier
// ---------------------------------------------------------------------------

/// Notifier para a lista de cartões de crédito.
///
/// Migrado de CreditCardsViewModel.cs — carrega e exclui cartões via
/// CreditCardService.
class CreditCardsListNotifier extends StateNotifier<CreditCardsListState> {
  final CreditCardService _creditCardService;

  CreditCardsListNotifier(this._creditCardService)
      : super(const CreditCardsListState());

  /// Carrega todos os cartões de crédito do usuário.
  Future<void> loadCards() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final cards = await _creditCardService.getCreditCards();
      state = state.copyWith(cards: cards);
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  /// Exclui um cartão e recarrega a lista.
  Future<bool> deleteCard(String id) async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _creditCardService.deleteCreditCard(id);
      if (success) {
        await loadCards();
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

/// Provider do CreditCardsListNotifier.
final creditCardsListNotifierProvider =
    StateNotifierProvider<CreditCardsListNotifier, CreditCardsListState>(
        (ref) {
  final service = ref.watch(creditCardServiceProvider);
  return CreditCardsListNotifier(service);
});
