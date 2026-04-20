enum TransactionType {
  RECEITA,
  DESPESA,
  TRANSFERENCIA,
  TRANSFERENCIA_ENTRADA,
  TRANSFERENCIA_SAIDA,
  PAGAMENTO_FATURA;

  String toJson() => name;

  static TransactionType fromJson(String json) =>
      TransactionType.values.firstWhere(
        (e) => e.name == json,
        orElse: () => TransactionType.DESPESA,
      );
}

enum AccountType {
  WALLET,
  BANK,
  SAVINGS,
  CREDIT_CARD;

  String toJson() => name;

  static AccountType fromJson(String json) => AccountType.values.firstWhere(
        (e) => e.name == json,
        orElse: () => AccountType.BANK,
      );
}

enum FuelType {
  GASOLINA,
  ETANOL,
  DIESEL,
  GNV,
  ELETRICO,
  OUTRO;

  String toJson() => name;

  static FuelType fromJson(String json) => FuelType.values.firstWhere(
        (e) => e.name == json,
        orElse: () => FuelType.OUTRO,
      );
}

enum UserRole {
  USER,
  ADMIN,
  MANAGER;

  String toJson() => name;

  static UserRole fromJson(String json) => UserRole.values.firstWhere(
        (e) => e.name == json,
        orElse: () => UserRole.USER,
      );
}

enum RecurrenceFrequency {
  DAILY,
  WEEKLY,
  BIWEEKLY,
  MONTHLY,
  YEARLY;

  String toJson() => name;

  static RecurrenceFrequency fromJson(String json) =>
      RecurrenceFrequency.values.firstWhere(
        (e) => e.name == json,
        orElse: () => RecurrenceFrequency.MONTHLY,
      );
}
