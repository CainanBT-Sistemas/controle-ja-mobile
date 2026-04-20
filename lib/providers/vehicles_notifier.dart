import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/error_handler.dart';
import '../models/vehicle.dart';
import '../services/vehicle_service.dart';
import 'service_providers.dart';

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/// Estado do VehiclesListNotifier.
///
/// Migrado de VehiclesViewModel.cs — lista de veículos.
class VehiclesListState {
  final bool isLoading;
  final List<Vehicle> vehicles;
  final UserFriendlyError? error;

  const VehiclesListState({
    this.isLoading = false,
    this.vehicles = const [],
    this.error,
  });

  VehiclesListState copyWith({
    bool? isLoading,
    List<Vehicle>? vehicles,
    UserFriendlyError? error,
    bool clearError = false,
  }) {
    return VehiclesListState(
      isLoading: isLoading ?? this.isLoading,
      vehicles: vehicles ?? this.vehicles,
      error: clearError ? null : (error ?? this.error),
    );
  }
}

// ---------------------------------------------------------------------------
// Notifier
// ---------------------------------------------------------------------------

/// Notifier para a lista de veículos.
///
/// Migrado de VehiclesViewModel.cs — carrega e exclui veículos via
/// VehicleService.
class VehiclesListNotifier extends StateNotifier<VehiclesListState> {
  final VehicleService _vehicleService;

  VehiclesListNotifier(this._vehicleService)
      : super(const VehiclesListState());

  /// Carrega todos os veículos do usuário.
  Future<void> loadVehicles() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final vehicles = await _vehicleService.getVehicles();
      state = state.copyWith(vehicles: vehicles);
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  /// Exclui um veículo e recarrega a lista.
  Future<bool> deleteVehicle(String id) async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _vehicleService.deleteVehicle(id);
      if (success) {
        await loadVehicles();
      }
      return success;
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
      return false;
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  void clearError() => state = state.copyWith(clearError: true);
}

// ---------------------------------------------------------------------------
// Providers
// ---------------------------------------------------------------------------

/// Provider do VehiclesListNotifier.
final vehiclesListNotifierProvider =
    StateNotifierProvider<VehiclesListNotifier, VehiclesListState>((ref) {
  final service = ref.watch(vehicleServiceProvider);
  return VehiclesListNotifier(service);
});
