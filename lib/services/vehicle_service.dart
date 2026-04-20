import 'dart:convert';

import '../models/vehicle.dart';
import 'api_client.dart';

/// Serviço de veículos.
///
/// Migrado de VehicleService.cs — mantém as rotas exatas da API.
class VehicleService {
  final ApiClient _apiClient;

  VehicleService({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<Vehicle>> getVehicles() async {
    try {
      final response = await _apiClient.dio.get('vehicles');
      if (response.data != null) {
        final List<dynamic> list = response.data is String
            ? jsonDecode(response.data as String) as List<dynamic>
            : response.data as List<dynamic>;
        return list
            .map((e) => Vehicle.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<bool> saveVehicle(Vehicle vehicle) async {
    try {
      final response = await _apiClient.dio.post(
        'vehicles',
        data: vehicle.toJson(),
      );
      return response.data != null;
    } catch (e) {
      return false;
    }
  }

  Future<bool> updateVehicle(String id, Vehicle vehicle) async {
    try {
      final response = await _apiClient.dio.put(
        'vehicles/$id',
        data: vehicle.toJson(),
      );
      return response.data != null;
    } catch (e) {
      return false;
    }
  }

  Future<bool> deleteVehicle(String id) async {
    try {
      final response = await _apiClient.dio.delete('vehicles/$id');
      return response.data != null;
    } catch (e) {
      return false;
    }
  }
}
