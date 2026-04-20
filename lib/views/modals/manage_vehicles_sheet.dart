import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../providers/vehicles_notifier.dart';
import 'vehicle_add_sheet.dart';

class ManageVehiclesSheet extends ConsumerStatefulWidget {
  const ManageVehiclesSheet({super.key});

  @override
  ConsumerState<ManageVehiclesSheet> createState() =>
      _ManageVehiclesSheetState();
}

class _ManageVehiclesSheetState extends ConsumerState<ManageVehiclesSheet> {
  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref.read(vehiclesListNotifierProvider.notifier).loadVehicles(),
    );
  }

  String _formatOdometer(double value) {
    return NumberFormat('#,##0', 'pt_BR').format(value);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(vehiclesListNotifierProvider);

    return Container(
      constraints:
          BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.85),
      decoration: const BoxDecoration(
        color: Color(0xFF0A0F17),
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // Drag handle
          Container(
            margin: const EdgeInsets.only(top: 12),
            width: 40,
            height: 4,
            decoration: BoxDecoration(
              color: const Color(0xFF334155),
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 12, 8),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Veículos',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                IconButton(
                  icon: const Icon(
                    Icons.add_circle_outline,
                    color: Color(0xFF00E676),
                    size: 28,
                  ),
                  onPressed: () => _openAddSheet(context),
                ),
              ],
            ),
          ),
          const Divider(color: Color(0xFF1E293B), height: 1),
          // Content
          Flexible(
            child: state.isLoading
                ? const Center(
                    child: Padding(
                      padding: EdgeInsets.all(32),
                      child: CircularProgressIndicator(
                        color: Color(0xFF00E676),
                      ),
                    ),
                  )
                : state.vehicles.isEmpty
                    ? const Padding(
                        padding: EdgeInsets.all(48),
                        child: Text(
                          'Nenhum veículo cadastrado',
                          style: TextStyle(
                            color: Color(0xFF94A3B8),
                            fontSize: 16,
                          ),
                        ),
                      )
                    : ListView.builder(
                        shrinkWrap: true,
                        itemCount: state.vehicles.length,
                        padding: const EdgeInsets.symmetric(vertical: 8),
                        itemBuilder: (context, index) {
                          final vehicle = state.vehicles[index];
                          return ListTile(
                            leading: const CircleAvatar(
                              backgroundColor: Color(0xFF1E2A3A),
                              child: Icon(
                                Icons.directions_car,
                                color: Color(0xFF00E676),
                                size: 20,
                              ),
                            ),
                            title: Text(
                              vehicle.name.isNotEmpty
                                  ? vehicle.name
                                  : vehicle.fullDescription,
                              style: const TextStyle(color: Colors.white),
                            ),
                            subtitle: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                if (vehicle.name.isNotEmpty)
                                  Text(
                                    vehicle.fullDescription,
                                    style: const TextStyle(
                                      color: Color(0xFF94A3B8),
                                      fontSize: 12,
                                    ),
                                  ),
                                if (vehicle.plate.isNotEmpty)
                                  Text(
                                    'Placa: ${vehicle.plate}',
                                    style: const TextStyle(
                                      color: Color(0xFF94A3B8),
                                      fontSize: 12,
                                    ),
                                  ),
                              ],
                            ),
                            trailing: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              crossAxisAlignment: CrossAxisAlignment.end,
                              children: [
                                Text(
                                  '${_formatOdometer(vehicle.currentOdometer)} km',
                                  style: const TextStyle(
                                    color: Color(0xFF00E676),
                                    fontWeight: FontWeight.bold,
                                    fontSize: 13,
                                  ),
                                ),
                                const Text(
                                  'Odômetro',
                                  style: TextStyle(
                                    color: Color(0xFF94A3B8),
                                    fontSize: 10,
                                  ),
                                ),
                              ],
                            ),
                            isThreeLine: vehicle.name.isNotEmpty &&
                                vehicle.plate.isNotEmpty,
                            onTap: () => _openAddSheet(
                              context,
                              vehicleId: vehicle.id,
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }

  void _openAddSheet(BuildContext context, {String? vehicleId}) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => VehicleAddSheet(vehicleId: vehicleId),
    ).then(
      (_) => ref.read(vehiclesListNotifierProvider.notifier).loadVehicles(),
    );
  }
}

/// Helper to show this sheet.
void showManageVehiclesSheet(BuildContext context) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.transparent,
    builder: (_) => const ManageVehiclesSheet(),
  );
}
