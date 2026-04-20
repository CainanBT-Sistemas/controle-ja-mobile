import 'dart:convert';

import '../models/credit_card.dart';
import 'api_client.dart';

/// Serviço de cartões de crédito.
///
/// Migrado de CreditCardService.cs — mantém as rotas exatas da API.
class CreditCardService {
  final ApiClient _apiClient;

  CreditCardService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<CreditCard>> getCreditCards() async {
    try {
      final response = await _apiClient.dio.get('cards');
      if (response.data != null) {
        final List<dynamic> list = response.data is String
            ? jsonDecode(response.data as String) as List<dynamic>
            : response.data as List<dynamic>;
        return list
            .map((e) => CreditCard.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<bool> saveCreditCard(CreditCard card) async {
    try {
      final response = await _apiClient.dio.post(
        'cards',
        data: card.toJson(),
      );
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        final saved = CreditCard.fromJson(parsed);
        return saved.id.isNotEmpty;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  Future<CreditCard?> getCreditCardById(String id) async {
    try {
      final response = await _apiClient.dio.get('cards/$id');
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        return CreditCard.fromJson(parsed);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  Future<bool> updateCreditCard(String id, Map<String, dynamic> data) async {
    try {
      final response = await _apiClient.dio.put(
        'cards/$id',
        data: data,
      );
      return response.data != null;
    } catch (e) {
      return false;
    }
  }

  Future<bool> deleteCreditCard(String id) async {
    try {
      final response = await _apiClient.dio.delete('cards/$id');
      return response.data != null;
    } catch (e) {
      return false;
    }
  }
}
