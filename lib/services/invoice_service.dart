import 'dart:convert';

import '../models/invoice_models.dart';
import 'api_client.dart';

/// Serviço de faturas de cartão de crédito.
///
/// Migrado de InvoiceService.cs — mantém as rotas exatas da API.
class InvoiceService {
  final ApiClient _apiClient;

  InvoiceService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<InvoiceDetailsDTO?> getInvoiceDetails(
      String cardId, int month, int year) async {
    try {
      final response = await _apiClient.dio
          .get('invoices/card/$cardId/month/$month/year/$year');
      if (response.data != null) {
        final raw = response.data is String
            ? response.data as String
            : jsonEncode(response.data);
        if (raw.contains('Not Found') || raw.contains('404')) return null;
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        return InvoiceDetailsDTO.fromJson(parsed);
      }
      return null;
    } catch (_) {
      return null;
    }
  }

  Future<List<AdvanceablePurchaseDTO>> getAdvanceablePurchases(
      String cardId, int month, int year) async {
    try {
      final response = await _apiClient.dio.get(
          'invoices/card/$cardId/month/$month/year/$year/advanceable');
      if (response.data != null) {
        final List<dynamic> list = response.data is String
            ? jsonDecode(response.data as String) as List<dynamic>
            : response.data as List<dynamic>;
        return list
            .map((e) =>
                AdvanceablePurchaseDTO.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (_) {
      return [];
    }
  }

  Future<bool> processRefund(
      String invoiceId, RefundRequestDTO dto) async {
    try {
      final response = await _apiClient.dio.post(
        'invoices/$invoiceId/refund',
        data: dto.toJson(),
      );
      final body = response.data?.toString() ?? '';
      return !body.contains('Erro');
    } catch (_) {
      return false;
    }
  }

  Future<bool> advanceInstallments(
      String invoiceId, AdvanceRequestDTO dto) async {
    try {
      final response = await _apiClient.dio.post(
        'invoices/$invoiceId/advance',
        data: dto.toJson(),
      );
      final body = response.data?.toString() ?? '';
      return !body.contains('Erro');
    } catch (_) {
      return false;
    }
  }
}
