import 'transaction.dart';

class TransactionGroup {
  final String dateHeader;
  final List<Transaction> transactions;

  const TransactionGroup({
    required this.dateHeader,
    this.transactions = const [],
  });

  TransactionGroup copyWith({
    String? dateHeader,
    List<Transaction>? transactions,
  }) {
    return TransactionGroup(
      dateHeader: dateHeader ?? this.dateHeader,
      transactions: transactions ?? this.transactions,
    );
  }

  factory TransactionGroup.fromJson(Map<String, dynamic> json) {
    return TransactionGroup(
      dateHeader: json['dateHeader'] as String? ?? '',
      transactions: (json['transactions'] as List<dynamic>?)
              ?.map((e) =>
                  Transaction.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'dateHeader': dateHeader,
      'transactions':
          transactions.map((e) => e.toJson()).toList(),
    };
  }

  @override
  String toString() =>
      'TransactionGroup(dateHeader: $dateHeader, '
      'count: ${transactions.length})';
}
