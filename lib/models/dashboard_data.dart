import 'account.dart';
import 'credit_card.dart';

class ChartData {
  final String label;
  final double value;
  final String? color;

  const ChartData({
    this.label = '',
    this.value = 0,
    this.color,
  });

  ChartData copyWith({
    String? label,
    double? value,
    String? color,
  }) {
    return ChartData(
      label: label ?? this.label,
      value: value ?? this.value,
      color: color ?? this.color,
    );
  }

  factory ChartData.fromJson(Map<String, dynamic> json) {
    return ChartData(
      label: json['label'] as String? ?? '',
      value: (json['value'] as num?)?.toDouble() ?? 0,
      color: json['color'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'label': label,
      'value': value,
      'color': color,
    };
  }
}

class FinancialSummary {
  final double totalIncome;
  final double totalExpense;
  final double balance;

  const FinancialSummary({
    this.totalIncome = 0,
    this.totalExpense = 0,
    this.balance = 0,
  });

  FinancialSummary copyWith({
    double? totalIncome,
    double? totalExpense,
    double? balance,
  }) {
    return FinancialSummary(
      totalIncome: totalIncome ?? this.totalIncome,
      totalExpense: totalExpense ?? this.totalExpense,
      balance: balance ?? this.balance,
    );
  }

  factory FinancialSummary.fromJson(Map<String, dynamic> json) {
    return FinancialSummary(
      totalIncome: (json['totalIncome'] as num?)?.toDouble() ?? 0,
      totalExpense: (json['totalExpense'] as num?)?.toDouble() ?? 0,
      balance: (json['balance'] as num?)?.toDouble() ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'totalIncome': totalIncome,
      'totalExpense': totalExpense,
      'balance': balance,
    };
  }
}

class DashboardAlert {
  final String id;
  final String? description;
  final double amount;
  final int dueDate;
  final String? icon;
  final String? color;
  final String? type;

  const DashboardAlert({
    required this.id,
    this.description,
    this.amount = 0,
    this.dueDate = 0,
    this.icon,
    this.color,
    this.type,
  });

  DashboardAlert copyWith({
    String? id,
    String? description,
    double? amount,
    int? dueDate,
    String? icon,
    String? color,
    String? type,
  }) {
    return DashboardAlert(
      id: id ?? this.id,
      description: description ?? this.description,
      amount: amount ?? this.amount,
      dueDate: dueDate ?? this.dueDate,
      icon: icon ?? this.icon,
      color: color ?? this.color,
      type: type ?? this.type,
    );
  }

  factory DashboardAlert.fromJson(Map<String, dynamic> json) {
    return DashboardAlert(
      id: json['id'] as String? ?? '',
      description: json['description'] as String?,
      amount: (json['amount'] as num?)?.toDouble() ?? 0,
      dueDate: json['dueDate'] as int? ?? 0,
      icon: json['icon'] as String?,
      color: json['color'] as String?,
      type: json['type'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'description': description,
      'amount': amount,
      'dueDate': dueDate,
      'icon': icon,
      'color': color,
      'type': type,
    };
  }
}

class DashboardFullSummary {
  final double availableBalance;
  final double projectedBalance;
  final double projectedPayables;
  final double projectedVariables;
  final List<DashboardAlert> pendingPayables;
  final List<DashboardAlert> pendingReceivables;
  final List<DashboardAlert> pendingInvoices;
  final List<Account> accounts;
  final List<CreditCard> creditCards;
  final List<DashboardAlert> overduePayables;
  final List<DashboardAlert> overdueInvoices;

  const DashboardFullSummary({
    this.availableBalance = 0,
    this.projectedBalance = 0,
    this.projectedPayables = 0,
    this.projectedVariables = 0,
    this.pendingPayables = const [],
    this.pendingReceivables = const [],
    this.pendingInvoices = const [],
    this.accounts = const [],
    this.creditCards = const [],
    this.overduePayables = const [],
    this.overdueInvoices = const [],
  });

  DashboardFullSummary copyWith({
    double? availableBalance,
    double? projectedBalance,
    double? projectedPayables,
    double? projectedVariables,
    List<DashboardAlert>? pendingPayables,
    List<DashboardAlert>? pendingReceivables,
    List<DashboardAlert>? pendingInvoices,
    List<Account>? accounts,
    List<CreditCard>? creditCards,
    List<DashboardAlert>? overduePayables,
    List<DashboardAlert>? overdueInvoices,
  }) {
    return DashboardFullSummary(
      availableBalance: availableBalance ?? this.availableBalance,
      projectedBalance: projectedBalance ?? this.projectedBalance,
      projectedPayables: projectedPayables ?? this.projectedPayables,
      projectedVariables: projectedVariables ?? this.projectedVariables,
      pendingPayables: pendingPayables ?? this.pendingPayables,
      pendingReceivables: pendingReceivables ?? this.pendingReceivables,
      pendingInvoices: pendingInvoices ?? this.pendingInvoices,
      accounts: accounts ?? this.accounts,
      creditCards: creditCards ?? this.creditCards,
      overduePayables: overduePayables ?? this.overduePayables,
      overdueInvoices: overdueInvoices ?? this.overdueInvoices,
    );
  }

  factory DashboardFullSummary.fromJson(Map<String, dynamic> json) {
    return DashboardFullSummary(
      availableBalance:
          (json['availableBalance'] as num?)?.toDouble() ?? 0,
      projectedBalance:
          (json['projectedBalance'] as num?)?.toDouble() ?? 0,
      projectedPayables:
          (json['projectedPayables'] as num?)?.toDouble() ?? 0,
      projectedVariables:
          (json['projectedVariables'] as num?)?.toDouble() ?? 0,
      pendingPayables: (json['pendingPayables'] as List<dynamic>?)
              ?.map((e) =>
                  DashboardAlert.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      pendingReceivables: (json['pendingReceivables'] as List<dynamic>?)
              ?.map((e) =>
                  DashboardAlert.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      pendingInvoices: (json['pendingInvoices'] as List<dynamic>?)
              ?.map((e) =>
                  DashboardAlert.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      accounts: (json['accounts'] as List<dynamic>?)
              ?.map(
                  (e) => Account.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      creditCards: (json['creditCards'] as List<dynamic>?)
              ?.map((e) =>
                  CreditCard.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      overduePayables: (json['overduePayables'] as List<dynamic>?)
              ?.map((e) =>
                  DashboardAlert.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      overdueInvoices: (json['overdueInvoices'] as List<dynamic>?)
              ?.map((e) =>
                  DashboardAlert.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'availableBalance': availableBalance,
      'projectedBalance': projectedBalance,
      'projectedPayables': projectedPayables,
      'projectedVariables': projectedVariables,
      'pendingPayables':
          pendingPayables.map((e) => e.toJson()).toList(),
      'pendingReceivables':
          pendingReceivables.map((e) => e.toJson()).toList(),
      'pendingInvoices':
          pendingInvoices.map((e) => e.toJson()).toList(),
      'accounts': accounts.map((e) => e.toJson()).toList(),
      'creditCards': creditCards.map((e) => e.toJson()).toList(),
      'overduePayables':
          overduePayables.map((e) => e.toJson()).toList(),
      'overdueInvoices':
          overdueInvoices.map((e) => e.toJson()).toList(),
    };
  }
}
