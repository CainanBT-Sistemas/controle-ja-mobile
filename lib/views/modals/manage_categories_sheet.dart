import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/theme.dart';
import '../../models/category.dart';
import '../../providers/categories_notifier.dart';
import 'category_add_sheet.dart';

class ManageCategoriesSheet extends ConsumerStatefulWidget {
  const ManageCategoriesSheet({super.key});

  @override
  ConsumerState<ManageCategoriesSheet> createState() =>
      _ManageCategoriesSheetState();
}

class _ManageCategoriesSheetState
    extends ConsumerState<ManageCategoriesSheet> {
  final Set<String> _expandedIds = {};

  @override
  void initState() {
    super.initState();
    Future.microtask(
      () =>
          ref.read(categoriesListNotifierProvider.notifier).loadCategories(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(categoriesListNotifierProvider);
    final categories = state.displayedCategories;

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
                  'Categorias',
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
          // Toggle tabs Despesas / Receitas
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
            child: Row(
              children: [
                _buildTab(
                  label: 'Despesas',
                  selected: state.showingExpenses,
                  onTap: () => ref
                      .read(categoriesListNotifierProvider.notifier)
                      .showExpenses(),
                ),
                const SizedBox(width: 12),
                _buildTab(
                  label: 'Receitas',
                  selected: !state.showingExpenses,
                  onTap: () => ref
                      .read(categoriesListNotifierProvider.notifier)
                      .showIncomes(),
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
                : categories.isEmpty
                    ? const Padding(
                        padding: EdgeInsets.all(48),
                        child: Text(
                          'Nenhuma categoria cadastrada',
                          style: TextStyle(
                            color: Color(0xFF94A3B8),
                            fontSize: 16,
                          ),
                        ),
                      )
                    : ListView.builder(
                        shrinkWrap: true,
                        itemCount: categories.length,
                        padding: const EdgeInsets.symmetric(vertical: 8),
                        itemBuilder: (context, index) =>
                            _buildCategoryTile(categories[index]),
                      ),
          ),
        ],
      ),
    );
  }

  Widget _buildTab({
    required String label,
    required bool selected,
    required VoidCallback onTap,
  }) {
    return Expanded(
      child: GestureDetector(
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 10),
          decoration: BoxDecoration(
            color: selected ? const Color(0xFF00E676) : const Color(0xFF1E2A3A),
            borderRadius: BorderRadius.circular(10),
          ),
          alignment: Alignment.center,
          child: Text(
            label,
            style: TextStyle(
              color: selected ? Colors.black : const Color(0xFF94A3B8),
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildCategoryTile(Category category) {
    final hasChildren = category.subCategories.isNotEmpty;
    final isExpanded = _expandedIds.contains(category.id);

    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        ListTile(
          leading: CircleAvatar(
            backgroundColor: AppColors.fromHex(category.color),
            child: Icon(
              AppMaterialIcons.fromName(category.icon),
              color: Colors.white,
              size: 20,
            ),
          ),
          title: Text(
            category.name,
            style: const TextStyle(color: Colors.white),
          ),
          subtitle: hasChildren
              ? Text(
                  '${category.subCategories.length} subcategoria(s)',
                  style: const TextStyle(
                    color: Color(0xFF94A3B8),
                    fontSize: 12,
                  ),
                )
              : null,
          trailing: hasChildren
              ? Icon(
                  isExpanded ? Icons.expand_less : Icons.expand_more,
                  color: const Color(0xFF94A3B8),
                )
              : null,
          onTap: () {
            if (hasChildren) {
              setState(() {
                if (isExpanded) {
                  _expandedIds.remove(category.id);
                } else {
                  _expandedIds.add(category.id);
                }
              });
            } else {
              _openAddSheet(context, categoryId: category.id);
            }
          },
          onLongPress: () => _openAddSheet(context, categoryId: category.id),
        ),
        if (hasChildren && isExpanded)
          ...category.subCategories.map((sub) => Padding(
                padding: const EdgeInsets.only(left: 32),
                child: ListTile(
                  leading: CircleAvatar(
                    radius: 16,
                    backgroundColor: AppColors.fromHex(
                      sub.parentColor ?? sub.color,
                    ),
                    child: Icon(
                      AppMaterialIcons.fromName(sub.icon),
                      color: Colors.white,
                      size: 16,
                    ),
                  ),
                  title: Text(
                    sub.name,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 14,
                    ),
                  ),
                  onTap: () =>
                      _openAddSheet(context, categoryId: sub.id),
                ),
              )),
      ],
    );
  }

  void _openAddSheet(BuildContext context, {String? categoryId}) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => CategoryAddSheet(categoryId: categoryId),
    ).then(
      (_) =>
          ref.read(categoriesListNotifierProvider.notifier).loadCategories(),
    );
  }
}

/// Helper to show this sheet.
void showManageCategoriesSheet(BuildContext context) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.transparent,
    builder: (_) => const ManageCategoriesSheet(),
  );
}
