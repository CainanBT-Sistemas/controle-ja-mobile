import 'dart:convert';

import '../models/category.dart';
import 'api_client.dart';

/// Serviço de categorias.
///
/// Migrado de CategoryService.cs — mantém as rotas exatas da API.
class CategoryService {
  final ApiClient _apiClient;

  CategoryService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<Category>> getCategoriesAsync() async {
    try {
      final response = await _apiClient.dio.get('categories');
      if (response.data != null) {
        final List<dynamic> list = response.data is String
            ? jsonDecode(response.data as String) as List<dynamic>
            : response.data as List<dynamic>;
        return list
            .map((e) => Category.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<Category?> saveCategoryAsync(Category category) async {
    try {
      final response = await _apiClient.dio.post(
        'categories',
        data: category.toJson(),
      );
      if (response.data != null) {
        final parsed = response.data is String
            ? jsonDecode(response.data as String) as Map<String, dynamic>
            : response.data as Map<String, dynamic>;
        return Category.fromJson(parsed);
      }
      return null;
    } catch (e) {
      return null;
    }
  }
}
