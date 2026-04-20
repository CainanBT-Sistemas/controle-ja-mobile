import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/theme.dart';
import '../../providers/credit_cards_notifier.dart';
import 'credit_card_add_sheet.dart';

class ManageCreditCardsSheet extends ConsumerStatefulWidget {
  const ManageCreditCardsSheet({super.key});

  @override
  ConsumerState<ManageCreditCardsSheet> createState() =>
      _ManageCreditCardsSheetState();
}

class _ManageCreditCardsSheetState
    extends ConsumerState<ManageCreditCardsSheet> {
  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref.read(creditCardsListNotifierProvider.notifier).loadCards(),
    );
  }

  String _formatCurrency(double value) {
    return NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$').format(value);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(creditCardsListNotifierProvider);

    return Container(
      constraints:
          BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.85),
      decoration: const BoxDecoration(
        color: Color(0xFF0A0F17),
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // Drag handle
          Container(
            margin: const EdgeInsets.only(top: 12),
            width: 40,
            height: 4,
            decoration: BoxDecoration(
              color: const Color(0xFF334155),
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 12, 8),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Cartões de Crédito',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                IconButton(
                  icon: const Icon(
                    Icons.add_circle_outline,
                    color: Color(0xFF00E676),
                    size: 28,
                  ),
                  onPressed: () => _openAddSheet(context),
                ),
              ],
            ),
          ),
          const Divider(color: Color(0xFF1E293B), height: 1),
          // Content
          Flexible(
            child: state.isLoading
                ? const Center(
                    child: Padding(
                      padding: EdgeInsets.all(32),
                      child: CircularProgressIndicator(
                        color: Color(0xFF00E676),
                      ),
                    ),
                  )
                : state.cards.isEmpty
                    ? const Padding(
                        padding: EdgeInsets.all(48),
                        child: Text(
                          'Nenhum cartão cadastrado',
                          style: TextStyle(
                            color: Color(0xFF94A3B8),
                            fontSize: 16,
                          ),
                        ),
                      )
                    : ListView.builder(
                        shrinkWrap: true,
                        itemCount: state.cards.length,
                        padding: const EdgeInsets.symmetric(vertical: 8),
                        itemBuilder: (context, index) {
                          final card = state.cards[index];
                          return InkWell(
                            onTap: () =>
                                _openAddSheet(context, cardId: card.id),
                            child: Padding(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 16,
                                vertical: 8,
                              ),
                              child: Container(
                                padding: const EdgeInsets.all(16),
                                decoration: BoxDecoration(
                                  color: const Color(0xFF1E2A3A),
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    // Card name + icon
                                    Row(
                                      children: [
                                        CircleAvatar(
                                          backgroundColor:
                                              AppColors.fromHex(card.color),
                                          child: Icon(
                                            AppMaterialIcons.fromName(
                                              card.icon,
                                            ),
                                            color: Colors.white,
                                            size: 20,
                                          ),
                                        ),
                                        const SizedBox(width: 12),
                                        Expanded(
                                          child: Text(
                                            card.name,
                                            style: const TextStyle(
                                              color: Colors.white,
                                              fontSize: 16,
                                              fontWeight: FontWeight.w600,
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 12),
                                    // Limit progress bar
                                    ClipRRect(
                                      borderRadius: BorderRadius.circular(4),
                                      child: LinearProgressIndicator(
                                        value: card.limitProgress.clamp(
                                          0.0,
                                          1.0,
                                        ),
                                        minHeight: 6,
                                        backgroundColor:
                                            const Color(0xFF334155),
                                        valueColor:
                                            AlwaysStoppedAnimation<Color>(
                                          card.limitProgress > 0.8
                                              ? const Color(0xFFFF5252)
                                              : const Color(0xFF00E676),
                                        ),
                                      ),
                                    ),
                                    const SizedBox(height: 8),
                                    // Used / Total
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        Text(
                                          'Usado: ${_formatCurrency(card.usedAmount)}',
                                          style: const TextStyle(
                                            color: Color(0xFF94A3B8),
                                            fontSize: 12,
                                          ),
                                        ),
                                        Text(
                                          'Limite: ${_formatCurrency(card.totalLimit)}',
                                          style: const TextStyle(
                                            color: Color(0xFF94A3B8),
                                            fontSize: 12,
                                          ),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 8),
                                    // Close day / Best day
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        Text(
                                          'Fecha dia ${card.closeDay}',
                                          style: const TextStyle(
                                            color: Color(0xFF94A3B8),
                                            fontSize: 12,
                                          ),
                                        ),
                                        Text(
                                          'Melhor compra dia ${card.bestDay}',
                                          style: const TextStyle(
                                            color: Color(0xFF94A3B8),
                                            fontSize: 12,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }

  void _openAddSheet(BuildContext context, {String? cardId}) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => CreditCardAddSheet(cardId: cardId),
    ).then(
      (_) => ref.read(creditCardsListNotifierProvider.notifier).loadCards(),
    );
  }
}

/// Helper to show this sheet.
void showManageCreditCardsSheet(BuildContext context) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.transparent,
    builder: (_) => const ManageCreditCardsSheet(),
  );
}
