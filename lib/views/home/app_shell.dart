import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'home_view.dart';

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
          _PlaceholderTab(label: 'Transações'),
          _PlaceholderTab(label: 'Cartões'),
          _PlaceholderTab(label: 'Planejamento'),
          _PlaceholderTab(label: 'Mais'),
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
        onPressed: () {
          // TODO: Abrir popup de nova transação (Sprint 5)
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
                content: Text('Nova transação estará disponível em breve.')),
          );
        },
        child: const Icon(Icons.add, color: Color(0xFF121214), size: 36),
      ),
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
