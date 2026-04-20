import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/theme.dart';
import '../../models/credit_card.dart';
import '../../providers/service_providers.dart';

class CreditCardAddSheet extends ConsumerStatefulWidget {
  final String? cardId;

  const CreditCardAddSheet({super.key, this.cardId});

  @override
  ConsumerState<CreditCardAddSheet> createState() =>
      _CreditCardAddSheetState();
}

class _CreditCardAddSheetState extends ConsumerState<CreditCardAddSheet> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _limitController = TextEditingController();
  final _closeDayController = TextEditingController();
  final _bestDayController = TextEditingController();
  final _currencyFormat =
      NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');

  String _selectedColor = AppColors.availableColorHex[2]; // Purple
  String _existingAccountId = '';
  bool _isLoading = false;
  bool _isLoadingData = false;

  bool get _isEditMode => widget.cardId != null;

  @override
  void initState() {
    super.initState();
    _limitController.text = _currencyFormat.format(0);
    if (_isEditMode) _loadCard();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _limitController.dispose();
    _closeDayController.dispose();
    _bestDayController.dispose();
    super.dispose();
  }

  Future<void> _loadCard() async {
    setState(() => _isLoadingData = true);
    try {
      final service = ref.read(creditCardServiceProvider);
      final card = await service.getCreditCardById(widget.cardId!);
      if (card != null && mounted) {
        _nameController.text = card.name;
        _limitController.text = _currencyFormat.format(card.totalLimit);
        _closeDayController.text = card.closeDay.toString();
        _bestDayController.text = card.bestDay.toString();
        setState(() {
          _selectedColor = card.color;
          _existingAccountId = card.accountId;
        });
      }
    } finally {
      if (mounted) setState(() => _isLoadingData = false);
    }
  }

  void _formatCurrency(String value) {
    final digits = value.replaceAll(RegExp(r'[^\d]'), '');
    final number = (int.tryParse(digits) ?? 0) / 100;
    final formatted = _currencyFormat.format(number);
    if (_limitController.text != formatted) {
      _limitController.value = TextEditingValue(
        text: formatted,
        selection: TextSelection.collapsed(offset: formatted.length),
      );
    }
  }

  double _parseCurrency(String text) {
    final digits = text.replaceAll(RegExp(r'[^\d]'), '');
    return (int.tryParse(digits) ?? 0) / 100;
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);
    try {
      final service = ref.read(creditCardServiceProvider);
      final limit = _parseCurrency(_limitController.text);
      final card = CreditCard(
        id: widget.cardId ?? '',
        accountId: _existingAccountId,
        name: _nameController.text.trim(),
        totalLimit: limit,
        currentLimit: _isEditMode ? limit : limit,
        closeDay: int.parse(_closeDayController.text.trim()),
        bestDay: int.parse(_bestDayController.text.trim()),
        icon: 'credit_card',
        color: _selectedColor,
      );

      final success = _isEditMode
          ? await service.updateCreditCard(widget.cardId!, card.toJson())
          : await service.saveCreditCard(card);

      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao salvar cartão'),
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
        title: const Text('Excluir cartão',
            style: TextStyle(color: Colors.white)),
        content: const Text(
            'Tem certeza que deseja excluir este cartão?',
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
      final service = ref.read(creditCardServiceProvider);
      final success = await service.deleteCreditCard(widget.cardId!);
      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao excluir cartão'),
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
                                'Nome do cartão', Icons.edit),
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o nome'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _limitController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Limite', Icons.attach_money),
                            keyboardType: TextInputType.number,
                            onChanged: _formatCurrency,
                            validator: (v) =>
                                _parseCurrency(v ?? '') <= 0
                                    ? 'Informe o limite'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          Row(
                            children: [
                              Expanded(
                                child: TextFormField(
                                  controller: _closeDayController,
                                  style: const TextStyle(
                                      color: Colors.white),
                                  decoration: _inputDecoration(
                                      'Dia fechamento',
                                      Icons.calendar_today),
                                  keyboardType:
                                      TextInputType.number,
                                  validator: (v) {
                                    final day =
                                        int.tryParse(v ?? '');
                                    if (day == null ||
                                        day < 1 ||
                                        day > 31) {
                                      return '1-31';
                                    }
                                    return null;
                                  },
                                ),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: TextFormField(
                                  controller: _bestDayController,
                                  style: const TextStyle(
                                      color: Colors.white),
                                  decoration: _inputDecoration(
                                      'Melhor dia',
                                      Icons.event_available),
                                  keyboardType:
                                      TextInputType.number,
                                  validator: (v) {
                                    final day =
                                        int.tryParse(v ?? '');
                                    if (day == null ||
                                        day < 1 ||
                                        day > 31) {
                                      return '1-31';
                                    }
                                    return null;
                                  },
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 20),
                          _buildColorSelector(),
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
            _isEditMode ? 'Editar Cartão' : 'Novo Cartão',
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

  Widget _buildColorSelector() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Cor',
            style: TextStyle(color: Color(0xFF94A3B8), fontSize: 14)),
        const SizedBox(height: 8),
        Wrap(
          spacing: 10,
          runSpacing: 10,
          children: AppColors.availableColorHex.map((hex) {
            final selected = _selectedColor == hex;
            return GestureDetector(
              onTap: () => setState(() => _selectedColor = hex),
              child: Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(
                  color: AppColors.fromHex(hex),
                  shape: BoxShape.circle,
                  border: selected
                      ? Border.all(color: Colors.white, width: 3)
                      : null,
                ),
                child: selected
                    ? const Icon(Icons.check,
                        color: Colors.white, size: 18)
                    : null,
              ),
            );
          }).toList(),
        ),
      ],
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
                _isEditMode ? 'Salvar Alterações' : 'Criar Cartão',
                style: const TextStyle(
                    fontSize: 16, fontWeight: FontWeight.bold),
              ),
      ),
    );
  }
}
