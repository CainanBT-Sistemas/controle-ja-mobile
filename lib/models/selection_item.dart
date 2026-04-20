class SelectionItem {
  final String id;
  final String? name;
  final String? icon;
  final String? color;
  final String? parentColor;
  final dynamic originalObject;
  final bool isSubItem;
  final List<SelectionItem> subItems;
  final bool isSelected;
  final bool isExpanded;

  const SelectionItem({
    required this.id,
    this.name,
    this.icon,
    this.color,
    this.parentColor,
    this.originalObject,
    this.isSubItem = false,
    this.subItems = const [],
    this.isSelected = false,
    this.isExpanded = true,
  });

  bool get hasSubItems => subItems.isNotEmpty;

  SelectionItem copyWith({
    String? id,
    String? name,
    String? icon,
    String? color,
    String? parentColor,
    dynamic originalObject,
    bool? isSubItem,
    List<SelectionItem>? subItems,
    bool? isSelected,
    bool? isExpanded,
  }) {
    return SelectionItem(
      id: id ?? this.id,
      name: name ?? this.name,
      icon: icon ?? this.icon,
      color: color ?? this.color,
      parentColor: parentColor ?? this.parentColor,
      originalObject: originalObject ?? this.originalObject,
      isSubItem: isSubItem ?? this.isSubItem,
      subItems: subItems ?? this.subItems,
      isSelected: isSelected ?? this.isSelected,
      isExpanded: isExpanded ?? this.isExpanded,
    );
  }

  factory SelectionItem.fromJson(Map<String, dynamic> json) {
    return SelectionItem(
      id: json['id'] as String? ?? '',
      name: json['name'] as String?,
      icon: json['icon'] as String?,
      color: json['color'] as String?,
      parentColor: json['parentColor'] as String?,
      isSubItem: json['isSubItem'] as bool? ?? false,
      subItems: (json['subItems'] as List<dynamic>?)
              ?.map(
                  (e) => SelectionItem.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      isSelected: json['isSelected'] as bool? ?? false,
      isExpanded: json['isExpanded'] as bool? ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'icon': icon,
      'color': color,
      'parentColor': parentColor,
      'isSubItem': isSubItem,
      'subItems': subItems.map((e) => e.toJson()).toList(),
      'isSelected': isSelected,
      'isExpanded': isExpanded,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SelectionItem &&
          runtimeType == other.runtimeType &&
          id == other.id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() =>
      'SelectionItem(id: $id, name: $name, isSelected: $isSelected)';
}
