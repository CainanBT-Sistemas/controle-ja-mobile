import 'dart:convert';

import '../models/account.dart';
import 'api_client.dart';

/// Serviço de contas bancárias.
///
/// Migrado de AccountService.cs — mantém as rotas exatas da API.
class AccountService {
  final ApiClient _apiClient;

  AccountService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<Account>> getAccounts() async {
    try {
      final response = await _apiClient.dio.get('accounts');
      if (response.data != null) {
        final List<dynamic> list = response.data is String
            ? jsonDecode(response.data as String) as List<dynamic>
            : response.data as List<dynamic>;
        return list
            .map((e) => Account.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<bool> saveAccount(Account account) async {
    try {
      final response = await _apiClient.dio.post(
        'accounts',
        data: account.toJson(),
      );
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        final saved = Account.fromJson(parsed);
        return saved.id.isNotEmpty;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  Future<Account?> getAccountById(String id) async {
    try {
      final response = await _apiClient.dio.get('accounts/$id');
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        return Account.fromJson(parsed);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  Future<bool> updateAccount(String id, Map<String, dynamic> data) async {
    try {
      final response = await _apiClient.dio.put(
        'accounts/$id',
        data: data,
      );
      return response.data != null;
    } catch (e) {
      return false;
    }
  }

  Future<bool> deleteAccount(String id) async {
    try {
      final response = await _apiClient.dio.delete('accounts/$id');
      return response.data != null;
    } catch (e) {
      return false;
    }
  }
}
