class AdvanceablePurchaseDTO {
  final String purchaseId;
  final String name;
  final int maxInstallmentsAvailable;

  const AdvanceablePurchaseDTO({
    required this.purchaseId,
    this.name = '',
    this.maxInstallmentsAvailable = 0,
  });

  AdvanceablePurchaseDTO copyWith({
    String? purchaseId,
    String? name,
    int? maxInstallmentsAvailable,
  }) {
    return AdvanceablePurchaseDTO(
      purchaseId: purchaseId ?? this.purchaseId,
      name: name ?? this.name,
      maxInstallmentsAvailable:
          maxInstallmentsAvailable ?? this.maxInstallmentsAvailable,
    );
  }

  factory AdvanceablePurchaseDTO.fromJson(Map<String, dynamic> json) {
    return AdvanceablePurchaseDTO(
      purchaseId: json['purchaseId'] as String? ?? '',
      name: json['name'] as String? ?? '',
      maxInstallmentsAvailable:
          json['maxInstallmentsAvailable'] as int? ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'purchaseId': purchaseId,
      'name': name,
      'maxInstallmentsAvailable': maxInstallmentsAvailable,
    };
  }
}

class InvoiceItemDTO {
  final String id;
  final int date;
  final String name;
  final int currentInstallment;
  final int totalInstallmentsPlan;
  final double amount;

  const InvoiceItemDTO({
    required this.id,
    this.date = 0,
    this.name = '',
    this.currentInstallment = 0,
    this.totalInstallmentsPlan = 0,
    this.amount = 0,
  });

  InvoiceItemDTO copyWith({
    String? id,
    int? date,
    String? name,
    int? currentInstallment,
    int? totalInstallmentsPlan,
    double? amount,
  }) {
    return InvoiceItemDTO(
      id: id ?? this.id,
      date: date ?? this.date,
      name: name ?? this.name,
      currentInstallment: currentInstallment ?? this.currentInstallment,
      totalInstallmentsPlan:
          totalInstallmentsPlan ?? this.totalInstallmentsPlan,
      amount: amount ?? this.amount,
    );
  }

  factory InvoiceItemDTO.fromJson(Map<String, dynamic> json) {
    return InvoiceItemDTO(
      id: json['id'] as String? ?? '',
      date: json['date'] as int? ?? 0,
      name: json['name'] as String? ?? '',
      currentInstallment: json['currentInstallment'] as int? ?? 0,
      totalInstallmentsPlan: json['totalInstallmentsPlan'] as int? ?? 0,
      amount: (json['amount'] as num?)?.toDouble() ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'date': date,
      'name': name,
      'currentInstallment': currentInstallment,
      'totalInstallmentsPlan': totalInstallmentsPlan,
      'amount': amount,
    };
  }
}

class InvoiceDetailsDTO {
  final String invoiceId;
  final String cardId;
  final String? cardName;
  final int month;
  final int year;
  final double totalAmount;
  final int expirationDate;
  final int closeDate;
  final String? status;
  final List<InvoiceItemDTO> items;

  const InvoiceDetailsDTO({
    required this.invoiceId,
    required this.cardId,
    this.cardName,
    this.month = 0,
    this.year = 0,
    this.totalAmount = 0,
    this.expirationDate = 0,
    this.closeDate = 0,
    this.status,
    this.items = const [],
  });

  InvoiceDetailsDTO copyWith({
    String? invoiceId,
    String? cardId,
    String? cardName,
    int? month,
    int? year,
    double? totalAmount,
    int? expirationDate,
    int? closeDate,
    String? status,
    List<InvoiceItemDTO>? items,
  }) {
    return InvoiceDetailsDTO(
      invoiceId: invoiceId ?? this.invoiceId,
      cardId: cardId ?? this.cardId,
      cardName: cardName ?? this.cardName,
      month: month ?? this.month,
      year: year ?? this.year,
      totalAmount: totalAmount ?? this.totalAmount,
      expirationDate: expirationDate ?? this.expirationDate,
      closeDate: closeDate ?? this.closeDate,
      status: status ?? this.status,
      items: items ?? this.items,
    );
  }

  factory InvoiceDetailsDTO.fromJson(Map<String, dynamic> json) {
    return InvoiceDetailsDTO(
      invoiceId: json['invoiceId'] as String? ?? '',
      cardId: json['cardId'] as String? ?? '',
      cardName: json['cardName'] as String?,
      month: json['month'] as int? ?? 0,
      year: json['year'] as int? ?? 0,
      totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0,
      expirationDate: json['expirationDate'] as int? ?? 0,
      closeDate: json['closeDate'] as int? ?? 0,
      status: json['status'] as String?,
      items: (json['items'] as List<dynamic>?)
              ?.map(
                  (e) => InvoiceItemDTO.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'invoiceId': invoiceId,
      'cardId': cardId,
      'cardName': cardName,
      'month': month,
      'year': year,
      'totalAmount': totalAmount,
      'expirationDate': expirationDate,
      'closeDate': closeDate,
      'status': status,
      'items': items.map((e) => e.toJson()).toList(),
    };
  }
}

class RefundRequestDTO {
  final String installmentId;
  final double refundAmount;

  const RefundRequestDTO({
    required this.installmentId,
    this.refundAmount = 0,
  });

  RefundRequestDTO copyWith({
    String? installmentId,
    double? refundAmount,
  }) {
    return RefundRequestDTO(
      installmentId: installmentId ?? this.installmentId,
      refundAmount: refundAmount ?? this.refundAmount,
    );
  }

  factory RefundRequestDTO.fromJson(Map<String, dynamic> json) {
    return RefundRequestDTO(
      installmentId: json['installmentId'] as String? ?? '',
      refundAmount: (json['refundAmount'] as num?)?.toDouble() ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'installmentId': installmentId,
      'refundAmount': refundAmount,
    };
  }
}

class AdvanceRequestDTO {
  final String purchaseId;
  final int quantityToAdvance;
  final double discountAmount;

  const AdvanceRequestDTO({
    required this.purchaseId,
    this.quantityToAdvance = 0,
    this.discountAmount = 0,
  });

  AdvanceRequestDTO copyWith({
    String? purchaseId,
    int? quantityToAdvance,
    double? discountAmount,
  }) {
    return AdvanceRequestDTO(
      purchaseId: purchaseId ?? this.purchaseId,
      quantityToAdvance: quantityToAdvance ?? this.quantityToAdvance,
      discountAmount: discountAmount ?? this.discountAmount,
    );
  }

  factory AdvanceRequestDTO.fromJson(Map<String, dynamic> json) {
    return AdvanceRequestDTO(
      purchaseId: json['purchaseId'] as String? ?? '',
      quantityToAdvance: json['quantityToAdvance'] as int? ?? 0,
      discountAmount:
          (json['discountAmount'] as num?)?.toDouble() ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'purchaseId': purchaseId,
      'quantityToAdvance': quantityToAdvance,
      'discountAmount': discountAmount,
    };
  }
}
