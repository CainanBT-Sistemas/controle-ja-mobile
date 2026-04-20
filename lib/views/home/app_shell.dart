import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'home_view.dart';
import 'transactions_tab.dart';
import 'settings_tab.dart';
import '../modals/transaction_add_sheet.dart';

/// Índice da aba ativa (provider simples).
final activeTabProvider = StateProvider<int>((ref) => 0);

/// Shell principal da aplicação logada.
///
/// Migrado de DashboardPage.xaml + BottomMenu.xaml — Scaffold com
/// BottomNavigationBar (5 itens) e FAB central verde (#00E676).
class AppShell extends ConsumerWidget {
  const AppShell({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activeTab = ref.watch(activeTabProvider);

    return Scaffold(
      backgroundColor: const Color(0xFF001524),
      body: IndexedStack(
        index: activeTab,
        children: const [
          HomeView(),
          TransactionsTab(),
          _PlaceholderTab(label: 'Cartões'),
          _PlaceholderTab(label: 'Planejamento'),
          SettingsTab(),
        ],
      ),
      bottomNavigationBar: _buildBottomBar(context, ref, activeTab),
      floatingActionButton: _buildFab(context),
      floatingActionButtonLocation: FloatingActionButtonLocation.centerDocked,
    );
  }

  Widget _buildBottomBar(
      BuildContext context, WidgetRef ref, int activeTab) {
    const activeColor = Color(0xFF00E676);
    const inactiveColor = Color(0xFF64748B);
    const bgColor = Color(0xFF0A0F17);

    Widget item(IconData icon, String label, int index) {
      final isActive = activeTab == index;
      return Expanded(
        child: InkWell(
          onTap: () => ref.read(activeTabProvider.notifier).state = index,
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 8),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(icon,
                    color: isActive ? activeColor : inactiveColor, size: 26),
                const SizedBox(height: 4),
                Text(
                  label,
                  style: TextStyle(
                    color: isActive ? activeColor : inactiveColor,
                    fontSize: 10,
                    fontWeight:
                        isActive ? FontWeight.bold : FontWeight.normal,
                  ),
                ),
              ],
            ),
          ),
        ),
      );
    }

    return Container(
      decoration: const BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.only(
          topLeft: Radius.circular(15),
          topRight: Radius.circular(15),
        ),
        boxShadow: [
          BoxShadow(color: Colors.black54, offset: Offset(0, -5), blurRadius: 15),
        ],
      ),
      child: SafeArea(
        child: SizedBox(
          height: 64,
          child: Row(
            children: [
              item(Icons.home, 'Principal', 0),
              item(Icons.receipt_long, 'Transações', 1),
              const Expanded(child: SizedBox()), // espaço do FAB
              item(Icons.directions_car, 'Planejamento', 3),
              item(Icons.person, 'Mais', 4),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildFab(BuildContext context) {
    return SizedBox(
      height: 64,
      width: 64,
      child: FloatingActionButton(
        backgroundColor: const Color(0xFF00E676),
        elevation: 6,
        shape: const CircleBorder(),
        onPressed: () => _showTransactionTypeSheet(context),
        child: const Icon(Icons.add, color: Color(0xFF121214), size: 36),
      ),
    );
  }

  void _showTransactionTypeSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      builder: (_) => Container(
        decoration: const BoxDecoration(
          color: Color(0xFF0A0F17),
          borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
        ),
        child: SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                margin: const EdgeInsets.only(top: 12),
                width: 40, height: 4,
                decoration: BoxDecoration(
                  color: const Color(0xFF334155),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const Padding(
                padding: EdgeInsets.fromLTRB(20, 16, 20, 8),
                child: Text(
                  'Novo Lançamento',
                  style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ),
              _txTypeItem(context, Icons.trending_down, 'Despesa', const Color(0xFFFF5252), 'Despesa'),
              _txTypeItem(context, Icons.credit_card, 'Despesa no Cartão', const Color(0xFF9333EA), 'Despesa no Cartão'),
              _txTypeItem(context, Icons.trending_up, 'Receita', const Color(0xFF00E676), 'Receita'),
              _txTypeItem(context, Icons.swap_horiz, 'Transferência', const Color(0xFF3B82F6), 'Transferência'),
              const SizedBox(height: 16),
            ],
          ),
        ),
      ),
    );
  }

  Widget _txTypeItem(BuildContext context, IconData icon, String label, Color color, String type) {
    return ListTile(
      leading: CircleAvatar(backgroundColor: color.withAlpha(40), child: Icon(icon, color: color)),
      title: Text(label, style: const TextStyle(color: Colors.white)),
      onTap: () {
        Navigator.of(context).pop();
        showModalBottomSheet(
          context: context,
          isScrollControlled: true,
          backgroundColor: Colors.transparent,
          builder: (_) => TransactionAddSheet(transactionType: type),
        );
      },
    );
  }
}

/// Placeholder para abas ainda não migradas.
class _PlaceholderTab extends StatelessWidget {
  final String label;
  const _PlaceholderTab({required this.label});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Text(
        '$label\n(Em construção)',
        textAlign: TextAlign.center,
        style: const TextStyle(color: Color(0xFF94A3B8), fontSize: 18),
      ),
    );
  }
}
