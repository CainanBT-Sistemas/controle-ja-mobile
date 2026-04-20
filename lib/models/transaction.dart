import 'enums.dart';

class Transaction {
  final String? id;
  final String name;
  final String? description;
  final TransactionType type;
  final double amount;
  final int date;
  final bool paid;
  final String? accountId;
  final String? targetAccountId;
  final String? categoryId;
  final String? categoryName;
  final String? accountName;
  final bool isRecurring;
  final bool isFixed;
  final RecurrenceFrequency? recurrenceFrequency;
  final int? recurrenceEndDate;
  final String? recurrenceRuleId;
  final int installments;
  final String? creditCardId;
  final String? targetInvoiceId;
  final String? vehicleId;
  final String? vehicleName;
  final double? currentOdometer;
  final double? liters;
  final FuelType? fuelType;
  final double? efficiency;

  const Transaction({
    this.id,
    this.name = '',
    this.description,
    this.type = TransactionType.DESPESA,
    this.amount = 0,
    this.date = 0,
    this.paid = false,
    this.accountId,
    this.targetAccountId,
    this.categoryId,
    this.categoryName,
    this.accountName,
    this.isRecurring = false,
    this.isFixed = false,
    this.recurrenceFrequency,
    this.recurrenceEndDate,
    this.recurrenceRuleId,
    this.installments = 1,
    this.creditCardId,
    this.targetInvoiceId,
    this.vehicleId,
    this.vehicleName,
    this.currentOdometer,
    this.liters,
    this.fuelType,
    this.efficiency,
  });

  Transaction copyWith({
    String? id,
    String? name,
    String? description,
    TransactionType? type,
    double? amount,
    int? date,
    bool? paid,
    String? accountId,
    String? targetAccountId,
    String? categoryId,
    String? categoryName,
    String? accountName,
    bool? isRecurring,
    bool? isFixed,
    RecurrenceFrequency? recurrenceFrequency,
    int? recurrenceEndDate,
    String? recurrenceRuleId,
    int? installments,
    String? creditCardId,
    String? targetInvoiceId,
    String? vehicleId,
    String? vehicleName,
    double? currentOdometer,
    double? liters,
    FuelType? fuelType,
    double? efficiency,
  }) {
    return Transaction(
      id: id ?? this.id,
      name: name ?? this.name,
      description: description ?? this.description,
      type: type ?? this.type,
      amount: amount ?? this.amount,
      date: date ?? this.date,
      paid: paid ?? this.paid,
      accountId: accountId ?? this.accountId,
      targetAccountId: targetAccountId ?? this.targetAccountId,
      categoryId: categoryId ?? this.categoryId,
      categoryName: categoryName ?? this.categoryName,
      accountName: accountName ?? this.accountName,
      isRecurring: isRecurring ?? this.isRecurring,
      isFixed: isFixed ?? this.isFixed,
      recurrenceFrequency:
          recurrenceFrequency ?? this.recurrenceFrequency,
      recurrenceEndDate: recurrenceEndDate ?? this.recurrenceEndDate,
      recurrenceRuleId: recurrenceRuleId ?? this.recurrenceRuleId,
      installments: installments ?? this.installments,
      creditCardId: creditCardId ?? this.creditCardId,
      targetInvoiceId: targetInvoiceId ?? this.targetInvoiceId,
      vehicleId: vehicleId ?? this.vehicleId,
      vehicleName: vehicleName ?? this.vehicleName,
      currentOdometer: currentOdometer ?? this.currentOdometer,
      liters: liters ?? this.liters,
      fuelType: fuelType ?? this.fuelType,
      efficiency: efficiency ?? this.efficiency,
    );
  }

  factory Transaction.fromJson(Map<String, dynamic> json) {
    return Transaction(
      id: json['id'] as String?,
      name: json['name'] as String? ?? '',
      description: json['description'] as String?,
      type: json['type'] != null
          ? TransactionType.fromJson(json['type'] as String)
          : TransactionType.DESPESA,
      amount: (json['amount'] as num?)?.toDouble() ?? 0,
      date: json['date'] as int? ?? 0,
      paid: json['paid'] as bool? ?? false,
      accountId: json['accountId'] as String?,
      targetAccountId: json['targetAccountId'] as String?,
      categoryId: json['categoryId'] as String?,
      categoryName: json['categoryName'] as String?,
      accountName: json['accountName'] as String?,
      isRecurring: json['isRecurring'] as bool? ?? false,
      isFixed: json['isFixed'] as bool? ?? false,
      recurrenceFrequency: json['recurrenceFrequency'] != null
          ? RecurrenceFrequency.fromJson(
              json['recurrenceFrequency'] as String)
          : null,
      recurrenceEndDate: json['recurrenceEndDate'] as int?,
      recurrenceRuleId: json['recurrenceRuleId'] as String?,
      installments: json['installments'] as int? ?? 1,
      creditCardId: json['creditCardId'] as String?,
      targetInvoiceId: json['targetInvoiceId'] as String?,
      vehicleId: json['vehicleId'] as String?,
      vehicleName: json['vehicleName'] as String?,
      currentOdometer:
          (json['currentOdometer'] as num?)?.toDouble(),
      liters: (json['liters'] as num?)?.toDouble(),
      fuelType: json['fuelType'] != null
          ? FuelType.fromJson(json['fuelType'] as String)
          : null,
      efficiency: (json['efficiency'] as num?)?.toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'type': type.toJson(),
      'amount': amount,
      'date': date,
      'paid': paid,
      'accountId': accountId,
      'targetAccountId': targetAccountId,
      'categoryId': categoryId,
      'categoryName': categoryName,
      'accountName': accountName,
      'isRecurring': isRecurring,
      'isFixed': isFixed,
      'recurrenceFrequency': recurrenceFrequency?.toJson(),
      'recurrenceEndDate': recurrenceEndDate,
      'recurrenceRuleId': recurrenceRuleId,
      'installments': installments,
      'creditCardId': creditCardId,
      'targetInvoiceId': targetInvoiceId,
      'vehicleId': vehicleId,
      'vehicleName': vehicleName,
      'currentOdometer': currentOdometer,
      'liters': liters,
      'fuelType': fuelType?.toJson(),
      'efficiency': efficiency,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Transaction &&
          runtimeType == other.runtimeType &&
          id == other.id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() =>
      'Transaction(id: $id, name: $name, type: $type, amount: $amount, '
      'date: $date, paid: $paid)';
}
