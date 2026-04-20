import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../models/account.dart';
import '../../models/category.dart';
import '../../models/credit_card.dart';
import '../../models/enums.dart';
import '../../models/transaction.dart';
import '../../providers/service_providers.dart';

/// Internal mode to distinguish card expenses from regular expenses.
enum _TxMode { expense, cardExpense, income, transfer }

class TransactionAddSheet extends ConsumerStatefulWidget {
  final String? transactionType;
  final Transaction? transactionToEdit;

  const TransactionAddSheet({
    super.key,
    this.transactionType,
    this.transactionToEdit,
  });

  @override
  ConsumerState<TransactionAddSheet> createState() =>
      _TransactionAddSheetState();
}

class _TransactionAddSheetState extends ConsumerState<TransactionAddSheet> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _amountController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _installmentsController = TextEditingController(text: '1');
  final _currencyFormat =
      NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');
  final _dateFormat = DateFormat('dd/MM/yyyy');

  _TxMode _mode = _TxMode.expense;
  DateTime _selectedDate = DateTime.now();
  bool _isPaid = false;
  bool _isFixed = false;
  RecurrenceFrequency _recurrenceFrequency = RecurrenceFrequency.MONTHLY;
  bool _showMoreDetails = false;

  // Selection state
  String? _selectedCategoryId;
  String? _selectedCategoryName;
  String? _selectedAccountId;
  String? _selectedAccountName;
  String? _selectedTargetAccountId;
  String? _selectedTargetAccountName;
  String? _selectedCardId;
  String? _selectedCardName;

  // Loaded data for selectors
  List<Account> _accounts = [];
  List<Category> _categories = [];
  List<CreditCard> _cards = [];

  bool _isLoading = false;
  bool _isLoadingData = false;

  bool get _isEditMode => widget.transactionToEdit != null;

  static const _modeLabels = {
    _TxMode.expense: 'Despesa',
    _TxMode.cardExpense: 'Cartão',
    _TxMode.income: 'Receita',
    _TxMode.transfer: 'Transferência',
  };

  static Color _modeColor(_TxMode mode) {
    switch (mode) {
      case _TxMode.expense:
        return const Color(0xFFFF5252);
      case _TxMode.cardExpense:
        return const Color(0xFF9C27B0);
      case _TxMode.income:
        return const Color(0xFF00E676);
      case _TxMode.transfer:
        return const Color(0xFF2196F3);
    }
  }

  @override
  void initState() {
    super.initState();
    _amountController.text = _currencyFormat.format(0);

    // Set initial mode
    if (widget.transactionType != null) {
      _mode = _modeFromTypeName(widget.transactionType!);
    }

    if (_isEditMode) {
      _populateFromTransaction(widget.transactionToEdit!);
    }

    _loadSelectors();
  }

  _TxMode _modeFromTypeName(String typeName) {
    switch (typeName) {
      case 'RECEITA':
        return _TxMode.income;
      case 'TRANSFERENCIA':
        return _TxMode.transfer;
      case 'DESPESA':
      default:
        return _TxMode.expense;
    }
  }

  void _populateFromTransaction(Transaction tx) {
    _nameController.text = tx.name;
    _amountController.text = _currencyFormat.format(tx.amount);
    _descriptionController.text = tx.description ?? '';
    _installmentsController.text = tx.installments.toString();
    _selectedDate = tx.date > 0
        ? DateTime.fromMillisecondsSinceEpoch(tx.date)
        : DateTime.now();
    _isPaid = tx.paid;
    _isFixed = tx.isFixed;
    _recurrenceFrequency =
        tx.recurrenceFrequency ?? RecurrenceFrequency.MONTHLY;
    _selectedCategoryId = tx.categoryId;
    _selectedCategoryName = tx.categoryName;
    _selectedAccountId = tx.accountId;
    _selectedAccountName = tx.accountName;
    _selectedTargetAccountId = tx.targetAccountId;
    _selectedCardId = tx.creditCardId;

    if (tx.description?.isNotEmpty == true || tx.isFixed) {
      _showMoreDetails = true;
    }

    // Determine mode
    if (tx.type == TransactionType.RECEITA) {
      _mode = _TxMode.income;
    } else if (tx.type == TransactionType.TRANSFERENCIA ||
        tx.type == TransactionType.TRANSFERENCIA_SAIDA) {
      _mode = _TxMode.transfer;
    } else if (tx.creditCardId != null &&
        tx.creditCardId!.isNotEmpty) {
      _mode = _TxMode.cardExpense;
    } else {
      _mode = _TxMode.expense;
    }
  }

  Future<void> _loadSelectors() async {
    setState(() => _isLoadingData = true);
    try {
      final accountService = ref.read(accountServiceProvider);
      final categoryService = ref.read(categoryServiceProvider);
      final cardService = ref.read(creditCardServiceProvider);

      final results = await Future.wait([
        accountService.getAccounts(),
        categoryService.getCategories(),
        cardService.getCreditCards(),
      ]);

      if (mounted) {
        setState(() {
          _accounts = results[0] as List<Account>;
          _categories = results[1] as List<Category>;
          _cards = results[2] as List<CreditCard>;

          // Resolve names for edit mode
          if (_isEditMode) {
            _resolveNames();
          }
        });
      }
    } finally {
      if (mounted) setState(() => _isLoadingData = false);
    }
  }

  void _resolveNames() {
    if (_selectedAccountId != null && _selectedAccountName == null) {
      final acc = _accounts
          .where((a) => a.id == _selectedAccountId)
          .firstOrNull;
      _selectedAccountName = acc?.name;
    }
    if (_selectedTargetAccountId != null &&
        _selectedTargetAccountName == null) {
      final acc = _accounts
          .where((a) => a.id == _selectedTargetAccountId)
          .firstOrNull;
      _selectedTargetAccountName = acc?.name;
    }
    if (_selectedCardId != null && _selectedCardName == null) {
      final card =
          _cards.where((c) => c.id == _selectedCardId).firstOrNull;
      _selectedCardName = card?.name;
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _amountController.dispose();
    _descriptionController.dispose();
    _installmentsController.dispose();
    super.dispose();
  }

  void _formatCurrency(String value) {
    final digits = value.replaceAll(RegExp(r'[^\d]'), '');
    final number = (int.tryParse(digits) ?? 0) / 100;
    final formatted = _currencyFormat.format(number);
    if (_amountController.text != formatted) {
      _amountController.value = TextEditingValue(
        text: formatted,
        selection: TextSelection.collapsed(offset: formatted.length),
      );
    }
  }

  double _parseCurrency(String text) {
    final digits = text.replaceAll(RegExp(r'[^\d]'), '');
    return (int.tryParse(digits) ?? 0) / 100;
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime(2020),
      lastDate: DateTime(2100),
      builder: (context, child) => Theme(
        data: ThemeData.dark().copyWith(
          colorScheme: const ColorScheme.dark(
            primary: Color(0xFF00E676),
            surface: Color(0xFF1E2A3A),
          ),
        ),
        child: child!,
      ),
    );
    if (picked != null) setState(() => _selectedDate = picked);
  }

  Future<void> _selectCategory() async {
    final typeFilter = _mode == _TxMode.income
        ? TransactionType.RECEITA
        : TransactionType.DESPESA;
    final filtered =
        _categories.where((c) => c.type == typeFilter && c.parentId == null).toList();

    final selected = await showDialog<Category>(
      context: context,
      builder: (ctx) => _SelectorDialog<Category>(
        title: 'Selecionar Categoria',
        items: filtered,
        labelBuilder: (c) => c.name,
        iconBuilder: (c) => Icons.category,
      ),
    );
    if (selected != null) {
      setState(() {
        _selectedCategoryId = selected.id;
        _selectedCategoryName = selected.name;
      });
    }
  }

  Future<void> _selectAccount({bool isTarget = false}) async {
    final selected = await showDialog<Account>(
      context: context,
      builder: (ctx) => _SelectorDialog<Account>(
        title: isTarget ? 'Conta Destino' : 'Selecionar Conta',
        items: _accounts,
        labelBuilder: (a) => a.name,
        iconBuilder: (a) => Icons.account_balance_wallet,
      ),
    );
    if (selected != null) {
      setState(() {
        if (isTarget) {
          _selectedTargetAccountId = selected.id;
          _selectedTargetAccountName = selected.name;
        } else {
          _selectedAccountId = selected.id;
          _selectedAccountName = selected.name;
        }
      });
    }
  }

  Future<void> _selectCard() async {
    final selected = await showDialog<CreditCard>(
      context: context,
      builder: (ctx) => _SelectorDialog<CreditCard>(
        title: 'Selecionar Cartão',
        items: _cards,
        labelBuilder: (c) => c.name,
        iconBuilder: (c) => Icons.credit_card,
      ),
    );
    if (selected != null) {
      setState(() {
        _selectedCardId = selected.id;
        _selectedCardName = selected.name;
      });
    }
  }

  Transaction _buildTransaction() {
    TransactionType txType;
    switch (_mode) {
      case _TxMode.expense:
      case _TxMode.cardExpense:
        txType = TransactionType.DESPESA;
        break;
      case _TxMode.income:
        txType = TransactionType.RECEITA;
        break;
      case _TxMode.transfer:
        txType = TransactionType.TRANSFERENCIA;
        break;
    }

    return Transaction(
      id: widget.transactionToEdit?.id,
      name: _nameController.text.trim(),
      description: _descriptionController.text.trim().isNotEmpty
          ? _descriptionController.text.trim()
          : null,
      type: txType,
      amount: _parseCurrency(_amountController.text),
      date: _selectedDate.millisecondsSinceEpoch,
      paid: _isPaid,
      accountId: _mode == _TxMode.cardExpense ? null : _selectedAccountId,
      targetAccountId:
          _mode == _TxMode.transfer ? _selectedTargetAccountId : null,
      categoryId:
          _mode == _TxMode.transfer ? null : _selectedCategoryId,
      creditCardId:
          _mode == _TxMode.cardExpense ? _selectedCardId : null,
      isFixed: _isFixed,
      isRecurring: _isFixed,
      recurrenceFrequency: _isFixed ? _recurrenceFrequency : null,
      installments: _mode == _TxMode.cardExpense
          ? (int.tryParse(_installmentsController.text) ?? 1)
          : 1,
    );
  }

  Future<void> _save({bool addAnother = false}) async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);
    try {
      final service = ref.read(transactionServiceProvider);
      final tx = _buildTransaction();

      final success = _isEditMode
          ? await service.updateTransaction(
              widget.transactionToEdit!.id!, tx)
          : await service.saveTransaction(tx);

      if (!mounted) return;
      if (success) {
        if (addAnother) {
          _formKey.currentState!.reset();
          _nameController.clear();
          _amountController.text = _currencyFormat.format(0);
          _descriptionController.clear();
          _installmentsController.text = '1';
          setState(() {
            _selectedCategoryId = null;
            _selectedCategoryName = null;
            _isPaid = false;
          });
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Transação salva! Adicione outra.'),
              backgroundColor: Color(0xFF00E676),
            ),
          );
        } else {
          Navigator.of(context).pop(true);
        }
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao salvar transação'),
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
        title: const Text('Excluir transação',
            style: TextStyle(color: Colors.white)),
        content: const Text(
            'Tem certeza que deseja excluir esta transação?',
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
      final service = ref.read(transactionServiceProvider);
      final success = await service
          .deleteTransaction(widget.transactionToEdit!.id!);
      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao excluir transação'),
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
        maxHeight: MediaQuery.of(context).size.height * 0.95,
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
                          _buildModeSelector(),
                          const SizedBox(height: 20),
                          TextFormField(
                            controller: _nameController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Nome', Icons.edit),
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o nome'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _amountController,
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Valor', Icons.attach_money),
                            keyboardType: TextInputType.number,
                            onChanged: _formatCurrency,
                            validator: (v) =>
                                _parseCurrency(v ?? '') <= 0
                                    ? 'Informe o valor'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          _buildDatePicker(),
                          const SizedBox(height: 16),
                          SwitchListTile(
                            title: const Text('Pago',
                                style:
                                    TextStyle(color: Colors.white)),
                            value: _isPaid,
                            activeColor: const Color(0xFF00E676),
                            tileColor: const Color(0xFF1E2A3A),
                            shape: RoundedRectangleBorder(
                                borderRadius:
                                    BorderRadius.circular(12)),
                            onChanged: (v) =>
                                setState(() => _isPaid = v),
                          ),
                          const SizedBox(height: 16),
                          if (_mode != _TxMode.transfer)
                            _buildSelectorTile(
                              label: 'Categoria',
                              value: _selectedCategoryName,
                              icon: Icons.category,
                              onTap: _selectCategory,
                            ),
                          if (_mode != _TxMode.transfer)
                            const SizedBox(height: 12),
                          if (_mode == _TxMode.cardExpense)
                            _buildSelectorTile(
                              label: 'Cartão',
                              value: _selectedCardName,
                              icon: Icons.credit_card,
                              onTap: _selectCard,
                            )
                          else
                            _buildSelectorTile(
                              label: _mode == _TxMode.transfer
                                  ? 'Conta Origem'
                                  : 'Conta',
                              value: _selectedAccountName,
                              icon: Icons.account_balance_wallet,
                              onTap: () =>
                                  _selectAccount(isTarget: false),
                            ),
                          if (_mode == _TxMode.transfer) ...[
                            const SizedBox(height: 12),
                            _buildSelectorTile(
                              label: 'Conta Destino',
                              value: _selectedTargetAccountName,
                              icon: Icons.swap_horiz,
                              onTap: () =>
                                  _selectAccount(isTarget: true),
                            ),
                          ],
                          const SizedBox(height: 16),
                          _buildMoreDetailsToggle(),
                          if (_showMoreDetails) ...[
                            const SizedBox(height: 16),
                            _buildMoreDetailsSection(),
                          ],
                          const SizedBox(height: 24),
                          _buildActionButtons(),
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
            _isEditMode ? 'Editar Transação' : 'Nova Transação',
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

  Widget _buildModeSelector() {
    return Row(
      children: _TxMode.values.map((mode) {
        final selected = _mode == mode;
        final color = _modeColor(mode);
        return Expanded(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 3),
            child: GestureDetector(
              onTap: () => setState(() => _mode = mode),
              child: Container(
                padding: const EdgeInsets.symmetric(vertical: 10),
                decoration: BoxDecoration(
                  color: selected
                      ? color.withAlpha(40)
                      : const Color(0xFF1E2A3A),
                  borderRadius: BorderRadius.circular(10),
                  border: selected
                      ? Border.all(color: color, width: 1.5)
                      : null,
                ),
                alignment: Alignment.center,
                child: Text(
                  _modeLabels[mode]!,
                  style: TextStyle(
                    color: selected ? color : const Color(0xFF94A3B8),
                    fontWeight: FontWeight.w600,
                    fontSize: 12,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
            ),
          ),
        );
      }).toList(),
    );
  }

  Widget _buildDatePicker() {
    return GestureDetector(
      onTap: _pickDate,
      child: InputDecorator(
        decoration: _inputDecoration('Data', Icons.calendar_today),
        child: Text(
          _dateFormat.format(_selectedDate),
          style: const TextStyle(color: Colors.white),
        ),
      ),
    );
  }

  Widget _buildSelectorTile({
    required String label,
    required String? value,
    required IconData icon,
    required VoidCallback onTap,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: InputDecorator(
        decoration: _inputDecoration(label, icon),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              value ?? 'Selecionar...',
              style: TextStyle(
                color: value != null
                    ? Colors.white
                    : const Color(0xFF64748B),
              ),
            ),
            const Icon(Icons.chevron_right,
                color: Color(0xFF94A3B8), size: 20),
          ],
        ),
      ),
    );
  }

  Widget _buildMoreDetailsToggle() {
    return GestureDetector(
      onTap: () =>
          setState(() => _showMoreDetails = !_showMoreDetails),
      child: Container(
        padding: const EdgeInsets.symmetric(
            vertical: 12, horizontal: 16),
        decoration: BoxDecoration(
          color: const Color(0xFF1E2A3A),
          borderRadius: BorderRadius.circular(12),
        ),
        child: Row(
          children: [
            const Icon(Icons.tune,
                color: Color(0xFF94A3B8), size: 20),
            const SizedBox(width: 12),
            const Text('Mais detalhes',
                style: TextStyle(color: Color(0xFF94A3B8))),
            const Spacer(),
            Icon(
              _showMoreDetails
                  ? Icons.expand_less
                  : Icons.expand_more,
              color: const Color(0xFF94A3B8),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMoreDetailsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        TextFormField(
          controller: _descriptionController,
          style: const TextStyle(color: Colors.white),
          decoration: _inputDecoration('Descrição', Icons.notes),
          maxLines: 3,
          minLines: 1,
        ),
        const SizedBox(height: 16),
        SwitchListTile(
          title: const Text('Fixo/Recorrente',
              style: TextStyle(color: Colors.white)),
          subtitle: const Text(
              'Transação se repete automaticamente',
              style: TextStyle(
                  color: Color(0xFF94A3B8), fontSize: 12)),
          value: _isFixed,
          activeColor: const Color(0xFF00E676),
          tileColor: const Color(0xFF1E2A3A),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12)),
          onChanged: (v) => setState(() => _isFixed = v),
        ),
        if (_isFixed) ...[
          const SizedBox(height: 16),
          DropdownButtonFormField<RecurrenceFrequency>(
            value: _recurrenceFrequency,
            dropdownColor: const Color(0xFF1E2A3A),
            style: const TextStyle(color: Colors.white),
            decoration:
                _inputDecoration('Frequência', Icons.repeat),
            items: const [
              DropdownMenuItem(
                value: RecurrenceFrequency.DAILY,
                child: Text('Diário'),
              ),
              DropdownMenuItem(
                value: RecurrenceFrequency.WEEKLY,
                child: Text('Semanal'),
              ),
              DropdownMenuItem(
                value: RecurrenceFrequency.BIWEEKLY,
                child: Text('Quinzenal'),
              ),
              DropdownMenuItem(
                value: RecurrenceFrequency.MONTHLY,
                child: Text('Mensal'),
              ),
              DropdownMenuItem(
                value: RecurrenceFrequency.YEARLY,
                child: Text('Anual'),
              ),
            ],
            onChanged: (v) {
              if (v != null) {
                setState(() => _recurrenceFrequency = v);
              }
            },
          ),
        ],
        if (_mode == _TxMode.cardExpense) ...[
          const SizedBox(height: 16),
          TextFormField(
            controller: _installmentsController,
            style: const TextStyle(color: Colors.white),
            decoration: _inputDecoration(
                'Parcelas', Icons.format_list_numbered),
            keyboardType: TextInputType.number,
            validator: (v) {
              final n = int.tryParse(v ?? '');
              if (n == null || n < 1) return 'Mínimo 1';
              return null;
            },
          ),
        ],
      ],
    );
  }

  Widget _buildActionButtons() {
    final color = _modeColor(_mode);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        SizedBox(
          height: 50,
          child: ElevatedButton(
            onPressed: _isLoading ? null : () => _save(),
            style: ElevatedButton.styleFrom(
              backgroundColor: color,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            child: _isLoading
                ? const SizedBox(
                    width: 24,
                    height: 24,
                    child: CircularProgressIndicator(
                      color: Colors.white,
                      strokeWidth: 2,
                    ),
                  )
                : Text(
                    _isEditMode ? 'Salvar Alterações' : 'Salvar',
                    style: const TextStyle(
                        fontSize: 16, fontWeight: FontWeight.bold),
                  ),
          ),
        ),
        if (!_isEditMode) ...[
          const SizedBox(height: 10),
          SizedBox(
            height: 46,
            child: OutlinedButton(
              onPressed: _isLoading
                  ? null
                  : () => _save(addAnother: true),
              style: OutlinedButton.styleFrom(
                foregroundColor: color,
                side: BorderSide(color: color),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
              child: const Text('Salvar e Adicionar Outra',
                  style: TextStyle(
                      fontSize: 14, fontWeight: FontWeight.w600)),
            ),
          ),
        ],
      ],
    );
  }
}

/// Generic list selector dialog used for categories, accounts, cards.
class _SelectorDialog<T> extends StatelessWidget {
  final String title;
  final List<T> items;
  final String Function(T) labelBuilder;
  final IconData Function(T) iconBuilder;

  const _SelectorDialog({
    required this.title,
    required this.items,
    required this.labelBuilder,
    required this.iconBuilder,
  });

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: const Color(0xFF0A0F17),
      title: Text(title, style: const TextStyle(color: Colors.white)),
      content: SizedBox(
        width: double.maxFinite,
        child: items.isEmpty
            ? const Padding(
                padding: EdgeInsets.all(16),
                child: Text('Nenhum item disponível',
                    style: TextStyle(color: Color(0xFF94A3B8))),
              )
            : ListView.builder(
                shrinkWrap: true,
                itemCount: items.length,
                itemBuilder: (ctx, i) {
                  final item = items[i];
                  return ListTile(
                    leading: Icon(iconBuilder(item),
                        color: const Color(0xFF94A3B8)),
                    title: Text(labelBuilder(item),
                        style:
                            const TextStyle(color: Colors.white)),
                    onTap: () => Navigator.of(ctx).pop(item),
                  );
                },
              ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancelar',
              style: TextStyle(color: Color(0xFF94A3B8))),
        ),
      ],
    );
  }
}
