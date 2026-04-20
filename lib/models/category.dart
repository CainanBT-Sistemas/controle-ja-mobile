import 'enums.dart';

class Category {
  final String id;
  final String name;
  final TransactionType type;
  final String icon;
  final String color;
  final bool isDefault;
  final String? parentId;
  final List<Category> subCategories;
  final String? parentColor;

  const Category({
    required this.id,
    this.name = '',
    this.type = TransactionType.DESPESA,
    this.icon = 'category',
    this.color = '#94A3B8',
    this.isDefault = false,
    this.parentId,
    this.subCategories = const [],
    this.parentColor,
  });

  bool get hasChildren => subCategories.isNotEmpty;

  int get childrenCount => subCategories.length;

  Category copyWith({
    String? id,
    String? name,
    TransactionType? type,
    String? icon,
    String? color,
    bool? isDefault,
    String? parentId,
    List<Category>? subCategories,
    String? parentColor,
  }) {
    return Category(
      id: id ?? this.id,
      name: name ?? this.name,
      type: type ?? this.type,
      icon: icon ?? this.icon,
      color: color ?? this.color,
      isDefault: isDefault ?? this.isDefault,
      parentId: parentId ?? this.parentId,
      subCategories: subCategories ?? this.subCategories,
      parentColor: parentColor ?? this.parentColor,
    );
  }

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      type: json['categoryType'] != null
          ? TransactionType.fromJson(json['categoryType'] as String)
          : TransactionType.DESPESA,
      icon: json['icon'] as String? ?? 'category',
      color: json['color'] as String? ?? '#94A3B8',
      isDefault: json['isDefault'] as bool? ?? false,
      parentId: json['parentId'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'categoryType': type.toJson(),
      'icon': icon,
      'color': color,
      'isDefault': isDefault,
      'parentId': parentId,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Category && runtimeType == other.runtimeType && id == other.id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() =>
      'Category(id: $id, name: $name, type: $type, icon: $icon, '
      'color: $color, isDefault: $isDefault, parentId: $parentId)';
}
