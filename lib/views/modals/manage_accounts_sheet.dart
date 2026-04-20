import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/theme.dart';
import '../../providers/accounts_notifier.dart';
import 'account_add_sheet.dart';

class ManageAccountsSheet extends ConsumerStatefulWidget {
  const ManageAccountsSheet({super.key});

  @override
  ConsumerState<ManageAccountsSheet> createState() =>
      _ManageAccountsSheetState();
}

class _ManageAccountsSheetState extends ConsumerState<ManageAccountsSheet> {
  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref.read(accountsListNotifierProvider.notifier).loadAccounts(),
    );
  }

  String _formatCurrency(double value) {
    return NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$').format(value);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(accountsListNotifierProvider);

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
                  'Contas',
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
                : state.accounts.isEmpty
                    ? const Padding(
                        padding: EdgeInsets.all(48),
                        child: Text(
                          'Nenhuma conta cadastrada',
                          style: TextStyle(
                            color: Color(0xFF94A3B8),
                            fontSize: 16,
                          ),
                        ),
                      )
                    : ListView.builder(
                        shrinkWrap: true,
                        itemCount: state.accounts.length,
                        padding: const EdgeInsets.symmetric(vertical: 8),
                        itemBuilder: (context, index) {
                          final account = state.accounts[index];
                          return ListTile(
                            leading: CircleAvatar(
                              backgroundColor:
                                  AppColors.fromHex(account.color),
                              child: Icon(
                                AppMaterialIcons.fromName(account.icon),
                                color: Colors.white,
                                size: 20,
                              ),
                            ),
                            title: Text(
                              account.name,
                              style: const TextStyle(color: Colors.white),
                            ),
                            subtitle: Text(
                              account.institution.isNotEmpty
                                  ? account.institution
                                  : account.type.name,
                              style: const TextStyle(
                                color: Color(0xFF94A3B8),
                                fontSize: 12,
                              ),
                            ),
                            trailing: Text(
                              _formatCurrency(account.currentBalance),
                              style: TextStyle(
                                color: account.currentBalance >= 0
                                    ? const Color(0xFF00E676)
                                    : const Color(0xFFFF5252),
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            onTap: () => _openAddSheet(
                              context,
                              accountId: account.id,
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }

  void _openAddSheet(BuildContext context, {String? accountId}) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => AccountAddSheet(accountId: accountId),
    ).then(
      (_) => ref.read(accountsListNotifierProvider.notifier).loadAccounts(),
    );
  }
}

/// Helper to show this sheet.
void showManageAccountsSheet(BuildContext context) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.transparent,
    builder: (_) => const ManageAccountsSheet(),
  );
}
