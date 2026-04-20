import 'dart:convert';

import '../models/dashboard_data.dart';
import 'api_client.dart';

/// Serviço do dashboard.
///
/// Migrado de DashboardService.cs — mantém todas as rotas exatas da API.
class DashboardService {
  final ApiClient _apiClient;

  DashboardService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<ChartData>> getExpensesByCategory(int start, int end) async {
    try {
      final response = await _apiClient.dio
          .get('dashboard/expenses-category?start=$start&end=$end');
      return _parseChartDataList(response.data);
    } catch (e) {
      return [];
    }
  }

  Future<List<ChartData>> getCreditExpensesByCategory(
      int start, int end) async {
    try {
      final response = await _apiClient.dio
          .get('dashboard/credit-expenses-category?start=$start&end=$end');
      return _parseChartDataList(response.data);
    } catch (e) {
      return [];
    }
  }

  Future<List<ChartData>> getIncomesByCategory(int start, int end) async {
    try {
      final response = await _apiClient.dio
          .get('dashboard/incomes-category?start=$start&end=$end');
      return _parseChartDataList(response.data);
    } catch (e) {
      return [];
    }
  }

  Future<List<ChartData>> getFuelComparison(int start, int end) async {
    try {
      final response = await _apiClient.dio
          .get('dashboard/fuel-comparison?start=$start&end=$end');
      return _parseChartDataList(response.data);
    } catch (e) {
      return [];
    }
  }

  Future<FinancialSummary?> getDashboardSummary(int start, int end) async {
    try {
      final response = await _apiClient.dio
          .get('dashboard/summary?start=$start&end=$end');
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        return FinancialSummary.fromJson(parsed);
      }
      return FinancialSummary();
    } catch (e) {
      return FinancialSummary();
    }
  }

  Future<List<ChartData>> getEvolution(
      int start, int end, String? uuid) async {
    try {
      String endpoint = 'dashboard/evolution?start=$start&end=$end';
      if (uuid != null && uuid.isNotEmpty) {
        endpoint += '&categoryId=$uuid';
      }
      final response = await _apiClient.dio.get(endpoint);
      return _parseChartDataList(response.data);
    } catch (e) {
      return [];
    }
  }

  Future<DashboardFullSummary?> getFullSummary(
      int start, int end) async {
    try {
      final response = await _apiClient.dio
          .get('dashboard/full-summary?start=$start&end=$end');
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        return DashboardFullSummary.fromJson(parsed);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  // --- Helper ---

  List<ChartData> _parseChartDataList(dynamic data) {
    if (data != null) {
      final List<dynamic> list = data is String
          ? jsonDecode(data) as List<dynamic>
          : data as List<dynamic>;
      return list
          .map((e) => ChartData.fromJson(e as Map<String, dynamic>))
          .toList();
    }
    return [];
  }
}
