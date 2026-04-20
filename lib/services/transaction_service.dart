import 'dart:convert';

import '../models/transaction.dart';
import 'api_client.dart';

/// Serviço de transações financeiras.
///
/// Migrado de TransactionService.cs — mantém as rotas exatas da API.
class TransactionService {
  final ApiClient _apiClient;

  TransactionService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<Transaction>> getTransactions({
    int? start,
    int? end,
  }) async {
    try {
      String url = 'transactions';
      if (start != null && end != null) {
        url += '?start=$start&end=$end';
      }
      final response = await _apiClient.dio.get(url);
      if (response.data != null) {
        final List<dynamic> list = response.data is String
            ? jsonDecode(response.data as String) as List<dynamic>
            : response.data as List<dynamic>;
        return list
            .map((e) => Transaction.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<bool> saveTransaction(Transaction transaction) async {
    try {
      final response = await _apiClient.dio.post(
        'transactions',
        data: transaction.toJson(),
      );
      return response.data != null;
    } catch (e) {
      return false;
    }
  }

  Future<bool> updateTransaction(
    String id,
    Transaction transaction, {
    bool updateFuture = false,
  }) async {
    try {
      final response = await _apiClient.dio.put(
        'transactions/$id?updateFuture=$updateFuture',
        data: transaction.toJson(),
      );
      return response.data != null;
    } catch (e) {
      return false;
    }
  }

  Future<bool> deleteTransaction(
    String id, {
    bool cancelFuture = false,
  }) async {
    try {
      final response = await _apiClient.dio
          .delete('transactions/$id?cancelFuture=$cancelFuture');
      return response.data != null;
    } catch (e) {
      return false;
    }
  }
}
