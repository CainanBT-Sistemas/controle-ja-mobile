import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../core/error_handler.dart';
import '../models/enums.dart';
import '../models/transaction.dart';
import '../models/transaction_group.dart';
import '../services/transaction_service.dart';
import 'service_providers.dart';

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/// Estado do TransactionsListNotifier.
///
/// Migrado de TransactionsViewModel.cs — lista agrupada de transações por dia,
/// totais formatados e navegação por mês.
class TransactionsListState {
  final bool isLoading;
  final List<TransactionGroup> groupedTransactions;
  final DateTime currentDate;
  final String currentMonthYear;
  final String totalInFormatted;
  final String totalOutFormatted;
  final String balanceFormatted;
  final int balanceColor;
  final bool isEmpty;
  final UserFriendlyError? error;

  TransactionsListState({
    this.isLoading = false,
    this.groupedTransactions = const [],
    DateTime? currentDate,
    this.currentMonthYear = '',
    this.totalInFormatted = 'R\$ 0,00',
    this.totalOutFormatted = 'R\$ 0,00',
    this.balanceFormatted = 'R\$ 0,00',
    this.balanceColor = 0xFF4CAF50,
    this.isEmpty = true,
    this.error,
  }) : currentDate = currentDate ?? DateTime.now();

  TransactionsListState copyWith({
    bool? isLoading,
    List<TransactionGroup>? groupedTransactions,
    DateTime? currentDate,
    String? currentMonthYear,
    String? totalInFormatted,
    String? totalOutFormatted,
    String? balanceFormatted,
    int? balanceColor,
    bool? isEmpty,
    UserFriendlyError? error,
    bool clearError = false,
  }) {
    return TransactionsListState(
      isLoading: isLoading ?? this.isLoading,
      groupedTransactions:
          groupedTransactions ?? this.groupedTransactions,
      currentDate: currentDate ?? this.currentDate,
      currentMonthYear: currentMonthYear ?? this.currentMonthYear,
      totalInFormatted: totalInFormatted ?? this.totalInFormatted,
      totalOutFormatted: totalOutFormatted ?? this.totalOutFormatted,
      balanceFormatted: balanceFormatted ?? this.balanceFormatted,
      balanceColor: balanceColor ?? this.balanceColor,
      isEmpty: isEmpty ?? this.isEmpty,
      error: clearError ? null : (error ?? this.error),
    );
  }
}

// ---------------------------------------------------------------------------
// Notifier
// ---------------------------------------------------------------------------

/// Notifier para a lista de transações.
///
/// Migrado de TransactionsViewModel.cs — carrega transações do mês,
/// agrupa por dia, calcula totais e formata em pt_BR.
class TransactionsListNotifier
    extends StateNotifier<TransactionsListState> {
  final TransactionService _transactionService;

  // Formatadores pt_BR.
  static final _currencyFormat =
      NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$');
  static final _dateFormat = DateFormat("dd 'de' MMMM", 'pt_BR');
  static final _monthYearFormat = DateFormat("MMMM 'de' yyyy", 'pt_BR');

  TransactionsListNotifier(this._transactionService)
      : super(TransactionsListState()) {
    _updateMonthYear();
  }

  /// Carrega as transações do mês corrente.
  Future<void> loadTransactions() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final firstDay = DateTime(
          state.currentDate.year, state.currentDate.month, 1);
      final lastDay = DateTime(
              state.currentDate.year, state.currentDate.month + 1, 1)
          .subtract(const Duration(seconds: 1));

      final start = firstDay.millisecondsSinceEpoch;
      final end = lastDay.millisecondsSinceEpoch;

      final transactions =
          await _transactionService.getTransactions(start: start, end: end);

      _applyTransactions(transactions);
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  /// Avança um mês e recarrega.
  Future<void> nextMonth() async {
    final next = DateTime(
        state.currentDate.year, state.currentDate.month + 1, 1);
    state = state.copyWith(currentDate: next);
    _updateMonthYear();
    await loadTransactions();
  }

  /// Retrocede um mês e recarrega.
  Future<void> previousMonth() async {
    final prev = DateTime(
        state.currentDate.year, state.currentDate.month - 1, 1);
    state = state.copyWith(currentDate: prev);
    _updateMonthYear();
    await loadTransactions();
  }

  /// Exclui uma transação e recarrega a lista.
  Future<bool> deleteTransaction(String id) async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _transactionService.deleteTransaction(id);
      if (success) {
        await loadTransactions();
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

  // --- Privados ---

  void _updateMonthYear() {
    state = state.copyWith(
      currentMonthYear: _monthYearFormat.format(state.currentDate),
    );
  }

  /// Agrupa transações por dia e calcula totais.
  void _applyTransactions(List<Transaction> transactions) {
    // Agrupa por dia.
    final Map<String, List<Transaction>> grouped = {};
    for (final t in transactions) {
      final dateTime =
          DateTime.fromMillisecondsSinceEpoch(t.date);
      final key = _dateFormat.format(dateTime);
      grouped.putIfAbsent(key, () => []).add(t);
    }

    final groups = grouped.entries
        .map((e) => TransactionGroup(
              dateHeader: e.key,
              transactions: e.value,
            ))
        .toList();

    // Calcula totais.
    double totalIn = 0;
    double totalOut = 0;
    for (final t in transactions) {
      if (t.type == TransactionType.RECEITA ||
          t.type == TransactionType.TRANSFERENCIA_ENTRADA) {
        totalIn += t.amount;
      } else if (t.type == TransactionType.DESPESA ||
          t.type == TransactionType.TRANSFERENCIA_SAIDA ||
          t.type == TransactionType.PAGAMENTO_FATURA) {
        totalOut += t.amount;
      }
    }

    final balance = totalIn - totalOut;
    // Verde se positivo, vermelho se negativo.
    final color = balance >= 0 ? 0xFF4CAF50 : 0xFFF44336;

    state = state.copyWith(
      groupedTransactions: groups,
      totalInFormatted: _currencyFormat.format(totalIn),
      totalOutFormatted: _currencyFormat.format(totalOut),
      balanceFormatted: _currencyFormat.format(balance),
      balanceColor: color,
      isEmpty: transactions.isEmpty,
    );
  }
}

// ---------------------------------------------------------------------------
// Providers
// ---------------------------------------------------------------------------

/// Provider do TransactionsListNotifier.
final transactionsListNotifierProvider = StateNotifierProvider<
    TransactionsListNotifier, TransactionsListState>((ref) {
  final service = ref.watch(transactionServiceProvider);
  return TransactionsListNotifier(service);
});
