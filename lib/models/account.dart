import 'enums.dart';

class Account {
  final String id;
  final String name;
  final AccountType type;
  final String institution;
  final double currentBalance;
  final String icon;
  final String color;
  final bool isDefault;

  const Account({
    required this.id,
    this.name = '',
    this.type = AccountType.BANK,
    this.institution = '',
    this.currentBalance = 0,
    this.icon = 'account_balance_wallet',
    this.color = '#42A5F5',
    this.isDefault = false,
  });

  Account copyWith({
    String? id,
    String? name,
    AccountType? type,
    String? institution,
    double? currentBalance,
    String? icon,
    String? color,
    bool? isDefault,
  }) {
    return Account(
      id: id ?? this.id,
      name: name ?? this.name,
      type: type ?? this.type,
      institution: institution ?? this.institution,
      currentBalance: currentBalance ?? this.currentBalance,
      icon: icon ?? this.icon,
      color: color ?? this.color,
      isDefault: isDefault ?? this.isDefault,
    );
  }

  factory Account.fromJson(Map<String, dynamic> json) {
    return Account(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      type: json['type'] != null
          ? AccountType.fromJson(json['type'] as String)
          : AccountType.BANK,
      institution: json['institution'] as String? ?? '',
      currentBalance: (json['currentBalance'] as num?)?.toDouble() ?? 0,
      icon: json['icon'] as String? ?? 'account_balance_wallet',
      color: json['color'] as String? ?? '#42A5F5',
      isDefault: json['isDefault'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'type': type.toJson(),
      'institution': institution,
      'currentBalance': currentBalance,
      'icon': icon,
      'color': color,
      'isDefault': isDefault,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Account && runtimeType == other.runtimeType && id == other.id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() =>
      'Account(id: $id, name: $name, type: $type, institution: $institution, '
      'currentBalance: $currentBalance, icon: $icon, color: $color, '
      'isDefault: $isDefault)';
}
