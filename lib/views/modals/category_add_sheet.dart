import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/theme.dart';
import '../../models/category.dart';
import '../../models/enums.dart';
import '../../providers/service_providers.dart';

class CategoryAddSheet extends ConsumerStatefulWidget {
  final String? categoryId;
  final String? parentId;
  final String? parentType;

  const CategoryAddSheet({
    super.key,
    this.categoryId,
    this.parentId,
    this.parentType,
  });

  @override
  ConsumerState<CategoryAddSheet> createState() => _CategoryAddSheetState();
}

class _CategoryAddSheetState extends ConsumerState<CategoryAddSheet> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();

  TransactionType _selectedType = TransactionType.DESPESA;
  String _selectedIcon = AppIconLists.categoryIcons.first;
  String _selectedColor = AppColors.availableColorHex[5];
  bool _isDefault = false;
  bool _isLoading = false;
  bool _isLoadingData = false;
  List<Category> _subCategories = [];

  bool get _isEditMode => widget.categoryId != null;
  bool get _isSubcategory => widget.parentId != null;

  @override
  void initState() {
    super.initState();
    if (_isSubcategory && widget.parentType != null) {
      _selectedType = TransactionType.fromJson(widget.parentType!);
    }
    if (_isEditMode) _loadCategory();
  }

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  Future<void> _loadCategory() async {
    setState(() => _isLoadingData = true);
    try {
      final service = ref.read(categoryServiceProvider);
      final category = await service.getCategoryById(widget.categoryId!);
      if (category != null && mounted) {
        _nameController.text = category.name;
        setState(() {
          _selectedType = category.type;
          _selectedIcon = category.icon;
          _selectedColor = category.color;
          _isDefault = category.isDefault;
        });
        // Load subcategories
        final all = await service.getCategories();
        if (mounted) {
          setState(() {
            _subCategories = all
                .where((c) => c.parentId == widget.categoryId)
                .toList();
          });
        }
      }
    } finally {
      if (mounted) setState(() => _isLoadingData = false);
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);
    try {
      final service = ref.read(categoryServiceProvider);
      final category = Category(
        id: widget.categoryId ?? '',
        name: _nameController.text.trim(),
        type: _selectedType,
        icon: _selectedIcon,
        color: _selectedColor,
        isDefault: _isDefault,
        parentId: _isSubcategory ? widget.parentId : null,
      );

      bool success;
      if (_isEditMode) {
        success = await service.updateCategory(
            widget.categoryId!, category.toJson());
      } else {
        final saved = await service.saveCategory(category);
        success = saved != null;
      }

      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao salvar categoria'),
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
        title: const Text('Excluir categoria',
            style: TextStyle(color: Colors.white)),
        content: const Text(
            'Tem certeza que deseja excluir esta categoria?',
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
      final service = ref.read(categoryServiceProvider);
      final success = await service.deleteCategory(widget.categoryId!);
      if (!mounted) return;
      if (success) {
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao excluir categoria'),
            backgroundColor: Color(0xFFFF5252),
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _addSubcategory() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => CategoryAddSheet(
        parentId: widget.categoryId,
        parentType: _selectedType.toJson(),
      ),
    ).then((result) {
      if (result == true) _loadCategory();
    });
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
                                'Nome da categoria', Icons.edit),
                            validator: (v) =>
                                (v == null || v.trim().isEmpty)
                                    ? 'Informe o nome'
                                    : null,
                          ),
                          const SizedBox(height: 16),
                          DropdownButtonFormField<TransactionType>(
                            value: _selectedType,
                            dropdownColor: const Color(0xFF1E2A3A),
                            style: const TextStyle(color: Colors.white),
                            decoration: _inputDecoration(
                                'Tipo', Icons.swap_vert),
                            items: const [
                              DropdownMenuItem(
                                value: TransactionType.DESPESA,
                                child: Text('Despesa'),
                              ),
                              DropdownMenuItem(
                                value: TransactionType.RECEITA,
                                child: Text('Receita'),
                              ),
                            ],
                            onChanged: _isSubcategory
                                ? null
                                : (v) {
                                    if (v != null) {
                                      setState(
                                          () => _selectedType = v);
                                    }
                                  },
                          ),
                          const SizedBox(height: 20),
                          _buildIconSelector(),
                          const SizedBox(height: 20),
                          _buildColorSelector(),
                          if (_isEditMode && !_isSubcategory) ...[
                            const SizedBox(height: 24),
                            _buildSubcategoriesSection(),
                          ],
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
    final title = _isSubcategory
        ? (_isEditMode ? 'Editar Subcategoria' : 'Nova Subcategoria')
        : (_isEditMode ? 'Editar Categoria' : 'Nova Categoria');
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 16, 12, 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(title,
              style: const TextStyle(
                  color: Colors.white,
                  fontSize: 20,
                  fontWeight: FontWeight.bold)),
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
          spacing: 10,
          runSpacing: 10,
          children: AppIconLists.categoryIcons.map((iconName) {
            final selected = _selectedIcon == iconName;
            return GestureDetector(
              onTap: () => setState(() => _selectedIcon = iconName),
              child: Container(
                width: 44,
                height: 44,
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
                  size: 22,
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

  Widget _buildSubcategoriesSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Subcategorias (${_subCategories.length})',
              style: const TextStyle(
                  color: Color(0xFF94A3B8), fontSize: 14),
            ),
            TextButton.icon(
              onPressed: _addSubcategory,
              icon: const Icon(Icons.add,
                  color: Color(0xFF00E676), size: 18),
              label: const Text('Adicionar',
                  style: TextStyle(color: Color(0xFF00E676))),
            ),
          ],
        ),
        if (_subCategories.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: 8),
            child: Text('Nenhuma subcategoria',
                style: TextStyle(
                    color: Color(0xFF64748B), fontSize: 13)),
          )
        else
          ...(_subCategories.map((sub) => ListTile(
                contentPadding: EdgeInsets.zero,
                leading: CircleAvatar(
                  radius: 16,
                  backgroundColor: AppColors.fromHex(sub.color),
                  child: Icon(
                    AppMaterialIcons.fromName(sub.icon),
                    color: Colors.white,
                    size: 16,
                  ),
                ),
                title: Text(sub.name,
                    style: const TextStyle(
                        color: Colors.white, fontSize: 14)),
                onTap: () {
                  showModalBottomSheet(
                    context: context,
                    isScrollControlled: true,
                    backgroundColor: Colors.transparent,
                    builder: (_) =>
                        CategoryAddSheet(categoryId: sub.id),
                  ).then((result) {
                    if (result == true) _loadCategory();
                  });
                },
              ))),
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
                _isEditMode ? 'Salvar Alterações' : 'Criar Categoria',
                style: const TextStyle(
                    fontSize: 16, fontWeight: FontWeight.bold),
              ),
      ),
    );
  }
}
