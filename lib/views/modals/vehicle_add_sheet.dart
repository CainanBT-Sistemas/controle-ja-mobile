import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../models/vehicle.dart';
import '../../providers/service_providers.dart';

class VehicleAddSheet extends ConsumerStatefulWidget {
  final String? vehicleId;

  const VehicleAddSheet({super.key, this.vehicleId});

  @override
  ConsumerState<VehicleAddSheet> createState() => _VehicleAddSheetState();
}

class _VehicleAddSheetState extends ConsumerState<VehicleAddSheet> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _brandController = TextEditingController();
  final _modelController = TextEditingController();
  final _yearController = TextEditingController();
  final _plateController = TextEditingController();
  final _odometerController = TextEditingController();

  static final _plateRegex =
      RegExp(r'^[A-Za-z]{3}-?[0-9][A-Za-z0-9][0-9]{2}$');

  bool _isLoading = false;
  bool _isLoadingData = false;

  bool get _isEditMode => widget.vehicleId != null;

  @override
  void initState() {
    super.initState();
    if (_isEditMode) _loadVehicle();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _brandController.dispose();
    _modelController.dispose();
    _yearController.dispose();
    _plateController.dispose();
    _odometerController.dispose();
    super.dispose();
  }

  Future<void> _loadVehicle() async {
    setState(() => _isLoadingData = true);
    try {
      final service = ref.read(vehicleServiceProvider);
      final vehicles = await service.getVehicles();
      final vehicle =
          vehicles.where((v) => v.id == widget.vehicleId).firstOrNull;
      if (vehicle != null && mounted) {
        _nameController.text = vehicle.name;
        _brandController.text = vehicle.brand;
        _modelController.text = vehicle.model;
        if (vehicle.year > 0) {
          _yearController.text = vehicle.year.toString();
        }
        _plateController.text = vehicle.plate;
        _odometerController.text =
            vehicle.currentOdometer.toStringAsFixed(0);
      }
    } finally {
      if (mounted) setState(() => _isLoadingData = false);
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);
    try {
      final service = ref.read(vehicleServiceProvider);
      final vehicle = Vehicle(
        id: widget.vehicleId ?? '',
        name: _nameController.text.trim(),
        brand: _brandController.text.trim(),
        model: _modelController.text.trim(),
        year: int.tryParse(_yearController.text.trim()) ?? 0,
        plate: _plateController.text.trim().toUpperCase(),
        currentOdometer:
            double.tryParse(_odometerController.text.trim()) ?? 0,
      );

      final success = _isEditMode
          ? await service.updateVehicle(widget.vehicleId!, vehicle)
          : await service.saveVehicle(vehicle);

      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao salvar veículo'),
            backgroundColor: Color(0xFFFF5252),
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Erro: $e'),
            backgroundColor: const Color(0xFFFF5252),
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _delete() async {
    if (!_isEditMode) return;
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: const Color(0xFF1E2A3A),
        title: const Text('Excluir veículo',
            style: TextStyle(color: Colors.white)),
        content: const Text(
            'Tem certeza que deseja excluir este veículo?',
            style: TextStyle(color: Color(0xFF94A3B8))),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Cancelar',
                style: TextStyle(color: Color(0xFF94A3B8))),
          ),
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Excluir',
                style: TextStyle(color: Color(0xFFFF5252))),
          ),
        ],
      ),
    );
    if (confirmed != true) return;

    setState(() => _isLoading = true);
    try {
      final service = ref.read(vehicleServiceProvider);
      final success = await service.deleteVehicle(widget.vehicleId!);
      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao excluir veículo'),
            backgroundColor: Color(0xFFFF5252),
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  InputDecoration _inputDecoration(String label, IconData icon) {
    return InputDecoration(
      labelText: label,
      labelStyle: const TextStyle(color: Color(0xFF94A3B8)),
      filled: true,
      fillColor: const Color(0xFF1E2A3A),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide.none,
      ),
      prefixIcon: Icon(icon, color: const Color(0xFF94A3B8)),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: BoxConstraints(
        maxHeight: MediaQuery.of(context).size.height * 0.9,
      ),
      decoration: const BoxDecoration(
        color: Color(0xFF0A0F17),
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          _buildDragHandle(),
          _buildHeader(),
          const Divider(color: Color(0xFF1E293B), height: 1),
          Flexible(
            child: _isLoadingData
                ? const Center(
                    child: Padding(
                      padding: EdgeInsets.all(32),
                      child: CircularProgressIndicator(
                          color: Color(0xFF00E676)),
                    ),
                  )
                : SingleChildScrollView(
                    padding: const EdgeInsets.all(20),
                    child: Form(
                      key: _formKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          TextFormField(
                            controller: _nameController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Apelido do veículo',
                                Icons.directions_car),
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o nome'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _brandController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Marca', Icons.branding_watermark),
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _modelController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Modelo', Icons.local_offer),
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o modelo'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _yearController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Ano', Icons.calendar_today),
                            keyboardType: TextInputType.number,
                            inputFormatters: [
                              FilteringTextInputFormatter.digitsOnly
                            ],
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _plateController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Placa (ABC-1234 ou ABC1D23)',
                                Icons.confirmation_number),
                            textCapitalization:
                                TextCapitalization.characters,
                            validator: (v) {
                              if (v == null || v.trim().isEmpty) {
                                return 'Informe a placa';
                              }
                              if (!_plateRegex.hasMatch(v.trim())) {
                                return 'Formato inválido';
                              }
                              return null;
                            },
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _odometerController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Hodômetro atual (km)',
                                Icons.speed),
                            keyboardType: TextInputType.number,
                            inputFormatters: [
                              FilteringTextInputFormatter.digitsOnly
                            ],
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o hodômetro'
                                    : null,
                          ),
                          const SizedBox(height: 24),
                          _buildSaveButton(),
                          const SizedBox(height: 16),
                        ],
                      ),
                    ),
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildDragHandle() {
    return Container(
      margin: const EdgeInsets.only(top: 12),
      width: 40,
      height: 4,
      decoration: BoxDecoration(
        color: const Color(0xFF334155),
        borderRadius: BorderRadius.circular(2),
      ),
    );
  }

  Widget _buildHeader() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 16, 12, 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            _isEditMode ? 'Editar Veículo' : 'Novo Veículo',
            style: const TextStyle(
              color: Colors.white,
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
          if (_isEditMode)
            IconButton(
              icon: const Icon(Icons.delete_outline,
                  color: Color(0xFFFF5252)),
              onPressed: _isLoading ? null : _delete,
            ),
        ],
      ),
    );
  }

  Widget _buildSaveButton() {
    return SizedBox(
      height: 50,
      child: ElevatedButton(
        onPressed: _isLoading ? null : _save,
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color(0xFF00E676),
          foregroundColor: Colors.black,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
        child: _isLoading
            ? const SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(
                  color: Colors.black,
                  strokeWidth: 2,
                ),
              )
            : Text(
                _isEditMode
                    ? 'Salvar Alterações'
                    : 'Criar Veículo',
                style: const TextStyle(
                    fontSize: 16, fontWeight: FontWeight.bold),
              ),
      ),
    );
  }
}
