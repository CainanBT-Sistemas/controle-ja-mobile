import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../models/enums.dart';
import '../../models/transaction.dart';
import '../../providers/transactions_notifier.dart';
import '../modals/transaction_add_sheet.dart';

/// Aba de Transações — lista agrupada por dia com navegação mensal.
///
/// Migrado de TransactionsViewModel.cs / TransactionsPage.xaml.
class TransactionsTab extends ConsumerStatefulWidget {
  const TransactionsTab({super.key});

  @override
  ConsumerState<TransactionsTab> createState() => _TransactionsTabState();
}

class _TransactionsTabState extends ConsumerState<TransactionsTab> {
  static const _bgColor = Color(0xFF001524);
  static const _cardColor = Color(0xFF1E2A3A);
  static const _textColor = Color(0xFF94A3B8);
  static const _green = Color(0xFF00E676);
  static const _red = Color(0xFFFF5252);

  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref.read(transactionsListNotifierProvider.notifier).loadTransactions(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(transactionsListNotifierProvider);

    return SafeArea(
      child: Column(
        children: [
          _buildMonthNav(state),
          _buildSummaryBar(state),
          Expanded(child: _buildBody(state)),
        ],
      ),
    );
  }

  // ---- Month navigation header ----

  Widget _buildMonthNav(TransactionsListState state) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          IconButton(
            icon: const Icon(Icons.chevron_left, color: Colors.white),
            onPressed: () =>
                ref.read(transactionsListNotifierProvider.notifier).previousMonth(),
          ),
          Text(
            state.currentMonthYear,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 18,
              fontWeight: FontWeight.bold,
            ),
          ),
          IconButton(
            icon: const Icon(Icons.chevron_right, color: Colors.white),
            onPressed: () =>
                ref.read(transactionsListNotifierProvider.notifier).nextMonth(),
          ),
        ],
      ),
    );
  }

  // ---- Summary bar ----

  Widget _buildSummaryBar(TransactionsListState state) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: _cardColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          _summaryItem('Receitas', state.totalInFormatted, _green),
          _summaryItem('Despesas', state.totalOutFormatted, _red),
          _summaryItem(
            'Saldo',
            state.balanceFormatted,
            Color(state.balanceColor),
          ),
        ],
      ),
    );
  }

  Widget _summaryItem(String label, String value, Color color) {
    return Column(
      children: [
        Text(label, style: const TextStyle(color: _textColor, fontSize: 11)),
        const SizedBox(height: 4),
        Text(
          value,
          style: TextStyle(
            color: color,
            fontSize: 14,
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }

  // ---- Body ----

  Widget _buildBody(TransactionsListState state) {
    if (state.isLoading) {
      return _buildShimmer();
    }

    if (state.isEmpty) {
      return const Center(
        child: Text(
          'Nenhuma transação neste mês',
          style: TextStyle(color: _textColor, fontSize: 16),
        ),
      );
    }

    return RefreshIndicator(
      color: _green,
      onRefresh: () =>
          ref.read(transactionsListNotifierProvider.notifier).loadTransactions(),
      child: ListView.builder(
        padding: const EdgeInsets.only(top: 12, bottom: 80),
        itemCount: state.groupedTransactions.length,
        itemBuilder: (context, index) {
          final group = state.groupedTransactions[index];
          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Date header
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: Text(
                  group.dateHeader,
                  style: const TextStyle(
                    color: _textColor,
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
              // Transaction items
              ...group.transactions.map((tx) => _buildTransactionTile(tx)),
            ],
          );
        },
      ),
    );
  }

  // ---- Transaction tile ----

  Widget _buildTransactionTile(Transaction tx) {
    final isIncome = tx.type == TransactionType.RECEITA ||
        tx.type == TransactionType.TRANSFERENCIA_ENTRADA;
    final color = isIncome ? _green : _red;
    final icon = _iconForType(tx.type);

    final formattedAmount =
        NumberFormat.currency(locale: 'pt_BR', symbol: r'R$').format(tx.amount);

    return InkWell(
      onTap: () => _openEdit(tx),
      onLongPress: () => _confirmDelete(tx),
      child: Container(
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: _cardColor,
          borderRadius: BorderRadius.circular(10),
        ),
        child: Row(
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: color.withAlpha(30),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(icon, color: color, size: 22),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    tx.name,
                    style: const TextStyle(color: Colors.white, fontSize: 14),
                    overflow: TextOverflow.ellipsis,
                  ),
                  if (tx.categoryName != null)
                    Text(
                      tx.categoryName!,
                      style: const TextStyle(color: _textColor, fontSize: 11),
                    ),
                ],
              ),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  formattedAmount,
                  style: TextStyle(
                    color: color,
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                if (tx.paid)
                  const Icon(Icons.check_circle, color: _green, size: 14),
              ],
            ),
          ],
        ),
      ),
    );
  }

  IconData _iconForType(TransactionType type) {
    switch (type) {
      case TransactionType.RECEITA:
        return Icons.trending_up;
      case TransactionType.DESPESA:
        return Icons.trending_down;
      case TransactionType.TRANSFERENCIA:
      case TransactionType.TRANSFERENCIA_ENTRADA:
      case TransactionType.TRANSFERENCIA_SAIDA:
        return Icons.swap_horiz;
      case TransactionType.PAGAMENTO_FATURA:
        return Icons.credit_card;
    }
  }

  // ---- Actions ----

  Future<void> _openEdit(Transaction tx) async {
    await showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => TransactionAddSheet(transactionToEdit: tx),
    );
    ref.read(transactionsListNotifierProvider.notifier).loadTransactions();
  }

  Future<void> _confirmDelete(Transaction tx) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: _cardColor,
        title: const Text('Excluir transação', style: TextStyle(color: Colors.white)),
        content: Text(
          'Deseja excluir "${tx.name}"?',
          style: const TextStyle(color: _textColor),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Excluir', style: TextStyle(color: _red)),
          ),
        ],
      ),
    );
    if (confirmed == true && tx.id != null) {
      await ref
          .read(transactionsListNotifierProvider.notifier)
          .deleteTransaction(tx.id!);
    }
  }

  // ---- Shimmer / loading ----

  Widget _buildShimmer() {
    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: 6,
      itemBuilder: (_, __) => Container(
        margin: const EdgeInsets.only(bottom: 12),
        height: 60,
        decoration: BoxDecoration(
          color: _cardColor.withAlpha(120),
          borderRadius: BorderRadius.circular(10),
        ),
      ),
    );
  }
}
