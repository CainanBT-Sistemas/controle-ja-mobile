class CreditCard {
  final String id;
  final String accountId;
  final String name;
  final double totalLimit;
  final double currentLimit;
  final int closeDay;
  final int bestDay;
  final String icon;
  final String color;

  const CreditCard({
    required this.id,
    required this.accountId,
    this.name = '',
    this.totalLimit = 0,
    this.currentLimit = 0,
    this.closeDay = 1,
    this.bestDay = 1,
    this.icon = 'credit_card',
    this.color = '#9C27B0',
  });

  double get usedAmount => (totalLimit - currentLimit).clamp(0, double.infinity);

  double get limitProgress =>
      totalLimit == 0 ? 0 : usedAmount / totalLimit;

  CreditCard copyWith({
    String? id,
    String? accountId,
    String? name,
    double? totalLimit,
    double? currentLimit,
    int? closeDay,
    int? bestDay,
    String? icon,
    String? color,
  }) {
    return CreditCard(
      id: id ?? this.id,
      accountId: accountId ?? this.accountId,
      name: name ?? this.name,
      totalLimit: totalLimit ?? this.totalLimit,
      currentLimit: currentLimit ?? this.currentLimit,
      closeDay: closeDay ?? this.closeDay,
      bestDay: bestDay ?? this.bestDay,
      icon: icon ?? this.icon,
      color: color ?? this.color,
    );
  }

  factory CreditCard.fromJson(Map<String, dynamic> json) {
    return CreditCard(
      id: json['id'] as String? ?? '',
      accountId: json['accountId'] as String? ?? '',
      name: json['name'] as String? ?? '',
      totalLimit: (json['totalLimit'] as num?)?.toDouble() ?? 0,
      currentLimit: (json['currentLimit'] as num?)?.toDouble() ?? 0,
      closeDay: json['closeDay'] as int? ?? 1,
      bestDay: json['bestDay'] as int? ?? 1,
      icon: json['icon'] as String? ?? 'credit_card',
      color: json['color'] as String? ?? '#9C27B0',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'accountId': accountId,
      'name': name,
      'totalLimit': totalLimit,
      'currentLimit': currentLimit,
      'closeDay': closeDay,
      'bestDay': bestDay,
      'icon': icon,
      'color': color,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CreditCard &&
          runtimeType == other.runtimeType &&
          id == other.id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() =>
      'CreditCard(id: $id, accountId: $accountId, name: $name, '
      'totalLimit: $totalLimit, currentLimit: $currentLimit, '
      'closeDay: $closeDay, bestDay: $bestDay, icon: $icon, color: $color)';
}
