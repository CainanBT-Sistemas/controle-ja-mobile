import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../core/error_handler.dart';
import '../models/account.dart';
import '../models/credit_card.dart';
import '../models/dashboard_data.dart';
import '../services/dashboard_service.dart';
import 'auth_providers.dart';

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/// Estado do DashboardNotifier.
///
/// Migrado de DashboardViewModel.cs — resumo financeiro, alertas, gráficos.
class DashboardState {
  final bool isLoading;
  final String userName;
  final DateTime currentDate;

  // Resumo financeiro
  final double availableBalance;
  final double projectedBalance;
  final double projectedPayables;
  final double projectedVariables;

  // Contas e cartões
  final List<Account> accounts;
  final List<CreditCard> creditCards;

  // Alertas
  final List<DashboardAlert> overduePayables;
  final List<DashboardAlert> overdueInvoices;
  final List<DashboardAlert> pendingPayables;
  final List<DashboardAlert> pendingReceivables;
  final List<DashboardAlert> pendingInvoices;

  // Dados de gráfico
  final List<ChartData> expensesByCategory;
  final List<ChartData> incomesByCategory;
  final List<ChartData> creditExpensesByCategory;

  // Popup de alertas
  final bool isAlertsPopupVisible;
  final String alertsPopupTitle;
  final List<DashboardAlert> currentAlertsList;

  // Menu de configurações
  final bool isSettingsMenuVisible;

  final UserFriendlyError? error;

  DashboardState({
    this.isLoading = false,
    this.userName = 'Usuário',
    DateTime? currentDate,
    this.availableBalance = 0,
    this.projectedBalance = 0,
    this.projectedPayables = 0,
    this.projectedVariables = 0,
    this.accounts = const [],
    this.creditCards = const [],
    this.overduePayables = const [],
    this.overdueInvoices = const [],
    this.pendingPayables = const [],
    this.pendingReceivables = const [],
    this.pendingInvoices = const [],
    this.expensesByCategory = const [],
    this.incomesByCategory = const [],
    this.creditExpensesByCategory = const [],
    this.isAlertsPopupVisible = false,
    this.alertsPopupTitle = '',
    this.currentAlertsList = const [],
    this.isSettingsMenuVisible = false,
    this.error,
  }) : currentDate = currentDate ?? DateTime.now();

  DashboardState copyWith({
    bool? isLoading,
    String? userName,
    DateTime? currentDate,
    double? availableBalance,
    double? projectedBalance,
    double? projectedPayables,
    double? projectedVariables,
    List<Account>? accounts,
    List<CreditCard>? creditCards,
    List<DashboardAlert>? overduePayables,
    List<DashboardAlert>? overdueInvoices,
    List<DashboardAlert>? pendingPayables,
    List<DashboardAlert>? pendingReceivables,
    List<DashboardAlert>? pendingInvoices,
    List<ChartData>? expensesByCategory,
    List<ChartData>? incomesByCategory,
    List<ChartData>? creditExpensesByCategory,
    bool? isAlertsPopupVisible,
    String? alertsPopupTitle,
    List<DashboardAlert>? currentAlertsList,
    bool? isSettingsMenuVisible,
    UserFriendlyError? error,
    bool clearError = false,
  }) {
    return DashboardState(
      isLoading: isLoading ?? this.isLoading,
      userName: userName ?? this.userName,
      currentDate: currentDate ?? this.currentDate,
      availableBalance: availableBalance ?? this.availableBalance,
      projectedBalance: projectedBalance ?? this.projectedBalance,
      projectedPayables: projectedPayables ?? this.projectedPayables,
      projectedVariables: projectedVariables ?? this.projectedVariables,
      accounts: accounts ?? this.accounts,
      creditCards: creditCards ?? this.creditCards,
      overduePayables: overduePayables ?? this.overduePayables,
      overdueInvoices: overdueInvoices ?? this.overdueInvoices,
      pendingPayables: pendingPayables ?? this.pendingPayables,
      pendingReceivables: pendingReceivables ?? this.pendingReceivables,
      pendingInvoices: pendingInvoices ?? this.pendingInvoices,
      expensesByCategory: expensesByCategory ?? this.expensesByCategory,
      incomesByCategory: incomesByCategory ?? this.incomesByCategory,
      creditExpensesByCategory:
          creditExpensesByCategory ?? this.creditExpensesByCategory,
      isAlertsPopupVisible:
          isAlertsPopupVisible ?? this.isAlertsPopupVisible,
      alertsPopupTitle: alertsPopupTitle ?? this.alertsPopupTitle,
      currentAlertsList: currentAlertsList ?? this.currentAlertsList,
      isSettingsMenuVisible:
          isSettingsMenuVisible ?? this.isSettingsMenuVisible,
      error: clearError ? null : (error ?? this.error),
    );
  }

  /// Mês corrente formatado: "abril de 2026"
  String get currentMonthDisplay {
    const months = [
      'janeiro', 'fevereiro', 'março', 'abril', 'maio', 'junho',
      'julho', 'agosto', 'setembro', 'outubro', 'novembro', 'dezembro',
    ];
    return '${months[currentDate.month - 1]} de ${currentDate.year}';
  }

  bool get hasAccounts => accounts.isNotEmpty;
  bool get hasCreditCards => creditCards.isNotEmpty;
  bool get hasOverduePayables => overduePayables.isNotEmpty;
  bool get hasOverdueInvoices => overdueInvoices.isNotEmpty;
  bool get hasPendingPayables => pendingPayables.isNotEmpty;
  bool get hasPendingReceivables => pendingReceivables.isNotEmpty;
  bool get hasPendingInvoices => pendingInvoices.isNotEmpty;
  bool get hasExpenseChart =>
      expensesByCategory.any((e) => e.value > 0);
  bool get hasIncomeChart =>
      incomesByCategory.any((e) => e.value > 0);
  bool get hasCreditChart =>
      creditExpensesByCategory.any((e) => e.value > 0);
}

// ---------------------------------------------------------------------------
// Notifier
// ---------------------------------------------------------------------------

/// Notifier para o Dashboard.
///
/// Migrado de DashboardViewModel.cs — busca resumo + gráficos via
/// DashboardService e gerencia mês corrente.
class DashboardNotifier extends StateNotifier<DashboardState> {
  final DashboardService _dashboardService;

  DashboardNotifier(this._dashboardService)
      : super(DashboardState(currentDate: DateTime.now())) {
    _loadUserName();
  }

  Future<void> _loadUserName() async {
    final prefs = await SharedPreferences.getInstance();
    final name = prefs.getString('UserName') ?? 'Usuário';
    state = state.copyWith(userName: name);
  }

  /// Carrega todos os dados do dashboard para o mês corrente.
  Future<void> loadDashboard() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      await Future.wait([_loadSummary(), _loadCharts()]);
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
    await loadDashboard();
  }

  /// Retrocede um mês e recarrega.
  Future<void> previousMonth() async {
    final prev = DateTime(
        state.currentDate.year, state.currentDate.month - 1, 1);
    state = state.copyWith(currentDate: prev);
    await loadDashboard();
  }

  // --- Alertas ---

  void openAlerts(String type) {
    List<DashboardAlert> list;
    String title;
    switch (type) {
      case 'OVERDUE_PAYABLES':
        list = state.overduePayables;
        title = 'Contas Atrasadas';
      case 'OVERDUE_INVOICES':
        list = state.overdueInvoices;
        title = 'Faturas Vencidas';
      case 'PAYABLES':
        list = state.pendingPayables;
        title = 'A Pagar (Mês)';
      case 'RECEIVABLES':
        list = state.pendingReceivables;
        title = 'A Receber (Mês)';
      case 'INVOICES':
        list = state.pendingInvoices;
        title = 'Faturas Abertas';
      default:
        return;
    }
    if (list.isEmpty) return;
    state = state.copyWith(
      currentAlertsList: List.of(list),
      alertsPopupTitle: title,
      isAlertsPopupVisible: true,
    );
  }

  void closeAlerts() => state = state.copyWith(isAlertsPopupVisible: false);

  void toggleSettingsMenu() =>
      state = state.copyWith(
          isSettingsMenuVisible: !state.isSettingsMenuVisible);

  void closeSettingsMenu() =>
      state = state.copyWith(isSettingsMenuVisible: false);

  void clearError() => state = state.copyWith(clearError: true);

  // --- Privados ---

  Future<void> _loadSummary() async {
    final now = DateTime.now();
    final firstDay = DateTime(now.year, now.month, 1);
    final lastDay =
        DateTime(now.year, now.month + 1, 1).subtract(const Duration(seconds: 1));

    final start = firstDay.millisecondsSinceEpoch;
    final end = lastDay.millisecondsSinceEpoch;

    final summary = await _dashboardService.getFullSummary(start, end);
    if (summary != null) {
      state = state.copyWith(
        availableBalance: summary.availableBalance,
        projectedBalance: summary.projectedBalance,
        projectedPayables: summary.projectedPayables,
        projectedVariables: summary.projectedVariables,
        accounts: summary.accounts,
        creditCards: summary.creditCards,
        overduePayables: summary.overduePayables,
        overdueInvoices: summary.overdueInvoices,
        pendingPayables: summary.pendingPayables,
        pendingReceivables: summary.pendingReceivables,
        pendingInvoices: summary.pendingInvoices,
      );
    }
  }

  Future<void> _loadCharts() async {
    final firstDay =
        DateTime(state.currentDate.year, state.currentDate.month, 1);
    final lastDay = DateTime(
            state.currentDate.year, state.currentDate.month + 1, 1)
        .subtract(const Duration(seconds: 1));

    final start = firstDay.millisecondsSinceEpoch;
    final end = lastDay.millisecondsSinceEpoch;

    final results = await Future.wait([
      _dashboardService.getExpensesByCategory(start, end),
      _dashboardService.getIncomesByCategory(start, end),
      _dashboardService.getCreditExpensesByCategory(start, end),
    ]);

    state = state.copyWith(
      expensesByCategory: results[0],
      incomesByCategory: results[1],
      creditExpensesByCategory: results[2],
    );
  }
}

// ---------------------------------------------------------------------------
// Providers
// ---------------------------------------------------------------------------

/// Provider do DashboardService.
final dashboardServiceProvider = Provider<DashboardService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return DashboardService(apiClient: apiClient);
});

/// Provider do DashboardNotifier.
final dashboardNotifierProvider =
    StateNotifierProvider<DashboardNotifier, DashboardState>((ref) {
  final service = ref.watch(dashboardServiceProvider);
  return DashboardNotifier(service);
});
