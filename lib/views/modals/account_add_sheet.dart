import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/theme.dart';
import '../../models/account.dart';
import '../../models/enums.dart';
import '../../providers/service_providers.dart';

class AccountAddSheet extends ConsumerStatefulWidget {
  final String? accountId;

  const AccountAddSheet({super.key, this.accountId});

  @override
  ConsumerState<AccountAddSheet> createState() => _AccountAddSheetState();
}

class _AccountAddSheetState extends ConsumerState<AccountAddSheet> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _institutionController = TextEditingController();
  final _balanceController = TextEditingController();
  final _currencyFormat =
      NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');

  AccountType _selectedType = AccountType.BANK;
  String _selectedIcon = AppIconLists.accountIcons.first;
  String _selectedColor = AppColors.availableColorHex[5];
  bool _isDefault = false;
  bool _isLoading = false;
  bool _isLoadingData = false;

  bool get _isEditMode => widget.accountId != null;

  static const _accountTypeLabels = {
    AccountType.WALLET: 'Carteira',
    AccountType.BANK: 'Banco',
    AccountType.SAVINGS: 'Poupança',
  };

  @override
  void initState() {
    super.initState();
    _balanceController.text = _currencyFormat.format(0);
    if (_isEditMode) _loadAccount();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _institutionController.dispose();
    _balanceController.dispose();
    super.dispose();
  }

  Future<void> _loadAccount() async {
    setState(() => _isLoadingData = true);
    try {
      final service = ref.read(accountServiceProvider);
      final account = await service.getAccountById(widget.accountId!);
      if (account != null && mounted) {
        _nameController.text = account.name;
        _institutionController.text = account.institution;
        _balanceController.text =
            _currencyFormat.format(account.currentBalance);
        setState(() {
          _selectedType = account.type;
          _selectedIcon = account.icon;
          _selectedColor = account.color;
          _isDefault = account.isDefault;
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
    if (_balanceController.text != formatted) {
      _balanceController.value = TextEditingValue(
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
      final service = ref.read(accountServiceProvider);
      final account = Account(
        id: widget.accountId ?? '',
        name: _nameController.text.trim(),
        type: _selectedType,
        institution: _institutionController.text.trim(),
        currentBalance: _parseCurrency(_balanceController.text),
        icon: _selectedIcon,
        color: _selectedColor,
        isDefault: _isDefault,
      );

      final success = _isEditMode
          ? await service.updateAccount(widget.accountId!, account.toJson())
          : await service.saveAccount(account);

      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao salvar conta'),
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
    if (!_isEditMode || _isDefault) return;
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: const Color(0xFF1E2A3A),
        title: const Text('Excluir conta',
            style: TextStyle(color: Colors.white)),
        content: const Text('Tem certeza que deseja excluir esta conta?',
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
      final service = ref.read(accountServiceProvider);
      final success = await service.deleteAccount(widget.accountId!);
      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao excluir conta'),
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
                            decoration:
                                _inputDecoration('Nome da conta', Icons.edit),
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o nome'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _institutionController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Instituição', Icons.business),
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _balanceController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Saldo', Icons.attach_money),
                            keyboardType: TextInputType.number,
                            onChanged: _formatCurrency,
                          ),
                          const SizedBox(height: 16),
                          DropdownButtonFormField<AccountType>(
                            value: _selectedType,
                            dropdownColor: const Color(0xFF1E2A3A),
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Tipo de conta', Icons.account_balance),
                            items: _accountTypeLabels.entries
                                .map((e) => DropdownMenuItem(
                                    value: e.key, child: Text(e.value)))
                                .toList(),
                            onChanged: (v) {
                              if (v != null) {
                                setState(() => _selectedType = v);
                              }
                            },
                          ),
                          const SizedBox(height: 16),
                          SwitchListTile(
                            title: const Text('Conta padrão',
                                style: TextStyle(color: Colors.white)),
                            subtitle: const Text(
                                'Usada como padrão para transações',
                                style: TextStyle(
                                    color: Color(0xFF94A3B8),
                                    fontSize: 12)),
                            value: _isDefault,
                            activeColor: const Color(0xFF00E676),
                            tileColor: const Color(0xFF1E2A3A),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12)),
                            onChanged: (v) =>
                                setState(() => _isDefault = v),
                          ),
                          const SizedBox(height: 20),
                          _buildIconSelector(),
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
            _isEditMode ? 'Editar Conta' : 'Nova Conta',
            style: const TextStyle(
              color: Colors.white,
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
          if (_isEditMode && !_isDefault)
            IconButton(
              icon: const Icon(Icons.delete_outline,
                  color: Color(0xFFFF5252)),
              onPressed: _isLoading ? null : _delete,
            ),
        ],
      ),
    );
  }

  Widget _buildIconSelector() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Ícone',
            style: TextStyle(color: Color(0xFF94A3B8), fontSize: 14)),
        const SizedBox(height: 8),
        Wrap(
          spacing: 12,
          runSpacing: 12,
          children: AppIconLists.accountIcons.map((iconName) {
            final selected = _selectedIcon == iconName;
            return GestureDetector(
              onTap: () => setState(() => _selectedIcon = iconName),
              child: Container(
                width: 48,
                height: 48,
                decoration: BoxDecoration(
                  color: selected
                      ? AppColors.fromHex(_selectedColor)
                      : const Color(0xFF1E2A3A),
                  borderRadius: BorderRadius.circular(12),
                  border: selected
                      ? Border.all(
                          color: const Color(0xFF00E676), width: 2)
                      : null,
                ),
                child: Icon(
                  AppMaterialIcons.fromName(iconName),
                  color: Colors.white,
                  size: 24,
                ),
              ),
            );
          }).toList(),
        ),
      ],
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
                _isEditMode ? 'Salvar Alterações' : 'Criar Conta',
                style: const TextStyle(
                    fontSize: 16, fontWeight: FontWeight.bold),
              ),
      ),
    );
  }
}
