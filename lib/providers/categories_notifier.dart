import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/error_handler.dart';
import '../models/category.dart';
import '../models/enums.dart';
import '../services/category_service.dart';
import 'service_providers.dart';

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/// Estado do CategoriesListNotifier.
///
/// Migrado de CategoriesViewModel.cs — lista de categorias com filtro
/// despesa/receita e hierarquia pai/subcategorias.
class CategoriesListState {
  final bool isLoading;
  final List<Category> allCategories;
  final bool showingExpenses;
  final UserFriendlyError? error;

  // Caches internos construídos a partir de allCategories.
  final List<Category> _expenseRoots;
  final List<Category> _incomeRoots;

  CategoriesListState({
    this.isLoading = false,
    this.allCategories = const [],
    this.showingExpenses = true,
    this.error,
    List<Category> expenseRoots = const [],
    List<Category> incomeRoots = const [],
  })  : _expenseRoots = expenseRoots,
        _incomeRoots = incomeRoots;

  /// Categorias raiz exibidas (conforme filtro ativo).
  List<Category> get displayedCategories =>
      showingExpenses ? _expenseRoots : _incomeRoots;

  CategoriesListState copyWith({
    bool? isLoading,
    List<Category>? allCategories,
    bool? showingExpenses,
    UserFriendlyError? error,
    bool clearError = false,
    List<Category>? expenseRoots,
    List<Category>? incomeRoots,
  }) {
    return CategoriesListState(
      isLoading: isLoading ?? this.isLoading,
      allCategories: allCategories ?? this.allCategories,
      showingExpenses: showingExpenses ?? this.showingExpenses,
      error: clearError ? null : (error ?? this.error),
      expenseRoots: expenseRoots ?? _expenseRoots,
      incomeRoots: incomeRoots ?? _incomeRoots,
    );
  }
}

// ---------------------------------------------------------------------------
// Notifier
// ---------------------------------------------------------------------------

/// Notifier para a lista de categorias.
///
/// Migrado de CategoriesViewModel.cs — carrega categorias, monta árvore
/// pai/subcategorias e alterna entre despesas e receitas.
class CategoriesListNotifier extends StateNotifier<CategoriesListState> {
  final CategoryService _categoryService;

  CategoriesListNotifier(this._categoryService)
      : super(CategoriesListState());

  /// Carrega todas as categorias e monta a hierarquia.
  Future<void> loadCategories() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final all = await _categoryService.getCategories();
      final tree = _buildTree(all);
      state = state.copyWith(
        allCategories: all,
        expenseRoots: tree.expenseRoots,
        incomeRoots: tree.incomeRoots,
      );
    } catch (e) {
      state = state.copyWith(error: ErrorHandler.parse(e));
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }

  /// Exibe categorias de despesa.
  void showExpenses() => state = state.copyWith(showingExpenses: true);

  /// Exibe categorias de receita.
  void showIncomes() => state = state.copyWith(showingExpenses: false);

  /// Exclui uma categoria e recarrega a lista.
  Future<bool> deleteCategory(String id) async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final success = await _categoryService.deleteCategory(id);
      if (success) {
        await loadCategories();
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

  // --- Privados ---

  /// Monta a árvore de categorias (pai → subcategorias).
  _CategoryTree _buildTree(List<Category> all) {
    // Separa raízes (sem parentId) e subcategorias.
    final roots = <Category>[];
    final childrenMap = <String, List<Category>>{};

    for (final cat in all) {
      if (cat.parentId == null || cat.parentId!.isEmpty) {
        roots.add(cat);
      } else {
        childrenMap.putIfAbsent(cat.parentId!, () => []).add(cat);
      }
    }

    // Associa subcategorias às raízes, herdando cor do pai.
    final builtRoots = roots.map((root) {
      final children = (childrenMap[root.id] ?? [])
          .map((sub) => sub.copyWith(parentColor: root.color))
          .toList();
      return root.copyWith(subCategories: children);
    }).toList();

    // Separa por tipo.
    final expenseRoots = builtRoots
        .where((c) => c.type == TransactionType.DESPESA)
        .toList();
    final incomeRoots = builtRoots
        .where((c) => c.type == TransactionType.RECEITA)
        .toList();

    return _CategoryTree(expenseRoots: expenseRoots, incomeRoots: incomeRoots);
  }
}

/// Resultado auxiliar da montagem da árvore.
class _CategoryTree {
  final List<Category> expenseRoots;
  final List<Category> incomeRoots;

  const _CategoryTree({
    required this.expenseRoots,
    required this.incomeRoots,
  });
}

// ---------------------------------------------------------------------------
// Providers
// ---------------------------------------------------------------------------

/// Provider do CategoriesListNotifier.
final categoriesListNotifierProvider =
    StateNotifierProvider<CategoriesListNotifier, CategoriesListState>((ref) {
  final service = ref.watch(categoryServiceProvider);
  return CategoriesListNotifier(service);
});
