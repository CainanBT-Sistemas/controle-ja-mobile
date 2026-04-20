import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/theme.dart';
import '../../models/dashboard_data.dart';
import '../../providers/dashboard_notifier.dart';

/// Tela Home / Dashboard.
///
/// Migrada de HomeView.xaml + DashboardViewModel.cs — mantém identidade visual
/// (fundo #001524, cards #1E2A3A, verde #00E676, textos #94A3B8).
/// Implementa shimmer loading nativo via AnimatedOpacity.
class HomeView extends ConsumerStatefulWidget {
  const HomeView({super.key});

  @override
  ConsumerState<HomeView> createState() => _HomeViewState();
}

class _HomeViewState extends ConsumerState<HomeView> {
  @override
  void initState() {
    super.initState();
    // Carrega dados ao entrar na tela
    Future.microtask(
        () => ref.read(dashboardNotifierProvider.notifier).loadDashboard());
  }

  String _formatCurrency(double value) {
    final formatter =
        NumberFormat.currency(locale: 'pt_BR', symbol: 'R\$');
    return formatter.format(value);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(dashboardNotifierProvider);
    final notifier = ref.read(dashboardNotifierProvider.notifier);

    // Erros via SnackBar
    ref.listen<DashboardState>(dashboardNotifierProvider, (prev, next) {
      if (next.error != null && prev?.error != next.error) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(next.error!.message)),
        );
        notifier.clearError();
      }
    });

    return Stack(
      children: [
        // --- Conteúdo principal ---
        Column(
          children: [
            _buildHeader(state, notifier),
            Expanded(
              child: state.isLoading
                  ? _buildShimmer()
                  : _buildContent(state, notifier),
            ),
          ],
        ),

        // --- Settings overlay ---
        if (state.isSettingsMenuVisible) _buildSettingsOverlay(notifier),

        // --- Alerts popup ---
        if (state.isAlertsPopupVisible) _buildAlertsPopup(state, notifier),
      ],
    );
  }

  // -------------------------------------------------------------------------
  // Header
  // -------------------------------------------------------------------------

  Widget _buildHeader(DashboardState state, DashboardNotifier notifier) {
    return Container(
      color: const Color(0xFF001524),
      padding: EdgeInsets.only(
        top: MediaQuery.of(context).padding.top + 12,
        left: 20,
        right: 20,
        bottom: 10,
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Olá, ${state.userName}',
                    style: const TextStyle(
                        color: Colors.white,
                        fontSize: 20,
                        fontWeight: FontWeight.bold)),
                const Text('Bem-vindo de volta',
                    style: TextStyle(color: Color(0xFF94A3B8), fontSize: 12)),
              ],
            ),
          ),
          IconButton(
            icon: const Icon(Icons.settings, color: Colors.white, size: 28),
            onPressed: notifier.toggleSettingsMenu,
          ),
        ],
      ),
    );
  }

  // -------------------------------------------------------------------------
  // Shimmer / Loading skeleton
  // -------------------------------------------------------------------------

  Widget _buildShimmer() {
    return _ShimmerContainer(
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(top: 10, bottom: 160),
        child: Column(
          children: [
            // Balance skeleton
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Column(
                children: [
                  _shimmerBox(120, 20),
                  const SizedBox(height: 10),
                  _shimmerBox(200, 40),
                ],
              ),
            ),
            const SizedBox(height: 25),
            // Cards skeleton
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _shimmerBox(150, 15),
                  const SizedBox(height: 10),
                  SizedBox(
                    height: 100,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      itemCount: 3,
                      separatorBuilder: (_, __) => const SizedBox(width: 12),
                      itemBuilder: (_, __) => _shimmerBox(140, 100,
                          radius: 12),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 25),
            // Chart skeleton
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 15),
              child: _shimmerBox(double.infinity, 250, radius: 16),
            ),
          ],
        ),
      ),
    );
  }

  Widget _shimmerBox(double width, double height, {double radius = 4}) {
    return Container(
      width: width == double.infinity ? null : width,
      height: height,
      decoration: BoxDecoration(
        color: const Color(0xFF1E2A3A),
        borderRadius: BorderRadius.circular(radius),
      ),
    );
  }

  // -------------------------------------------------------------------------
  // Content (loaded)
  // -------------------------------------------------------------------------

  Widget _buildContent(DashboardState state, DashboardNotifier notifier) {
    return RefreshIndicator(
      color: const Color(0xFF00E676),
      backgroundColor: const Color(0xFF1E2A3A),
      onRefresh: notifier.loadDashboard,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.only(top: 10, bottom: 160),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Saldo disponível
            _buildBalanceSection(state),
            const SizedBox(height: 25),

            // Alertas
            if (state.hasOverduePayables ||
                state.hasOverdueInvoices ||
                state.hasPendingPayables ||
                state.hasPendingReceivables ||
                state.hasPendingInvoices)
              _buildAlertsSection(state, notifier),

            // Contas
            if (state.hasAccounts) ...[
              const SizedBox(height: 20),
              _buildAccountsSection(state),
            ],

            // Cartões
            if (state.hasCreditCards) ...[
              const SizedBox(height: 20),
              _buildCreditCardsSection(state),
            ],

            // Gráficos
            const SizedBox(height: 20),
            _buildChartsSection(state, notifier),
          ],
        ),
      ),
    );
  }

  // --- Balance ---

  Widget _buildBalanceSection(DashboardState state) {
    return Column(
      children: [
        const Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.visibility, color: Color(0xFF94A3B8), size: 16),
            SizedBox(width: 8),
            Text('Saldo Disponível',
                style: TextStyle(
                    color: Color(0xFF94A3B8),
                    fontSize: 13,
                    fontWeight: FontWeight.bold)),
          ],
        ),
        const SizedBox(height: 5),
        Text(
          _formatCurrency(state.availableBalance),
          style: const TextStyle(
              color: Colors.white, fontSize: 36, fontWeight: FontWeight.bold),
          textAlign: TextAlign.center,
        ),
      ],
    );
  }

  // --- Alerts horizontal scroll ---

  Widget _buildAlertsSection(
      DashboardState state, DashboardNotifier notifier) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Padding(
          padding: EdgeInsets.symmetric(horizontal: 20),
          child: Text('Pendências e Alertas',
              style: TextStyle(
                  color: Color(0xFF94A3B8),
                  fontSize: 14,
                  fontWeight: FontWeight.bold)),
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 100,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 20),
            children: [
              if (state.hasOverduePayables)
                _alertCard(
                  icon: Icons.warning,
                  label: 'Contas Atrasadas',
                  value: _formatCurrency(
                      state.overduePayables.fold(0.0, (s, e) => s + e.amount)),
                  valueColor: const Color(0xFFFF5252),
                  bgColor: const Color(0xFF2D1B1B),
                  borderColor: const Color(0xFFFF5252),
                  onTap: () => notifier.openAlerts('OVERDUE_PAYABLES'),
                ),
              if (state.hasOverdueInvoices)
                _alertCard(
                  icon: Icons.credit_card_off,
                  label: 'Faturas Vencidas',
                  value: _formatCurrency(
                      state.overdueInvoices.fold(0.0, (s, e) => s + e.amount)),
                  valueColor: const Color(0xFFFF5252),
                  bgColor: const Color(0xFF2D1B1B),
                  borderColor: const Color(0xFFFF5252),
                  onTap: () => notifier.openAlerts('OVERDUE_INVOICES'),
                ),
              if (state.hasPendingPayables)
                _alertCard(
                  icon: Icons.arrow_downward,
                  label: 'A Pagar (Mês)',
                  value: _formatCurrency(state.pendingPayables
                      .fold(0.0, (s, e) => s + e.amount)),
                  valueColor: Colors.white,
                  iconColor: const Color(0xFFFFAB00),
                  onTap: () => notifier.openAlerts('PAYABLES'),
                ),
              if (state.hasPendingReceivables)
                _alertCard(
                  icon: Icons.arrow_upward,
                  label: 'A Receber (Mês)',
                  value: _formatCurrency(state.pendingReceivables
                      .fold(0.0, (s, e) => s + e.amount)),
                  valueColor: Colors.white,
                  iconColor: const Color(0xFF00E676),
                  onTap: () => notifier.openAlerts('RECEIVABLES'),
                ),
              if (state.hasPendingInvoices)
                _alertCard(
                  icon: Icons.credit_card,
                  label: 'Faturas Abertas',
                  value: _formatCurrency(state.pendingInvoices
                      .fold(0.0, (s, e) => s + e.amount)),
                  valueColor: Colors.white,
                  iconColor: const Color(0xFFE040FB),
                  onTap: () => notifier.openAlerts('INVOICES'),
                ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _alertCard({
    required IconData icon,
    required String label,
    required String value,
    required Color valueColor,
    Color bgColor = const Color(0xFF1E2A3A),
    Color? borderColor,
    Color iconColor = const Color(0xFFFF5252),
    VoidCallback? onTap,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 140,
        margin: const EdgeInsets.only(right: 12),
        padding: const EdgeInsets.all(15),
        decoration: BoxDecoration(
          color: bgColor,
          borderRadius: BorderRadius.circular(12),
          border: borderColor != null
              ? Border.all(color: borderColor, width: 1)
              : null,
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, color: iconColor, size: 20),
            const SizedBox(height: 5),
            Text(label,
                style: const TextStyle(
                    color: Color(0xFF94A3B8),
                    fontSize: 11,
                    fontWeight: FontWeight.bold)),
            const SizedBox(height: 3),
            Text(value,
                style: TextStyle(
                    color: valueColor,
                    fontSize: 14,
                    fontWeight: FontWeight.bold)),
          ],
        ),
      ),
    );
  }

  // --- Accounts ---

  Widget _buildAccountsSection(DashboardState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Padding(
          padding: EdgeInsets.symmetric(horizontal: 20),
          child: Text('Minhas Contas',
              style: TextStyle(
                  color: Color(0xFF94A3B8),
                  fontSize: 14,
                  fontWeight: FontWeight.bold)),
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 100,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 20),
            itemCount: state.accounts.length,
            separatorBuilder: (_, __) => const SizedBox(width: 15),
            itemBuilder: (context, i) {
              final acc = state.accounts[i];
              final color = AppColors.fromHex(acc.color);
              return Container(
                width: 145,
                padding: const EdgeInsets.all(15),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E2A3A),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          width: 30,
                          height: 30,
                          decoration: BoxDecoration(
                            color: color,
                            borderRadius: BorderRadius.circular(15),
                          ),
                          child: Icon(
                            AppMaterialIcons.fromName(acc.icon),
                            color: const Color(0xFF001524),
                            size: 16,
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: Text(acc.name,
                              style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 13,
                                  fontWeight: FontWeight.bold),
                              overflow: TextOverflow.ellipsis),
                        ),
                      ],
                    ),
                    const SizedBox(height: 10),
                    Text(_formatCurrency(acc.currentBalance),
                        style: TextStyle(
                            color: color,
                            fontSize: 15,
                            fontWeight: FontWeight.bold)),
                  ],
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  // --- Credit Cards ---

  Widget _buildCreditCardsSection(DashboardState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Padding(
          padding: EdgeInsets.symmetric(horizontal: 20),
          child: Text('Cartões de Crédito',
              style: TextStyle(
                  color: Color(0xFF94A3B8),
                  fontSize: 14,
                  fontWeight: FontWeight.bold)),
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 175,
          child: PageView.builder(
            itemCount: state.creditCards.length,
            controller: PageController(viewportFraction: 0.9),
            itemBuilder: (context, i) {
              final card = state.creditCards[i];
              final color = AppColors.fromHex(card.color);
              return Padding(
                padding: const EdgeInsets.symmetric(horizontal: 6),
                child: Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: const Color(0xFF1E2A3A),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Container(
                            width: 36,
                            height: 36,
                            decoration: BoxDecoration(
                              color: color,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Icon(
                                AppMaterialIcons.fromName(card.icon),
                                color: Colors.white,
                                size: 20),
                          ),
                          const SizedBox(width: 15),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(card.name,
                                    style: const TextStyle(
                                        color: Colors.white,
                                        fontSize: 16,
                                        fontWeight: FontWeight.bold),
                                    overflow: TextOverflow.ellipsis),
                                Text(
                                    'Fecha dia ${card.closeDay} · Vence dia ${card.bestDay}',
                                    style: const TextStyle(
                                        color: Color(0xFF64748B),
                                        fontSize: 11)),
                              ],
                            ),
                          ),
                        ],
                      ),
                      const Spacer(),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text('Usado: ${_formatCurrency(card.usedAmount)}',
                              style: const TextStyle(
                                  color: Color(0xFFA1A1AA), fontSize: 11)),
                          Text(
                              'Livre: ${_formatCurrency(card.currentLimit)}',
                              style: const TextStyle(
                                  color: Color(0xFF00E676),
                                  fontSize: 11,
                                  fontWeight: FontWeight.bold)),
                        ],
                      ),
                      const SizedBox(height: 6),
                      ClipRRect(
                        borderRadius: BorderRadius.circular(3),
                        child: LinearProgressIndicator(
                          value: card.limitProgress,
                          backgroundColor: const Color(0xFF001524),
                          valueColor: AlwaysStoppedAnimation<Color>(color),
                          minHeight: 6,
                        ),
                      ),
                    ],
                  ),
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  // --- Charts section ---

  Widget _buildChartsSection(
      DashboardState state, DashboardNotifier notifier) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 15),
      padding: const EdgeInsets.all(15),
      decoration: BoxDecoration(
        color: const Color(0xFF1E2A3A),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          // Month navigator
          Row(
            children: [
              GestureDetector(
                onTap: notifier.previousMonth,
                child: const Icon(Icons.chevron_left,
                    color: Color(0xFF94A3B8), size: 28),
              ),
              Expanded(
                child: Text(
                  state.currentMonthDisplay.toUpperCase(),
                  style: const TextStyle(
                      color: Colors.white,
                      fontSize: 14,
                      fontWeight: FontWeight.bold),
                  textAlign: TextAlign.center,
                ),
              ),
              GestureDetector(
                onTap: notifier.nextMonth,
                child: const Icon(Icons.chevron_right,
                    color: Color(0xFF94A3B8), size: 28),
              ),
            ],
          ),
          const Divider(color: Color(0xFF2D3A4F), height: 20),

          // Expenses chart placeholder
          _chartBlock(
            title: 'Despesas Gerais (Conta e Carteira)',
            hasData: state.hasExpenseChart,
            emptyMessage: 'Nenhum gasto neste mês.',
            data: state.expensesByCategory,
          ),
          const Divider(color: Color(0xFF2D3A4F), height: 20),

          // Incomes chart placeholder
          _chartBlock(
            title: 'Receitas (Entradas)',
            hasData: state.hasIncomeChart,
            emptyMessage: 'Nenhuma receita neste mês.',
            data: state.incomesByCategory,
          ),
          const Divider(color: Color(0xFF2D3A4F), height: 20),

          // Credit expenses chart placeholder
          _chartBlock(
            title: 'Despesas do Cartão de Crédito',
            hasData: state.hasCreditChart,
            emptyMessage: 'Nenhum gasto no crédito neste mês.',
            data: state.creditExpensesByCategory,
          ),
        ],
      ),
    );
  }

  Widget _chartBlock({
    required String title,
    required bool hasData,
    required String emptyMessage,
    required List<ChartData> data,
  }) {
    return Column(
      children: [
        Text(title,
            style: const TextStyle(
                color: Color(0xFF94A3B8),
                fontSize: 13,
                fontWeight: FontWeight.bold)),
        const SizedBox(height: 10),
        if (!hasData)
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 20),
            child: Text(emptyMessage,
                style:
                    const TextStyle(color: Color(0xFF64748B), fontSize: 12)),
          )
        else
          // Representação em lista das categorias (donut chart virá com pacote)
          ...data.where((e) => e.value > 0).map((entry) {
            final color = entry.color != null && entry.color!.isNotEmpty
                ? AppColors.fromHex(entry.color!)
                : const Color(0xFF2979FF);
            return Padding(
              padding: const EdgeInsets.symmetric(vertical: 4),
              child: Row(
                children: [
                  Container(
                      width: 12,
                      height: 12,
                      decoration: BoxDecoration(
                          color: color, shape: BoxShape.circle)),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(entry.label,
                        style: const TextStyle(
                            color: Colors.white, fontSize: 13)),
                  ),
                  Text(_formatCurrency(entry.value),
                      style: const TextStyle(
                          color: Colors.white,
                          fontSize: 13,
                          fontWeight: FontWeight.bold)),
                ],
              ),
            );
          }),
      ],
    );
  }

  // -------------------------------------------------------------------------
  // Settings overlay
  // -------------------------------------------------------------------------

  Widget _buildSettingsOverlay(DashboardNotifier notifier) {
    return GestureDetector(
      onTap: notifier.closeSettingsMenu,
      child: Container(
        color: Colors.black.withValues(alpha: 0.7),
        child: Align(
          alignment: Alignment.topRight,
          child: SafeArea(
            child: Container(
              margin: const EdgeInsets.only(top: 60, right: 20),
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: const Color(0xFF1E2A3A),
                borderRadius: BorderRadius.circular(12),
              ),
              width: 160,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  TextButton(
                    onPressed: () {
                      notifier.closeSettingsMenu();
                      // TODO: navegar para perfil
                    },
                    child: const Align(
                      alignment: Alignment.centerLeft,
                      child: Text('Meu Perfil',
                          style: TextStyle(color: Colors.white)),
                    ),
                  ),
                  const Divider(color: Color(0xFF2D3A4F), height: 1),
                  TextButton(
                    onPressed: () {
                      notifier.closeSettingsMenu();
                      // TODO: logout real
                    },
                    child: const Align(
                      alignment: Alignment.centerLeft,
                      child: Text('Sair',
                          style: TextStyle(
                              color: Color(0xFFFF5252),
                              fontWeight: FontWeight.bold)),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  // -------------------------------------------------------------------------
  // Alerts Bottom Sheet Popup
  // -------------------------------------------------------------------------

  Widget _buildAlertsPopup(
      DashboardState state, DashboardNotifier notifier) {
    return GestureDetector(
      onTap: notifier.closeAlerts,
      child: Container(
        color: Colors.black.withValues(alpha: 0.8),
        child: Align(
          alignment: Alignment.bottomCenter,
          child: GestureDetector(
            onTap: () {}, // bloqueia tap passando pelo sheet
            child: Container(
              constraints: BoxConstraints(
                  maxHeight: MediaQuery.of(context).size.height * 0.6),
              padding: const EdgeInsets.fromLTRB(20, 25, 20, 0),
              decoration: const BoxDecoration(
                color: Color(0xFF1E2A3A),
                borderRadius: BorderRadius.only(
                  topLeft: Radius.circular(25),
                  topRight: Radius.circular(25),
                ),
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // Header
                  Row(
                    children: [
                      Expanded(
                        child: Text(state.alertsPopupTitle,
                            style: const TextStyle(
                                color: Colors.white,
                                fontSize: 18,
                                fontWeight: FontWeight.bold)),
                      ),
                      IconButton(
                        icon: const Icon(Icons.close,
                            color: Color(0xFF94A3B8), size: 26),
                        onPressed: notifier.closeAlerts,
                      ),
                    ],
                  ),
                  const Divider(color: Color(0xFF2D3A4F)),

                  // Lista
                  Flexible(
                    child: ListView.builder(
                      shrinkWrap: true,
                      padding: const EdgeInsets.only(bottom: 120),
                      itemCount: state.currentAlertsList.length,
                      itemBuilder: (context, i) {
                        final alert = state.currentAlertsList[i];
                        final color = alert.color != null
                            ? AppColors.fromHex(alert.color!)
                            : const Color(0xFF94A3B8);
                        return Padding(
                          padding: const EdgeInsets.only(bottom: 12),
                          child: Row(
                            children: [
                              Container(
                                width: 36,
                                height: 36,
                                decoration: BoxDecoration(
                                    color: color,
                                    borderRadius: BorderRadius.circular(18)),
                                child: Icon(
                                  alert.icon != null
                                      ? AppMaterialIcons.fromName(alert.icon!)
                                      : Icons.receipt,
                                  color: const Color(0xFF001524),
                                  size: 18,
                                ),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment:
                                      CrossAxisAlignment.start,
                                  children: [
                                    Text(alert.description ?? '',
                                        style: const TextStyle(
                                            color: Colors.white,
                                            fontSize: 14,
                                            fontWeight: FontWeight.bold),
                                        overflow: TextOverflow.ellipsis),
                                    if (alert.dueDate > 0)
                                      Text(
                                        'Venc: ${_formatDate(alert.dueDate)}',
                                        style: const TextStyle(
                                            color: Color(0xFF94A3B8),
                                            fontSize: 11),
                                      ),
                                  ],
                                ),
                              ),
                              const SizedBox(width: 8),
                              Text(_formatCurrency(alert.amount),
                                  style: TextStyle(
                                      color: color,
                                      fontSize: 14,
                                      fontWeight: FontWeight.bold)),
                              const SizedBox(width: 8),
                              GestureDetector(
                                onTap: () {
                                  // TODO: quick pay (Sprint 5)
                                },
                                child: Container(
                                  width: 32,
                                  height: 32,
                                  decoration: BoxDecoration(
                                    color: const Color(0xFF00E676),
                                    borderRadius: BorderRadius.circular(16),
                                  ),
                                  child: const Icon(Icons.check,
                                      color: Color(0xFF001524),
                                      size: 18),
                                ),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  String _formatDate(int timestampMs) {
    final dt = DateTime.fromMillisecondsSinceEpoch(timestampMs);
    return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year}';
  }
}

// ---------------------------------------------------------------------------
// Shimmer animation container
// ---------------------------------------------------------------------------

/// Widget que pulsa seus filhos entre 30% e 80% de opacidade,
/// replicando o efeito skeleton do HomeView.xaml.cs.
class _ShimmerContainer extends StatefulWidget {
  final Widget child;
  const _ShimmerContainer({required this.child});

  @override
  State<_ShimmerContainer> createState() => _ShimmerContainerState();
}

class _ShimmerContainerState extends State<_ShimmerContainer>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _opacity;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1000),
    )..repeat(reverse: true);
    _opacity = Tween<double>(begin: 0.3, end: 0.8).animate(_controller);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _opacity,
      builder: (context, child) => Opacity(opacity: _opacity.value, child: child),
      child: widget.child,
    );
  }
}
